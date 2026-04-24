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

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class ExhibitA : PenanceBaseCard
{
    // 耗能 0，类型 Skill，稀有度 Special，目标 Self
    public ExhibitA() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true)
    {
    }

    // 绑定消耗词条 (写全路径防报错)
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => 
        [MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 遍历所有卡池，精准抓取“疑惑(Doubt)”的 ModelId
        var doubtId = ModelDb.AllCardPools
            .SelectMany(p => p.AllCardIds)
            .FirstOrDefault(id => id.Entry.ToLowerInvariant() == "doubt");

        if (doubtId != null) // 确保找到了对应的 ID
        {
            // 使用正确的 ModelId 获取卡牌数据模型
            var doubtModel = ModelDb.GetById<CardModel>(doubtId);
            var doubtCopy = doubtModel.ToMutable();
            
            // 将生成的卡牌加入手牌
            await CardPileCmd.AddGeneratedCardToCombat(doubtCopy, PileType.Hand, addedByPlayer: false);
        }

        // 2. 抽 2 张牌
        await CardPileCmd.Draw(choiceContext, 2, Owner);
    }

    protected override void OnUpgrade()
    {
        // 衍生牌且无升级逻辑，留空即可
    }
}