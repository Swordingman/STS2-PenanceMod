using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class SelfIncrimination : PenanceBaseCard
{
    public SelfIncrimination() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<ExhibitA>(),
        HoverTipFactory.FromCard<Perjury>()
    ];

    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            if (!IsInCombat)
                return true;

            var player = Owner;
            if (player == null)
                return true;

            var drawPile = PileType.Draw.GetPile(player);
            return drawPile.Cards.Any(card => card.Type == CardType.Curse);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player == null)
            return;

        var drawPile = PileType.Draw.GetPile(player);

        // 从抽牌堆顶部往下找第一张诅咒
        var curse = drawPile.Cards.FirstOrDefault(card => card.Type == CardType.Curse);
        if (curse == null)
            return;

        // 把这张诅咒从抽牌堆“抽出”到手牌
        await CardPileCmd.Add(
            curse,
            PileType.Hand,
            CardPilePosition.Bottom,
            this
        );

        await Cmd.Wait(0.1f);

        await CreateGeneratedCardInHand<ExhibitA>(player, IsUpgraded);
        await CreateGeneratedCardInHand<Perjury>(player, IsUpgraded);
    }

    private static async Task<T?> CreateGeneratedCardInHand<T>(Player player, bool upgraded)
        where T : CardModel
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null)
            return null;

        var card = combatState.CreateCard<T>(player);

        if (upgraded && card.IsUpgradable && !card.IsUpgraded)
        {
            card.UpgradeInternal();
            card.FinalizeUpgradeInternal();
        }

        await CardPileCmd.AddGeneratedCardToCombat(
            card,
            PileType.Hand,
            player
        );

        return card;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}