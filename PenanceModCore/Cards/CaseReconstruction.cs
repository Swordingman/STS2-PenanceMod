using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class CaseReconstruction : PenanceBaseCard
{
    public CaseReconstruction() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("CaseRecon-Scry", 5m),
        new DynamicVar("CaseRecon-Draw", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int scryAmount = vars.Count > 0 ? vars[0].IntValue : 5;
        int drawAmount = vars.Count > 1 ? vars[1].IntValue : 2;

        // ==========================================
        // 1. 手搓“预见”
        // ==========================================
        var drawPile = PileType.Draw.GetPile(Owner);
        var topCards = drawPile.Cards.Take(scryAmount).ToList();

        if (topCards.Count > 0)
        {
            var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, topCards.Count);

            IEnumerable<CardModel> cardsToDiscard = await CardSelectCmd.FromSimpleGrid(
                choiceContext, 
                topCards, 
                Owner, 
                prefs
            );

            if (cardsToDiscard != null && cardsToDiscard.Any())
            {
                foreach (var card in cardsToDiscard)
                {
                    // 将选中的牌移入弃牌堆
                    await CardPileCmd.Add(card, PileType.Discard);
                }
            }
        }

        // ==========================================
        // 2. 抽牌
        // ==========================================
        await CardPileCmd.Draw(choiceContext, drawAmount, Owner);
    }

    protected override void OnUpgrade()
    {
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 1)
        {
            vars[1].UpgradeValueBy(1);
        }
    }
}