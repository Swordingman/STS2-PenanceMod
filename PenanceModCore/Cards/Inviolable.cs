using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Inviolable : PenanceBaseCard
{
    public Inviolable() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    // 🌟 注册消耗关键字
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Inv-Barrier", 28m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 直接获得巨量屏障
        await ApplyBarrier(Owner.Creature, DynamicVars["Inv-Barrier"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Inv-Barrier"].UpgradeValueBy(7);
    }
}
