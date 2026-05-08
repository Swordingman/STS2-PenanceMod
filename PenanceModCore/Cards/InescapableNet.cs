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
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class InescapableNet : PenanceBaseCard
{
    public InescapableNet() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Net-Barrier", 12m),
        new DynamicVar("Net-Draw", 1m) 
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int barrierGain = DynamicVars["Net-Barrier"].IntValue;
        int drawGain = DynamicVars["Net-Draw"].IntValue;

        await PowerCmd.Apply<InescapableNetPower>(choiceContext, Owner.Creature, barrierGain, Owner.Creature, this);

        var power = Owner.Creature.GetPower<InescapableNetPower>();
        if (power != null)
        {
            power.DynamicVars["draw"].UpgradeValueBy(drawGain);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Net-Barrier"].UpgradeValueBy(3);
    }
}