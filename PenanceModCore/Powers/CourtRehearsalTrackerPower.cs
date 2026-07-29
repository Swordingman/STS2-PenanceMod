using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.Scripts.Cards;

namespace PenanceMod.PenanceModCode.Powers;

public class CourtRehearsalTrackerPower : CustomPowerModel
{
    private sealed class TrackedCardState
    {
        public CardModel Card { get; set; }

        public bool IsUpgradedMode { get; set; }

        public TrackedCardState(CardModel card, bool isUpgradedMode)
        {
            Card = card;
            IsUpgradedMode = isUpgradedMode;
        }
    }

    private readonly List<TrackedCardState> _trackedCards = [];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    protected override bool IsVisibleInternal => false;

    public void Track(CardModel card, bool isUpgradedMode)
    {
        var existing = _trackedCards.FirstOrDefault(tracked => object.ReferenceEquals(tracked.Card, card));

        if (existing != null)
        {
            existing.IsUpgradedMode |= isUpgradedMode;
            return;
        }

        _trackedCards.Add(new TrackedCardState(card, isUpgradedMode));
    }

    private CardModel? GenerateRandomCard(Player player, string currentCardEntryId, bool isUpgradedMode)
    {
        var candidates = ModelDb.AllCardPools
            .OfType<PenanceModCardPool>()
            .SelectMany(pool => pool.AllCardIds)
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(card => card != null
                && card is not CourtRehearsal
                && card.Type != CardType.Curse
                && card.Type != CardType.Status
                && card.Id.Entry != currentCardEntryId)
            .ToList();

        if (candidates.Count == 0) return null;

        var randomCard = CardFactory.GetDistinctForCombat(player, candidates, 1, player.RunState.Rng.CombatCardGeneration).FirstOrDefault();

        if (randomCard != null && isUpgradedMode && randomCard.IsUpgradable && !randomCard.IsUpgraded)
        {
            randomCard.UpgradeInternal();
            randomCard.FinalizeUpgradeInternal();
        }

        return randomCard;
    }

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != player.Creature) return;

        var handPile = PileType.Hand.GetPile(player);

        foreach (var tracked in _trackedCards.ToList())
        {
            var isStillInHand = handPile.Cards.Any(card => object.ReferenceEquals(card, tracked.Card));

            if (!isStillInHand)
            {
                _trackedCards.Remove(tracked);
                continue;
            }

            var newCard = GenerateRandomCard(player, tracked.Card.Id.Entry, tracked.IsUpgradedMode);
            if (newCard == null) continue;

            newCard.AddKeyword(CardKeyword.Retain);
            newCard.AddKeyword(CardKeyword.Exhaust);

            /*
             * 这里已经排除了 CourtRehearsal，
             * 不会再发生“Tracker 变出庭审预演，庭审预演立刻嵌套变形”的情况。
             */
            await CardCmd.Transform(tracked.Card, newCard);

            // Transform 发生在手牌中，不会经过加入牌库时的替换 Hook。
            tracked.Card = newCard;
        }

        if (_trackedCards.Count == 0)
        {
            await PowerCmd.Remove(this);
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var removedCount = _trackedCards.RemoveAll(tracked => object.ReferenceEquals(tracked.Card, cardPlay.Card));

        if (removedCount > 0 && _trackedCards.Count == 0)
        {
            await PowerCmd.Remove(this);
        }
    }
}