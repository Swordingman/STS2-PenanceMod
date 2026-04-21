using Godot;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace PenanceMod.PenanceModCode.Powers;

public class BarrierPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(BarrierPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(BarrierPower)}.png";

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 1. 安全检查：如果受伤的不是自己，或者伤害小于等于0，直接放行
        if (target != Owner || amount <= 0 || props.HasFlag(ValueProp.Unblockable)) 
            return amount;
        
        decimal damageBlocked = 0;

        // 2. 核心抵扣计算
        if (amount >= Amount)
        {
            // 【情况A：屏障破碎】
            damageBlocked = Amount;
            decimal remainingDamage = amount - Amount;
            
            // 将层数归零（PowerModel 内部的 SetAmount 方法会自动触发更新）
            SetAmount(0); 

            // 触发破碎特效
            // var wrath = Owner.GetPower<SilenceWrathPower>();
            // wrath?.OnBarrierBroken();

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
            // var asceticism = Owner.GetPower<AsceticismPower>();
            // asceticism?.OnBarrierDamaged();

            // // var guardian = Owner.GetPower<GuardianOfTheLawPower>();
            // guardian?.OnBarrierDamaged();
            
            // 裁决
            // if (dealer != null && dealer != Owner)
            // {
            //     // var judgement = Owner.GetPower<JudgementPower>();
            //     judgement?.OnBarrierDamaged(dealer);
            // }
        }

        // 4. 返回处理后的最终伤害值给游戏引擎去结算
        return amount;
    }
}