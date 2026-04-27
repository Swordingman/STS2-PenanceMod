using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.ValueProps;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Tempering : PenanceBaseCard
{
    public Tempering() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Tempering-Barrier", 12m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        if (creature == null) return;

        var selfDamageVar = new DamageVar(3, ValueProp.Unblockable);
        await CreatureCmd.Damage(choiceContext, creature, selfDamageVar, this);

        await Cmd.Wait(0.1f);

        int barrierAmt = DynamicVars["Tempering-Barrier"].IntValue;
        await ApplyBarrier(creature, barrierAmt);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Tempering-Barrier"].UpgradeValueBy(3);
    }
}