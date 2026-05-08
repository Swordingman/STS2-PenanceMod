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
public class GlowOfSuffering : PenanceBaseCard
{
    public GlowOfSuffering() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Glow-Mult", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int displayMultiplier = DynamicVars["Glow-Mult"].IntValue;
        int powerStacksToApply = displayMultiplier * 100;
        await PowerCmd.Apply<GlowOfSufferingPower>(choiceContext, Owner.Creature, powerStacksToApply, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            DynamicVars["Glow-Mult"].UpgradeValueBy(1);
        }
    }
}