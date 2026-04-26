using System;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;

namespace PenanceMod.PenanceModCode.Powers;

public class SceneInvestigationPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(SceneInvestigationPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(SceneInvestigationPower)}.png";

    private CardType? _lastCardType = null;
    private int _energyPerTrigger;
    private bool _cardsShouldUpgrade;

    public int EnergyTotal => (int)Amount * _energyPerTrigger;
    public bool IsUpgraded => _cardsShouldUpgrade;

    public void Initialize(int energy, bool upgrade)
    {
        _energyPerTrigger = energy;
        _cardsShouldUpgrade = upgrade;
    }

    public void UpdateStats(int newEnergy, bool newUpgrade)
    {
        _energyPerTrigger = Math.Max(_energyPerTrigger, newEnergy);
        _cardsShouldUpgrade = _cardsShouldUpgrade || newUpgrade;
    }

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
        int count = (int)Amount;

        switch (type)
        {
            case CardType.Attack:
                await GiveRandomCards(CardType.Skill, count);
                break;
            case CardType.Skill:
                await GiveRandomCards(CardType.Attack, count);
                break;
            case CardType.Power:
                await PlayerCmd.GainEnergy(EnergyTotal, player);
                break;
        }
    }

    private async Task GiveRandomCards(CardType targetType, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 获取随机卡牌的模型
            CardModel randomCard = GetRandomCardFromPool(targetType); 

            if (randomCard != null)
            {
                // 1. 升级卡牌 (严格遵循源码的两步升级法)
                if (_cardsShouldUpgrade) 
                {
                    randomCard.UpgradeInternal(); 
                    randomCard.FinalizeUpgradeInternal(); 
                }

                // 2. 赋予消耗和虚无 (使用统一的关键字系统)
                randomCard.AddKeyword(CardKeyword.Exhaust);
                randomCard.AddKeyword(CardKeyword.Ethereal);

                // 3. 调用底层指令：把捏好的卡牌加入手牌
                await CardPileCmd.AddGeneratedCardToCombat(
                    randomCard, 
                    PileType.Hand,           // 目标牌堆：手牌
                    true,                    // 是否由玩家添加：true
                    CardPilePosition.Bottom  // 添加位置
                );
            }
        }
    }

    // 辅助占位方法：你需要用 StS2 真实的随机卡获取 API 替换里面的内容
    private CardModel GetRandomCardFromPool(CardType targetType)
    {
        // 伪代码：
        // return RunManager.CurrentRun.CardPool.GetRandomCard(targetType).ToMutable();
        return null!; 
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == player.Creature)
        {
            _lastCardType = null;
        }
        return Task.CompletedTask;
    }
}