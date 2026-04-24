using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;

namespace PenanceMod.Scripts;

public static class WolfCurseHelper
{
    /// <summary>
    /// 获取所有带有狼群诅咒 Tag 的卡牌列表
    /// </summary>
    public static List<CardModel> GetAllWolfCurses()
    {
        return ModelDb.AllCardPools
            .SelectMany(pool => pool.AllCardIds)
            .Distinct()
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(card => card.Tags.Contains(PenanceCardTags.CurseOfWolves))
            .ToList();
    }

    /// <summary>
    /// 获取一张随机的狼群诅咒，并处理狂欢遗物 (CarnivalMoment) 逻辑
    /// </summary>
    public static CardModel GetRandomWolfCurse(Player player)
    {
        var list = GetAllWolfCurses();
        if (list.Count == 0) 
        {
            var clumsyId = ModelDb.AllCardPools
                .SelectMany(p => p.AllCardIds)
                .FirstOrDefault(id => id.Entry.ToLowerInvariant() == "clumsy") 
                ?? ModelDb.AllCardPools.First().AllCardIds.First();

            return ModelDb.GetById<CardModel>(clumsyId).ToMutable();
        }

        var runState = player.RunState;
        
        int randomIndex = runState.Rng.Shuffle.NextInt(list.Count); 
        var randomCard = list[randomIndex].ToMutable();

        // 遗物判定
        var relic = player.GetRelic<CarnivalMomentRelic>();

        if (relic != null && randomCard.IsUpgradable)
        {
            randomCard.UpgradeInternal();
            randomCard.FinalizeUpgradeInternal(); 
            relic.Flash();
        }

        return randomCard;
    }

    /// <summary>
    /// 异步选择狼群诅咒并加入手牌 (原 ChooseAction 的二代平替)
    /// </summary>
    public static async Task ChooseAndAddWolfCurse(PlayerChoiceContext choiceContext, Player player, int amount)
    {
        var sourceCards = GetAllWolfCurses();
        if (sourceCards.Count == 0) return;

        var relic = player.GetRelic<CarnivalMomentRelic>();
        var options = new List<CardModel>();

        foreach (var c in sourceCards)
        {
            var copy = c.ToMutable();
            // 遗物升级逻辑同上
            options.Add(copy);
        }

        for (int i = 0; i < amount; i++)
        {
            var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "CHOOSE_A_CURSE"), 1); // 选1张
            IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, options, player, prefs);
            CardModel selectedCard = selectedCards?.FirstOrDefault();
            
            if (selectedCard != null)
            {
                var finalCopy = selectedCard.ToMutable();
                
                // 判断手牌上限 (二代上限依然是 10 张)
                if (player.PlayerCombatState.Hand.Cards.Count < 10)
                {
                    // 参数：加入的卡牌，目标牌堆
                    await CardPileCmd.Add(finalCopy, PileType.Hand);
                }
                else
                {
                    await CardPileCmd.Add(finalCopy, PileType.Discard);
                }
            }
        }
        
        if (relic != null) relic.Flash();
    }
}