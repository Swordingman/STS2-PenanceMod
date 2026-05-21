using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Unyield : PenanceBaseCard
{
    protected override bool HasEnergyCostX => true;

    public Unyield() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Unyield-XB", 8m).WithTooltip("PENANCEMOD-BARRIER"),
        new DynamicVar("Unyield-XM", 5m)
            .WithTooltip("PENANCEMOD-JUDGEMENT")
            .WithTooltip("PENANCEMOD-THORN_AURA")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        if (creature == null) return;

        int xValue = ResolveEnergyXValue();

        int barrierMultiplier = DynamicVars["Unyield-XB"].IntValue;
        int magicMultiplier = DynamicVars["Unyield-XM"].IntValue;

        int totalXBarrier = xValue * barrierMultiplier;
        int totalXMagic = xValue * magicMultiplier;

        if (totalXBarrier > 0)
        {
            int finalBarrier = IsUpgraded ? totalXBarrier + 11 : totalXBarrier + 6;
            await ApplyBarrier(creature, finalBarrier);
            await Cmd.Wait(0.1f);
        }

        if (totalXMagic > 0)
        {
            int finalMagic = IsUpgraded ? totalXMagic + 8 : totalXMagic + 3;
            await ApplyJudgement(creature, finalMagic);
            await ApplyThornAura(creature, finalMagic);
            await Cmd.Wait(0.1f);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Unyield-XB"].UpgradeValueBy(1);
        DynamicVars["Unyield-XM"].UpgradeValueBy(1);
    }
}
