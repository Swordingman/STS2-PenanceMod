using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Linq;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class BadOpera : PenanceBaseCard
{
    private const string MagicKey = "BadOpera-Magic";

    public BadOpera() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(MagicKey, 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // ✅ 记吃记打：安全拿第一个变量的值 (生成2张)
        var magicVar = DynamicVars.Values.First();
        int generateAmount = magicVar.IntValue;

        // 检索全游戏所有合法的攻击牌
        var allAttacks = ModelDb.AllCardPools
            .SelectMany(pool => pool.AllCardIds)
            .Distinct()
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(c => c.Type == CardType.Attack && c.CanBeGeneratedInCombat)
            .ToList();

        if (allAttacks.Count == 0) return;

        for (int i = 0; i < generateAmount; i++)
        {
            // 使用官方专属的卡牌生成 RNG 种子
            int randomIndex = Owner.RunState.Rng.Shuffle.NextInt(allAttacks.Count);
            var randomCard = allAttacks[randomIndex].ToMutable();

            // 如果这首歌剧升级了，生成的牌也自动升级！
            if (this.IsUpgraded && randomCard.IsUpgradable)
            {
                randomCard.UpgradeInternal();
                randomCard.FinalizeUpgradeInternal();
            }

            // 加入手牌
            await CardPileCmd.AddGeneratedCardToCombat(randomCard, PileType.Hand, addedByPlayer: true);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级不改变数量，而是改变特效。因为 OnPlay 里直接用 IsUpgraded 做了判定，
        // 所以这里留空即可，不需要去改动 DynamicVars 的数值。
    }
}