using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Patches.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class PreExecutionPrep : PenanceBaseCard
{
    // 耗能 2，类型 Skill，稀有度 Rare，目标 Self
    public PreExecutionPrep() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    // 🌟 注册基础关键词：消耗
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;

        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null)
            return;

        int maxHandSize = MaxHandSizePatch.GetMaxHandSize(player);
        int drawAmount = maxHandSize - hand.Cards.Count;

        if (drawAmount <= 0)
            return;

        // 抽牌前手牌快照，用 ReferenceEquals 避免同名/同 ID 卡牌误判
        var oldHandSnapshot = hand.Cards.ToList();

        await CardPileCmd.Draw(choiceContext, drawAmount, player);

        var newlyDrawnCards = hand.Cards
            .Where(card => !oldHandSnapshot.Any(oldCard => ReferenceEquals(oldCard, card)))
            .ToList();

        foreach (var card in newlyDrawnCards)
        {
            await ProcessDrawnCard(card, creature, player);
        }
    }

    // 🌟 处理单张卡牌收益：利用二代的异步指令流
    private async Task ProcessDrawnCard(CardModel card, Creature creature, Player player)
    {
        switch (card.Type)
        {
            case CardType.Attack:
                await PowerCmd.Apply<JudgementPower>(creature, 1, creature, this);
                break;

            case CardType.Skill:
                await PowerCmd.Apply<ThornAuraPower>(creature, 1, creature, this);
                break;

            case CardType.Power:
                await PowerCmd.Apply<StrengthPower>(creature, 1, creature, this);
                break;

            case CardType.Curse:
                await PlayerCmd.GainEnergy(1, player);
                break;
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}