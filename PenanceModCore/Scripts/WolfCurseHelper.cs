using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PenanceMod.Scripts;

public static class WolfCurseHelper
{
    /// <summary>
    /// 获取所有带有“狼群诅咒”Tag 的卡牌。
    ///
    /// 用法说明：
    /// ModelDb.AllCardPools 可以遍历所有牌池。
    /// pool.AllCardIds 是该牌池里的所有卡牌 ID。
    /// ModelDb.GetById<CardModel>(id) 可以拿到对应的卡牌模型。
    ///
    /// 注意：
    /// 这里返回的是数据库里的 canonical 卡牌，不应该直接塞进牌堆。
    /// 真正加入手牌、抽牌堆、弃牌堆前，要先 ToMutable()。
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
    /// 获取一张随机狼群诅咒。
    ///
    /// 参数说明：
    /// player：当前玩家，用来取 RunState.Rng，以及给生成卡设置 Owner。
    /// upgradeBecauseSourceCardIsUpgraded：来源卡是否升级。
    ///
    /// 这对应原 Java 逻辑：
    /// AbstractCard c = WolfCurseHelper.getRandomWolfCurse();
    /// if (this.upgraded) c.upgrade();
    /// </summary>
    public static CardModel GetRandomWolfCurse(
        Player player,
        bool upgradeBecauseSourceCardIsUpgraded = false
    )
    {
        var list = GetAllWolfCurses();

        if (list.Count == 0)
        {
            // 兜底逻辑：
            // 如果没有找到任何狼群诅咒，就尽量找 clumsy。
            // 找不到 clumsy 时，就拿第一张卡兜底，避免空列表崩溃。
            var fallbackId = ModelDb.AllCardPools
                .SelectMany(pool => pool.AllCardIds)
                .FirstOrDefault(id => id.Entry.Equals("clumsy", StringComparison.OrdinalIgnoreCase))
                ?? ModelDb.AllCardPools.First().AllCardIds.First();

            var fallback = ModelDb.GetById<CardModel>(fallbackId).ToMutable();
            fallback.Owner = player;
            return fallback;
        }

        // 使用 runState 的随机源，避免使用系统随机导致回放/同步问题。
        int randomIndex = player.RunState.Rng.Shuffle.NextInt(list.Count);

        // ToMutable()：把数据库里的 canonical 卡复制成战斗中可修改实例。
        var randomCard = list[randomIndex].ToMutable();

        // 生成卡必须归属到当前玩家。
        randomCard.Owner = player;

        // 本牌升级时，生成升级后的狼群诅咒。
        bool shouldUpgrade = upgradeBecauseSourceCardIsUpgraded;

        // 你的遗物逻辑：如果有 CarnivalMomentRelic，则生成的诅咒升级。
        //
        // 如果你的 GetRelic<T>() 是 BaseLib 或你自己写的扩展方法，这里可以直接用。
        // 如果 IDE 提示没有 GetRelic<T>()，就需要换成你项目里获取遗物的方式。
        var relic = player.GetRelic<CarnivalMomentRelic>();
        if (relic != null)
        {
            shouldUpgrade = true;
            relic.Flash();
        }

        if (shouldUpgrade)
        {
            UpgradeCardIfPossible(randomCard);
        }

        return randomCard;
    }

    /// <summary>
    /// 给 SyracusanWolves 做“狼群诅咒预览”。
    ///
    /// 用法说明：
    /// ExtraHoverTips 返回 IEnumerable<IHoverTip>。
    /// HoverTipFactory.FromCard(card) 会把一张 CardModel 转成卡牌预览。
    ///
    /// 如果你的 HoverTipFactory 没有 FromCard(CardModel) 这个重载，
    /// 这段会编译失败。
    ///
    /// 那种情况下有两个替代方案：
    /// 1. 手动写死每张狼群诅咒：
    ///    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ///        HoverTipFactory.FromCard<WolfCurseA>(),
    ///        HoverTipFactory.FromCard<WolfCurseB>(),
    ///        ...
    ///    ];
    ///
    /// 2. 暂时删掉 SyracusanWolves 里的 ExtraHoverTips，
    ///    只保留打出逻辑。
    /// </summary>
    public static IEnumerable<IHoverTip> GetWolfCurseHoverTips(bool upgradedPreview)
    {
        foreach (var canonicalCard in GetAllWolfCurses())
        {
            var previewCard = canonicalCard.ToMutable();

            if (upgradedPreview)
            {
                UpgradeCardIfPossible(previewCard);
            }

            yield return HoverTipFactory.FromCard(previewCard);
        }
    }

    /// <summary>
    /// 安全升级一张卡。
    ///
    /// 用法说明：
    /// UpgradeInternal() 会执行卡牌自己的 OnUpgrade()。
    /// FinalizeUpgradeInternal() 会把升级后的 DynamicVars / EnergyCost 定稿。
    /// 这两个最好成对调用。
    /// </summary>
    private static void UpgradeCardIfPossible(CardModel card)
    {
        if (!card.IsUpgradable)
            return;

        card.UpgradeInternal();
        card.FinalizeUpgradeInternal();
    }
}