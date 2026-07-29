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
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class LastStand : PenanceBaseCard
{
    public LastStand() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("LastStand-Barrier", 24m).WithTooltip("PENANCEMOD-BARRIER"),
        new DynamicVar("LastStand-Ceasefire", 2m).WithTooltip("PENANCEMOD-CEASE_FIRE")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int barrierAmount = vars.Count > 0 ? vars[0].IntValue : 30;
        int ceasefireAmount = vars.Count > 1 ? vars[1].IntValue : 3;

        var creature = Owner.Creature;

        await PowerCmd.Apply<BarrierPower>(choiceContext,creature, barrierAmount, creature, this);

        await Cmd.Wait(0.15f);

        await PowerCmd.Apply<CeasefirePower>(choiceContext,creature, ceasefireAmount, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LastStand-Barrier"].UpgradeValueBy(2);
        DynamicVars["LastStand-Ceasefire"].UpgradeValueBy(-1);
    }
}
