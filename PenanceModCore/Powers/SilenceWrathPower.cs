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

    public async Task OnBarrierDamaged(PlayerChoiceContext choiceContext, Creature attacker)
    {
        if (attacker != Owner && Amount > 0)
        {
            Flash();

            await PowerCmd.Apply<WeakPower>(choiceContext, attacker, Amount, Owner, null);
        }
    }

    public async Task OnBarrierBroken(PlayerChoiceContext choiceContext)
    {
        if (Amount > 0)
        {
            Flash();

            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
        }
    }
}