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

namespace PenanceMod.PenanceModCode.Powers;

public class BarrierPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(BarrierPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(BarrierPower)}.png";

    public override decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 1. 安全检查：如果受伤的不是自己，或者伤害小于等于0，直接放行
        if (target != Owner || amount <= 0 || props.HasFlag(ValueProp.Unblockable) || Amount <= 0) 
            return amount;
        
        decimal damageBlocked;

        // 2. 核心抵扣计算
        if (amount >= Amount)
        {
            // 【情况A：屏障破碎】
            damageBlocked = Amount;
            decimal remainingDamage = amount - Amount;
            
            // 将层数归零（PowerModel 内部的 SetAmount 方法会自动触发更新）
            SetAmount(0); 

            // 触发破碎特效
            var wrath = Owner.GetPower<SilenceWrathPower>();
            wrath?.OnBarrierBroken();

            amount = remainingDamage; // 剩余伤害继续扣除玩家血量
        }
        else
        {
            // 【情况B：屏障幸存】
            damageBlocked = amount;
            SetAmount(Amount - (int)amount); // 扣除对应层数
            amount = 0; // 伤害被完全吸收，不再扣血
        }

        // 3. 只要抵挡了伤害，就触发通用受损判定
        if (damageBlocked > 0)
        {
            // 1. 官方正宗格挡音效！
            SfxCmd.Play("event:/sfx/block_hit");

            // 2. 官方正宗格挡火花与“被格挡”特效！
            Node vfxContainer = Owner.GetVfxContainer();
            if (vfxContainer != null)
            {
                // 产生格挡蓝光火花
                vfxContainer.AddChildSafely(NBlockSparkVfx.Create(Owner));
                // 产生“被格挡”的特殊视觉反馈
                vfxContainer.AddChildSafely(NDamageBlockedVfx.Create(Owner));
            }

            // 3. 屏幕微微震动（也是源码里的原话）
            NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);

            var asceticism = Owner.GetPower<AsceticismPower>();
            asceticism?.OnBarrierDamaged();

            var guardian = Owner.GetPower<GuardianOfTheLawPower>();
            guardian?.OnBarrierDamaged();
            
            if (dealer != null && dealer != Owner)
            {
                var warth = Owner.GetPower<SilenceWrathPower>();
                warth?.OnBarrierDamaged(dealer);
                
                var judgement = Owner.GetPower<JudgementPower>();
                judgement?.OnBarrierDamaged(dealer);
            }
        }

        // 4. 返回处理后的最终伤害值给游戏引擎去结算
        return amount;
    }

    
}