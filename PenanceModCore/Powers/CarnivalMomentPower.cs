using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.PenanceModCode.Powers;

public sealed class CarnivalMomentPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None; 

    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(CarnivalMomentPower)}.png";

    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/{nameof(CarnivalMomentPower)}.png";

    public async Task TriggerCarnivalEffect(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        Flash();

        if (Owner == null || Owner.Player == null) return;
        var player = Owner.Player;

        // 1. 计算手牌空位
        int cardsToGenerate = CardPile.MaxCardsInHand - CardPile.GetCards(player, PileType.Hand).Count();
        if (cardsToGenerate <= 0) 
        {
            return; 
        }

        // 2. 获取全职业可用卡池 (Canonical 模板)
        IEnumerable<CardModel> globalCardPool = GetAllAvailableCards();

        // 3. 从全卡池中生成战斗可用的卡牌实例
        // 注意：这里返回的 distinctForCombat 已经是 Mutable 的卡牌实例，可以直接使用！
        var rng = player.RunState.Rng.CombatCardGeneration;
        IEnumerable<CardModel> instancedCards = CardFactory.GetDistinctForCombat(
            player, 
            globalCardPool, 
            cardsToGenerate, 
            rng
        );

        List<CardModel> generatedCards = new List<CardModel>();

        // 4. 遍历生成的实例卡牌，使用官方原生 API 一键改费
        foreach (var instancedCard in instancedCards)
        {
            instancedCard.SetToFreeThisCombat(); 

            generatedCards.Add(instancedCard);
        }

        // 5. 将处理好的 0 费卡牌塞入手牌
        await CardPileCmd.AddGeneratedCardsToCombat(generatedCards, PileType.Hand, base.Owner.Player);
    }

    /// <summary>
    /// 获取全职业（包括无色）的可用卡池
    /// </summary>
    private IEnumerable<CardModel> GetAllAvailableCards()
    {
        // 直接调用 ModelDb.AllCards 获取游戏内注册的所有卡牌
        return ModelDb.AllCards.Where(c => 
            c.Type != CardType.Status && // 排除状态牌（如伤口、黏液）
            c.Type != CardType.Curse &&  // 排除诅咒牌
            c.Rarity != CardRarity.Token && // 排除特殊衍生牌（如以小博大生成的牌）
            c.Rarity != CardRarity.Basic      // 排除基础牌（打击/防御），让狂欢时刻给的牌质量更高
        );
    }
}