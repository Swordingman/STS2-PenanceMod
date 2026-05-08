using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
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
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace PenanceMod.PenanceModCode.Powers;

public class SceneInvestigationPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(SceneInvestigationPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(SceneInvestigationPower)}.png";

    private CardType? _lastCardType = null;

    // --- 内部持久化数据 ---
    [SavedProperty]
    public int EnergyPerTrigger { get; set; } = 2;
    
    [SavedProperty]
    public bool CardsShouldUpgrade { get; set; } = false;

    // ==========================================
    // 1. 注册动态变量（这会让 JSON 里的 {EnergyTotal} 和 {IsUpgraded} 生效）
    // ==========================================
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("EnergyTotal", 0m), 
        new DynamicVar("IsUpgraded", 0m)
    ];

    // 同步变量数值的方法
    private void RefreshDynamicVars()
    {
        // 确保 DynamicVars 已经被初始化
        if (DynamicVars == null) return;

        // 更新数值到变量中
        DynamicVars["EnergyTotal"].BaseValue = Amount * EnergyPerTrigger;
        // 0 代表 false，1 代表 true，SmartFormat 会自动识别
        DynamicVars["IsUpgraded"].BaseValue = CardsShouldUpgrade ? 1m : 0m;
    }

    // ==========================================
    // 2. 生命周期钩子：在数值变化时刷新变量
    // ==========================================

    // 情况 A：当能力第一次被挂上，或者打出新牌导致能力叠层/升级时
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (cardSource != null)
        {
            CardsShouldUpgrade = CardsShouldUpgrade || cardSource.IsUpgraded;
            
            if (cardSource.DynamicVars.TryGetValue("Scene-Energy", out var energyVar))
            {
                EnergyPerTrigger = Math.Max(EnergyPerTrigger, energyVar.IntValue);
            }
        }
        
        RefreshDynamicVars();
        return Task.CompletedTask;
    }

    // 情况 B：当能力的层数（Amount）改变时（比如再次获得该能力导致 Counter 增加）
    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            RefreshDynamicVars();
        }
        return Task.CompletedTask;
    }

    // ==========================================
    // 3. 战斗逻辑部分（保持不变，但更简洁）
    // ==========================================

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var currentType = cardPlay.Card.Type;

        if (currentType != CardType.Attack && currentType != CardType.Skill && currentType != CardType.Power)
        {
            _lastCardType = null;
            return;
        }

        if (_lastCardType == currentType)
        {
            Flash();
            await TriggerEffect(currentType, context);
        }

        _lastCardType = currentType;
    }

    private async Task TriggerEffect(CardType type, PlayerChoiceContext context)
    {
        var player = Owner.Player;
        // 直接读变量里算好的值，或者读属性，都没问题
        int energyToGain = (int)DynamicVars["EnergyTotal"].BaseValue;

        switch (type)
        {
            case CardType.Attack:
                await GiveRandomCards(CardType.Skill, (int)Amount);
                break;
            case CardType.Skill:
                await GiveRandomCards(CardType.Attack, (int)Amount);
                break;
            case CardType.Power:
                if (energyToGain > 0)
                    await PlayerCmd.GainEnergy(energyToGain, player);
                break;
        }
    }

    private async Task GiveRandomCards(CardType targetType, int count)
    {
        var player = Owner.Player;
        if (player == null) return;

        var generatedCards = CardFactory.GetDistinctForCombat(
            player,
            from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            where c.Type == targetType && c.CanBeGeneratedInCombat
            select c,
            count,
            player.RunState.Rng.CombatCardGeneration
        ).ToList();

        foreach (var randomCard in generatedCards)
        {
            if (CardsShouldUpgrade && randomCard.IsUpgradable) 
            {
                randomCard.UpgradeInternal(); 
                randomCard.FinalizeUpgradeInternal(); 
            }

            randomCard.AddKeyword(CardKeyword.Exhaust);
            randomCard.AddKeyword(CardKeyword.Ethereal);

            await CardPileCmd.AddGeneratedCardToCombat(randomCard, PileType.Hand, player, CardPilePosition.Bottom);
        }
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == player.Creature) _lastCardType = null;
        return Task.CompletedTask;
    }
}