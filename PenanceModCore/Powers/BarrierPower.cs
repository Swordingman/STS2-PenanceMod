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

        // 处理屏障破裂事件
        if (_pendingBarrierBroken)
        {
            _pendingBarrierBroken = false;
            var wrath = owner.GetPower<SilenceWrathPower>();
            // 注意：如果你的 OnBarrierBroken 里面也有打伤害的逻辑，它也必须改成 async/await！
            wrath?.OnBarrierBroken(); 
        }

        // 处理裁决反击事件
        if (dealer != null && _pendingJudgementTargets.Contains(dealer))
        {
            // 划掉名字，防止重复触发
            _pendingJudgementTargets.Remove(dealer);

            // 触发其他非伤害类的能力效果
            var asceticism = owner.GetPower<AsceticismPower>();
            asceticism?.OnBarrierDamaged();

            var guardian = owner.GetPower<GuardianOfTheLawPower>();
            guardian?.OnBarrierDamaged();

            var wrath = owner.GetPower<SilenceWrathPower>();
            wrath?.OnBarrierDamaged(dealer);

            // 安全地触发裁决反击！把游戏真实的 choiceContext 传过去
            var judgement = owner.GetPower<JudgementPower>();
            if (judgement != null && judgement.Amount > 0)
            {
                await judgement.TriggerJudgementDamageAsync(dealer, choiceContext);
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
            if (chapterRelic != null && PenanceConfig.EnabledChallenges.Contains(1))
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
        // 1. 确保是当前能力拥有者的回合开始了
        if (Owner != null && Owner == player.Creature)
        {
            // 2. 尝试获取苦修之章遗物实例
            var chapterRelic = player.GetRelic<ChapterOfPenance>();

            // 3. 如果遗物存在，且启用了挑战2
            if (chapterRelic != null && PenanceConfig.EnabledChallenges.Contains(2))
            {
                if (this.Amount > 0)
                {
                    // 闪烁遗物提示玩家触发了衰减
                    chapterRelic.Flash();

                    // C# 整数除法自动向下取整，比如 5 / 2 = 2。我们减去这部分，保留 3
                    int decayAmount = this.Amount / 2;
                    this.SetAmount(this.Amount - decayAmount);
                }
            }
        }
    }
}