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

            var wrath = owner.GetPower<SilenceWrathPower>();
            wrath?.OnBarrierBroken();

            amount = remainingDamage;
        }
        else
        {
            damageBlocked = amount;
            SetAmount(Amount - (int)amount);
            amount = 0;
        }

        // 3. 只要抵挡了伤害，就触发通用受损判定
        if (damageBlocked > 0)
        {
            SfxCmd.Play("event:/sfx/block_hit");

            Node? vfxContainer = owner.GetVfxContainer();
            if (vfxContainer != null)
            {
                vfxContainer.AddChildSafely(NBlockSparkVfx.Create(owner));
                vfxContainer.AddChildSafely(NDamageBlockedVfx.Create(owner));
            }

            NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);

            var asceticism = owner.GetPower<AsceticismPower>();
            asceticism?.OnBarrierDamaged();

            var guardian = owner.GetPower<GuardianOfTheLawPower>();
            guardian?.OnBarrierDamaged();

            if (dealer != null && dealer != owner)
            {
                var wrath = owner.GetPower<SilenceWrathPower>();
                wrath?.OnBarrierDamaged(dealer);

                var judgement = owner.GetPower<JudgementPower>();
                judgement?.OnBarrierDamaged(dealer);
            }
        }

        return amount;
    }

    
}