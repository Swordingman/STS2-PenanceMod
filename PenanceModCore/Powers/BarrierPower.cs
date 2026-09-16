using System;
using Godot;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using PenanceMod.PenanceModCode.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using System.Linq;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;

namespace PenanceMod.PenanceModCode.Powers;

public class BarrierPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(BarrierPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(BarrierPower)}.png";

    private readonly HashSet<Creature> _pendingJudgementTargets = new();
    private bool _pendingBarrierBroken = false;

    // ModifyHpLostBeforeOstyLate 只能做纯计算。
    // 真正的伤害管线会紧接着调用 AfterModifyingHpLostBeforeOsty，
    // 而 UI / Forecast 可以只调用 Modify 阶段进行预览。
    // 因此把计算结果暂存在这里，等提交阶段再真正修改屏障。
    private BarrierDamageResolver.Resolution? _pendingBarrierResolution;

    // 裁决可能跨玩家/跨 BarrierPower 实例产生嵌套伤害，所以重入锁必须按整场 CombatState 共享。
    // ConditionalWeakTable 会随 CombatState 回收，不会把上一场战斗的状态带到下一场。
    private sealed class JudgementResolutionState
    {
        public int Depth;
    }

    private static readonly ConditionalWeakTable<ICombatState, JudgementResolutionState> JudgementStates = new();

    private static JudgementResolutionState GetJudgementState(ICombatState combatState) =>
        JudgementStates.GetValue(combatState, static _ => new JudgementResolutionState());

    private bool IsResolvingJudgement
    {
        get
        {
            var combatState = Owner?.CombatState;
            return combatState != null && GetJudgementState(combatState).Depth > 0;
        }
    }

    // ==========================================
    // 阶段1：纯计算阶段
    // 可以被真实伤害和伤害预览安全地重复调用
    // ==========================================
    public override decimal ModifyHpLostBeforeOstyLate(
        Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 每次新的计算都先清掉上一份缓存。
        // Forecast 留下的缓存不能污染后续真实伤害。
        _pendingBarrierResolution = null;

        var owner = Owner;
        if (owner == null || target != owner)
            return amount;

        var resolution = BarrierDamageResolver.Calculate(
            owner,
            Amount,
            amount,
            props,
            dealer,
            IsResolvingJudgement);

        if (!resolution.Absorbed)
            return amount;

        // 这里只缓存，绝不 SetAmount / 播 VFX / 登记裁决。
        _pendingBarrierResolution = resolution;

        return resolution.RemainingHpLoss;
    }

    // ==========================================
    // 阶段2：真实伤害提交阶段
    //
    // CreatureCmd.Damage 在 ModifyHpLostBeforeOsty 之后会调用这里。
    // 单纯的 UI Forecast 不应该走到这里，因此不会真的消耗屏障。
    // ==========================================
    public override Task AfterModifyingHpLostBeforeOsty()
    {
        if (_pendingBarrierResolution is not { } resolution)
            return Task.CompletedTask;

        // 先清缓存，避免下面任何同步/嵌套逻辑误用这次结果。
        _pendingBarrierResolution = null;

        var owner = Owner;
        if (owner == null || !resolution.Absorbed)
            return Task.CompletedTask;

        if (resolution.BarrierConsumed > 0)
        {
            SetAmount(Math.Max(0, Amount - resolution.BarrierConsumed));
        }

        if (resolution.BarrierBroken)
        {
            _pendingBarrierBroken = true;
        }

        if (resolution.JudgementTarget != null)
        {
            _pendingJudgementTargets.Add(resolution.JudgementTarget);
        }

        PlayBarrierHitEffects(owner);

        return Task.CompletedTask;
    }

    private static void PlayBarrierHitEffects(Creature owner)
    {
        SfxCmd.Play("event:/sfx/block_hit");

        Node? vfxContainer = owner.GetVfxContainer();
        if (vfxContainer != null)
        {
            vfxContainer.AddChildSafely(NBlockSparkVfx.Create(owner));
            vfxContainer.AddChildSafely(NDamageBlockedVfx.Create(owner));
        }

        NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
    }

    // ==========================================
    // 阶段3：受击结算阶段
    // 处理破盾联动与裁决
    // ==========================================
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        var owner = Owner;
        if (owner == null || target != owner) return;

        if (_pendingBarrierBroken)
        {
            _pendingBarrierBroken = false;

            var wrath = owner.GetPower<SilenceWrathPower>();
            if (wrath != null) await wrath.OnBarrierBroken(choiceContext);
        }

        // 荆棘、Debuff 等虽然会正常扣屏障，但没有被登记为裁决目标，
        // 因此会在这里直接结束，不触发后续“屏障受到攻击”类裁决联动。
        if (dealer == null || !_pendingJudgementTargets.Remove(dealer)) return;

        var asceticism = owner.GetPower<AsceticismPower>();
        asceticism?.OnBarrierDamaged();

        var guardian = owner.GetPower<GuardianOfTheLawPower>();
        guardian?.OnBarrierDamaged();

        var silenceWrath = owner.GetPower<SilenceWrathPower>();
        if (silenceWrath != null) await silenceWrath.OnBarrierDamaged(choiceContext, dealer);

        await TriggerAllJudgementsAsync(dealer, choiceContext);
    }

    private async Task TriggerAllJudgementsAsync(Creature dealer, PlayerChoiceContext choiceContext)
    {
        var combatState = Owner?.CombatState;
        if (combatState == null || !dealer.IsAlive) return;

        var resolutionState = GetJudgementState(combatState);
        if (resolutionState.Depth > 0) return;

        resolutionState.Depth++;

        try
        {
            var playerCreatures = combatState.PlayerCreatures.ToArray();

            foreach (var playerCreature in playerCreatures)
            {
                if (!dealer.IsAlive) break;

                var judgement = playerCreature.GetPower<JudgementPower>();
                if (judgement == null || judgement.Amount <= 0) continue;

                choiceContext.PushModel(judgement);

                try
                {
                    await judgement.TriggerJudgementDamageAsync(dealer, choiceContext);
                }
                finally
                {
                    choiceContext.PopModel(judgement);
                    judgement.InvokeExecutionFinished();
                }
            }
        }
        finally
        {
            resolutionState.Depth--;
        }
    }

    // ==========================================
    // 挑战1：屏障上限等同于最大生命值
    // ==========================================
    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || Owner == null || !Owner.IsPlayer || Owner.Player == null) return Task.CompletedTask;

        var chapterRelic = Owner.Player.GetRelic<ChapterOfPenance>();
        if (chapterRelic == null || !chapterRelic.HasChallenge(1)) return Task.CompletedTask;

        if (Amount > Owner.MaxHp)
        {
            SetAmount(Owner.MaxHp);
            chapterRelic.Flash();
        }

        return Task.CompletedTask;
    }

    // ==========================================
    // 挑战2：回合开始时屏障衰减50%
    // ==========================================
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        MegaCrit.Sts2.Core.Logging.Log.Info($"[PenanceMod] 回合开始钩子触发了！当前屏障层数: {Amount}");

        if (Owner == null || Owner != player.Creature) return Task.CompletedTask;

        var chapterRelic = player.GetRelic<ChapterOfPenance>();

        if (chapterRelic == null)
        {
            MegaCrit.Sts2.Core.Logging.Log.Info("[PenanceMod] 报错：玩家身上找不到苦修之章遗物！");
            return Task.CompletedTask;
        }

        MegaCrit.Sts2.Core.Logging.Log.Info($"[PenanceMod] 找到遗物。它身上记录的挑战是: '{chapterRelic.SavedChallenges}'");

        if (!chapterRelic.HasChallenge(2))
        {
            MegaCrit.Sts2.Core.Logging.Log.Info("[PenanceMod] 没有挑战2，跳过衰减。");
            return Task.CompletedTask;
        }

        MegaCrit.Sts2.Core.Logging.Log.Info("[PenanceMod] 判定通过！开始执行 50% 衰减...");

        if (Amount <= 0) return Task.CompletedTask;

        chapterRelic.Flash();

        int decayAmount = Amount / 2;
        SetAmount(Amount - decayAmount);

        return Task.CompletedTask;
    }
}
