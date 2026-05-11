using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using PenanceMod.PenanceModCode.Character;

namespace PenanceMod.PenanceModCode.Powers;

public class CourtRehearsalTrackerPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.None;

    protected override bool IsVisibleInternal => false;

    [SavedProperty]
    public CardModel TrackedCard { get; set; }

    [SavedProperty]
    public bool IsUpgradedMode { get; set; }

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        // 确保是当前 Power 拥有者的手牌结算阶段
        if (Owner != player.Creature) return;

        var handPile = PileType.Hand.GetPile(player);

        // 检查被追踪的卡是否还在手牌里（说明这回合玩家把这张牌憋在手里没打）
        if (TrackedCard != null && handPile.Cards.Contains(TrackedCard))
        {
            var newCard = GenerateRandomCard(player, TrackedCard.Id.Entry);
            if (newCard != null)
            {
                // 给新牌加上保留(Retain)和消耗(Exhaust)
                newCard.AddKeyword(CardKeyword.Retain);
                newCard.AddKeyword(CardKeyword.Exhaust);

                // 变身为下一张牌
                await CardCmd.Transform(TrackedCard, newCard);

                // 更新追踪目标
                TrackedCard = newCard;
            }
        }
        else
        {
            // 如果它不在手里（比如被打出或被手动丢弃了），追踪器自毁
            await PowerCmd.Remove(this);
        }
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card == TrackedCard)
        {
            // 如果打出了追踪的卡，使用弃用等待安全销毁追踪器
            _ = PowerCmd.Remove(this);
        }
        return Task.CompletedTask;
    }

    private CardModel GenerateRandomCard(Player player, string currentCardEntryId)
    {
        var candidates = ModelDb.AllCardPools
            .OfType<PenanceModCardPool>() 
            .SelectMany(pool => pool.AllCardIds)
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(card => card != null && card.Type != CardType.Curse && card.Type != CardType.Status && card.Id.Entry != currentCardEntryId)
            .ToList();

        if (candidates.Count == 0) return null;

        var randomCard = CardFactory.GetDistinctForCombat(
            player, candidates, 1, player.RunState.Rng.CombatCardGeneration
        ).FirstOrDefault();

        if (randomCard != null && IsUpgradedMode && randomCard.IsUpgradable && !randomCard.IsUpgraded)
        {
            randomCard.UpgradeInternal();
            randomCard.FinalizeUpgradeInternal();
        }

        return randomCard;
    }
}