using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.Scripts.Cards;

// 注意：如果是特殊/无色牌，通常不需要加上专属职业牌池的 [Pool] 属性
public class Perjury : PenanceBaseCard
{
    public Perjury() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, false)
    {
    }

    // 🌟 注册基础关键词：消耗
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player == null) return;

        var regretId = ModelDb.AllCardPools
            .SelectMany(p => p.AllCardIds)
            .FirstOrDefault(id => id.Entry.ToLowerInvariant() == "regret");

        if (regretId != null) // 确保找到了对应的 ID
        {
            // 使用正确的 ModelId 获取卡牌数据模型
            var regretModel = ModelDb.GetById<CardModel>(regretId);
            var regretCopy = regretModel.ToMutable();
            
            // 将生成的卡牌加入手牌
            await CardPileCmd.AddGeneratedCardToCombat(regretCopy, PileType.Hand, addedByPlayer: false);
        }

        // 稍微等待，让卡牌入手的音效和加能量的特效错开
        await Cmd.Wait(0.1f);

        await PlayerCmd.GainEnergy(2, player);
    }

    protected override void OnUpgrade()
    {
    }
}