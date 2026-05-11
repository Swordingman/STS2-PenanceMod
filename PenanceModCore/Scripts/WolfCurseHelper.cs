using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PenanceMod.Scripts;

public static class WolfCurseHelper
{
    public static List<CardModel> GetAllWolfCurses()
    {
        return ModelDb.AllCardPools
            .SelectMany(pool => pool.AllCardIds)
            .Distinct()
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(card => card != null)
            .Where(card => card.Tags.Contains(PenanceCardTags.CurseOfWolves))
            .ToList();
    }

    public static CardModel GetRandomWolfCurse(
        Player player,
        ICombatState combatState,
        bool upgradeBecauseSourceCardIsUpgraded = false
    )
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        if (combatState == null)
            throw new ArgumentNullException(nameof(combatState));

        var list = GetAllWolfCurses();

        if (list.Count == 0)
            throw new InvalidOperationException("没有找到任何带 CurseOfWolves Tag 的狼群诅咒牌。");

        int randomIndex = player.RunState.Rng.Shuffle.NextInt(list.Count);
        var canonicalCard = list[randomIndex];

        // 关键：不要 ToMutable。
        // 用官方 Soul 那种方式：combatState.CreateCard<T>(player)
        var randomCard = CreateCombatCardFromCanonical(
            canonicalCard,
            player,
            combatState
        );

        bool shouldUpgrade = upgradeBecauseSourceCardIsUpgraded;

        var relic = player.GetRelic<CarnivalMoment>();
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

    private static CardModel CreateCombatCardFromCanonical(
        CardModel canonicalCard,
        Player player,
        ICombatState combatState
    )
    {
        Type cardType = canonicalCard.GetType();

        MethodInfo? createCardMethod = typeof(ICombatState)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(method =>
                method.Name == "CreateCard" &&
                method.IsGenericMethodDefinition &&
                method.GetParameters().Length == 1
            );

        if (createCardMethod == null)
            throw new MissingMethodException("ICombatState.CreateCard<T>(Player) 没找到。");

        object? result = createCardMethod
            .MakeGenericMethod(cardType)
            .Invoke(combatState, new object[] { player });

        if (result is not CardModel card)
        {
            throw new InvalidOperationException(
                $"combatState.CreateCard<{cardType.Name}> 没有返回 CardModel。"
            );
        }

        return card;
    }

    private static List<IHoverTip>? _cachedNormalTips;
    private static List<IHoverTip>? _cachedUpgradedTips;
    public static IEnumerable<IHoverTip> GetWolfCurseHoverTips(bool upgradedPreview)
    {
        var tipsList = upgradedPreview ? GetOrGenerateUpgradedTips() : GetOrGenerateNormalTips();

        if (tipsList.Count == 0)
        {
            yield break;
        }

        // 每次鼠标移入时，纯随机抽选一张展示。
        // 反复摸这张牌，预览的诅咒就会不断变化！
        int randomIndex = new System.Random().Next(tipsList.Count);
        
        yield return tipsList[randomIndex];
    }

    // --- 内部缓存生成逻辑 ---
    private static List<IHoverTip> GetOrGenerateNormalTips()
    {
        if (_cachedNormalTips == null)
        {
            _cachedNormalTips = new List<IHoverTip>();
            foreach (var canonicalCard in GetAllWolfCurses())
            {
                _cachedNormalTips.Add(HoverTipFactory.FromCard(canonicalCard.ToMutable()));
            }
        }
        return _cachedNormalTips;
    }

    private static List<IHoverTip> GetOrGenerateUpgradedTips()
    {
        if (_cachedUpgradedTips == null)
        {
            _cachedUpgradedTips = new List<IHoverTip>();
            foreach (var canonicalCard in GetAllWolfCurses())
            {
                var previewCard = canonicalCard.ToMutable();
                UpgradeCardIfPossible(previewCard);
                _cachedUpgradedTips.Add(HoverTipFactory.FromCard(previewCard));
            }
        }
        return _cachedUpgradedTips;
    }

    private static void UpgradeCardIfPossible(CardModel card)
    {
        if (!card.IsUpgradable)
            return;

        card.UpgradeInternal();
        card.FinalizeUpgradeInternal();
    }
}