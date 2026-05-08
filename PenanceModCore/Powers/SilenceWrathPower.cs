using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace PenanceMod.PenanceModCode.Powers;

public class SilenceWrathPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(SilenceWrathPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(SilenceWrathPower)}.png";

    public void OnBarrierDamaged(Creature attacker)
    {
        if (attacker != null && attacker != Owner && Amount > 0)
        {
            Flash(); 
            // 挨打给虚弱，层数为 Amount
            _ = PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), attacker, Amount, Owner, null);
        }
    }

    public void OnBarrierBroken()
    {
        if (Amount > 0)
        {
            Flash(); 
            // 盾破给力量，数值也是 Amount
            _ = PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
        }
    }
}