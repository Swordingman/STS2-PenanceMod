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
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class TippingScales : PenanceBaseCard
{
    public TippingScales() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Tipping-JudgeCost", 2m).WithTooltip("PENANCEMOD-JUDGEMENT"),
        new DynamicVar("Tipping-Str", 1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        if (creature == null) return;

        var judgePower = creature.GetPower<JudgementPower>();
        if (judgePower != null && judgePower.Amount > 0)
        {
            int currentJudgement = judgePower.Amount;
            int judgeCost = DynamicVars["Tipping-JudgeCost"].IntValue;
            int strPerUnit = DynamicVars["Tipping-Str"].IntValue;

            int multiplier = currentJudgement / judgeCost;

            if (multiplier > 0)
            {
                int totalStr = multiplier * strPerUnit;
                await PowerCmd.Apply<StrengthPower>(choiceContext,creature, totalStr, creature, this);
                await Cmd.Wait(0.1f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Tipping-Str"].UpgradeValueBy(1);
    }
}
