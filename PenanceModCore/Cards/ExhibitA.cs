using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Players;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(TokenCardPool))]
public class ExhibitA : PenanceBaseCard
{
    public ExhibitA() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Ethereal
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player == null)
            return;

        var hand = PileType.Hand.GetPile(player);
        int count = Math.Min(2, hand.Cards.Count);

        if (count <= 0)
            return;

        List<CardModel> cardsToExhaust;

        if (IsUpgraded)
        {
            var prefs = new CardSelectorPrefs(
                new LocString("cards", "PENANCEMOD-EXHIBIT_A.select_message"),
                count
            )
            {
                RequireManualConfirmation = true
            };

            cardsToExhaust = (await CardSelectCmd.FromHand(
                choiceContext,
                player,
                prefs,
                null,
                this
            )).ToList();
        }
        else
        {
            cardsToExhaust = PickRandomCardsFromHand(player, count);
        }

        if (cardsToExhaust.Count == 0)
            return;

        foreach (var card in cardsToExhaust)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        await PlayerCmd.GainEnergy(2, player);
    }

    private static List<CardModel> PickRandomCardsFromHand(Player player, int count)
    {
        var candidates = PileType.Hand.GetPile(player).Cards.ToList();
        var result = new List<CardModel>();

        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            var picked = player.RunState.Rng.CombatCardSelection.NextItem(candidates);
            if (picked == null)
                break;

            candidates.Remove(picked);
            result.Add(picked);
        }

        return result;
    }

    protected override void OnUpgrade()
    {
        // 升级效果由 IsUpgraded 控制：随机 -> 手选
    }
}