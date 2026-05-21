using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class InescapableNet : PenanceBaseCard
{
    private const string TriggerKey = "InescapableNet-Triggers";

    public InescapableNet() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(TriggerKey, 2m)
            .WithTooltip("PENANCEMOD-JUDGEMENT")
            .WithTooltip("PENANCEMOD-THORN_AURA")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner?.Creature;
        if (creature == null)
        {
            return;
        }

        await PowerCmd.Apply<InescapableNetPower>(
            choiceContext,
            creature,
            DynamicVars[TriggerKey].IntValue,
            creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars[TriggerKey].UpgradeValueBy(1);
    }
}
