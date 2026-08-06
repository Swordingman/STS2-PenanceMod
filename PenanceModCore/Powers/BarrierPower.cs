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
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using PenanceMod.PenanceModCode.Relics;
using PenanceMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using System.Linq;

namespace PenanceMod.PenanceModCode.Powers;

public class BarrierPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(BarrierPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(BarrierPower)}.png";

    // 【新增】：用于记录在计算阶段打破屏障/攻击屏障的敌人，以便在结算阶段进行反击
    private HashSet<Creature> _pendingJudgementTargets = new HashSet<Creature>();
    private bool _pendingBarrierBroken = false;

    // ==========================================
    // 阶段1：纯计算阶段（绝对不执行带动作的指令）
    // ==========================================
    public override decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        var owner = Owner;

        // 1. 安全检查
        if (owner == null || target != owner || amount <= 0 || props.HasFlag(ValueProp.Unblockable) || Amount <= 0)
            return amount;

        decimal damageBlocked;

        // 2. 核心抵扣计算
        if (amount >= Amount)
        {
            damageBlocked = Amount;
            decimal remainingDamage = amount - Amount;

            SetAmount(0);
            
            // 记录屏障被打破，留给结算阶段处理
            _pendingBarrierBroken = true;

            amount = remainingDamage;
        }
        else
        {
            damageBlocked = amount;
            SetAmount(Amount - (int)amount);
            amount = 0;
        }

        // 3. 只要抵挡了伤害，播放特效并记录攻击者
        if (damageBlocked > 0)
        {
            // 播放音效和特效（这些是不进入Action队列的视觉表现，可以放在这里）
            SfxCmd.Play("event:/sfx/block_hit");

            Node? vfxContainer = owner.GetVfxContainer();
            if (vfxContainer != null)
            {
                vfxContainer.AddChildSafely(NBlockSparkVfx.Create(owner));
                vfxContainer.AddChildSafely(NDamageBlockedVfx.Create(owner));
            }

            NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);

            // 记录攻击者，留给下面的 AfterDamageReceived 真正进行排队反击
            if (dealer != null && dealer != owner)
            {
                _pendingJudgementTargets.Add(dealer);
            }
        }

        return amount;
    }

    // ==========================================
    // 阶段2：受击结算阶段（安全使用 await 执行动作）
    // ==========================================
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        var owner = Owner;
        if (owner == null || target != owner) return;

        if (_pendingBarrierBroken)
        {
            _pendingBarrierBroken = false;
            var wrath = owner.GetPower<SilenceWrathPower>();
            wrath?.OnBarrierBroken();
        }

        if (dealer == null || !_pendingJudgementTargets.Contains(dealer)) return;

        _pendingJudgementTargets.Remove(dealer);

        var asceticism = owner.GetPower<AsceticismPower>();
        asceticism?.OnBarrierDamaged();

        var guardian = owner.GetPower<GuardianOfTheLawPower>();
        guardian?.OnBarrierDamaged();

        var silenceWrath = owner.GetPower<SilenceWrathPower>();
        silenceWrath?.OnBarrierDamaged(dealer);

        await TriggerAllJudgementsAsync(dealer, choiceContext);
    }

    private async Task TriggerAllJudgementsAsync(Creature dealer, PlayerChoiceContext choiceContext)
    {
        var combatState = Owner?.CombatState;
        if (combatState == null) return;

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

    // ==========================================
    // 🌟 挑战 1：屏障上限等同于最大生命值
    // ==========================================
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // 1. 必须确保变动的是“我们自己”这个屏障能力，且拥有者是玩家
        if (power == this && Owner != null && Owner.IsPlayer && Owner.Player != null)
        {
            // 2. 尝试获取苦修之章遗物实例
            var chapterRelic = Owner.Player.GetRelic<ChapterOfPenance>();

            // 3. 如果遗物存在，且启用了挑战1
            if (chapterRelic != null && chapterRelic.HasChallenge(1))
            {
                // 如果当前层数超过了最大生命值，强制回调
                if (this.Amount > Owner.MaxHp)
                {
                    this.SetAmount(Owner.MaxHp);
                    
                    // 直接使用刚才获取的实例闪烁，省去再次查找的开销
                    chapterRelic.Flash();
                }
            }
        }
    }

    // ==========================================
    // 🌟 挑战 2：回合开始时屏障衰减 50%
    // ==========================================
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        MegaCrit.Sts2.Core.Logging.Log.Info($"[PenanceMod] 回合开始钩子触发了！当前屏障层数: {this.Amount}");

        if (Owner != null && Owner == player.Creature)
        {
            var chapterRelic = player.GetRelic<ChapterOfPenance>();
            
            if (chapterRelic == null)
            {
                MegaCrit.Sts2.Core.Logging.Log.Info($"[PenanceMod] 报错：玩家身上找不到苦修之章遗物！");
                return;
            }

            MegaCrit.Sts2.Core.Logging.Log.Info($"[PenanceMod] 找到遗物。它身上记录的挑战是: '{chapterRelic.SavedChallenges}'");

            if (chapterRelic.HasChallenge(2))
            {
                MegaCrit.Sts2.Core.Logging.Log.Info($"[PenanceMod] 判定通过！开始执行 50% 衰减...");
                if (this.Amount > 0)
                {
                    chapterRelic.Flash();
                    int decayAmount = this.Amount / 2;
                    this.SetAmount(this.Amount - decayAmount);
                }
            }
            else
            {
                MegaCrit.Sts2.Core.Logging.Log.Info($"[PenanceMod] 没有挑战2，跳过衰减。");
            }
        }
    }
}