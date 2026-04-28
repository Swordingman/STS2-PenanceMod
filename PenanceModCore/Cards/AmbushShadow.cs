using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Linq;
using System;
using System.Collections.Generic;
using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class AmbushShadow : PenanceBaseCard
{
    // 定义占位符字符串，仅用于 CanonicalVars 的初始化
    private const string MagicKey = "Ambush-Magic";

    public AmbushShadow() : base(1, CardType.Curse, CardRarity.Curse, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(MagicKey, 3m)
    ];

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this)
        {
            await Cmd.Wait(0.25f);
            await TriggerWolfAutoplay();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        
        // ⭐ 破局神技：直接抓取第一个变量，调用官方源码验证过的 IntValue
        var magicVar = DynamicVars.Values.First();
        int discardAmount = magicVar.IntValue;

        // 获取玩家手牌并转换为 List 以便安全操作
        var handCards = Owner.PlayerCombatState.Hand.Cards.ToList();
        
        // 防止要弃的牌数多于手牌实际数量
        int cardsToDiscard = Math.Min(discardAmount, handCards.Count);

        // ⭐ 手工打造：随机弃牌循环
        for (int i = 0; i < cardsToDiscard; i++)
        {
            if (handCards.Count == 0) break;
            
            // 使用官方种子随机数（如果 .Random 报错，请改为 .Next 或 .NextInt）
            int randomIndex = Owner.RunState.Rng.Shuffle.NextInt(handCards.Count); 
            var cardToDiscard = handCards[randomIndex];
            
            // 从临时列表移除，防止下一次循环重复选中同一张牌
            handCards.RemoveAt(randomIndex);
            
            // 调用官方的单张弃牌接口
            await CardCmd.Discard(choiceContext, cardToDiscard);
        }

        // 获得 1 层无实体 (如果无实体叫其他名字，请调整泛型类型)
        await PowerCmd.Apply<IntangiblePower>(creature, 1, creature, this);
    }

    protected override void OnUpgrade()
    {
        // ⭐ 同样直接抓取第一个变量进行升级
        var magicVar = DynamicVars.Values.First();
        magicVar.UpgradeValueBy(-1); 
    }
}