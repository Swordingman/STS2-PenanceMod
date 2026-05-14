using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class MaliciousCompetition : PenanceBaseCard
{
    public MaliciousCompetition() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Malicious-Barrier", 7m),
        new DynamicVar("Malicious-Judge", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;

        // 1. 先结算增益：获得屏障和裁决
        await ApplyBarrier(player.Creature, DynamicVars["Malicious-Barrier"].IntValue);
        await ApplyJudgement(player.Creature, DynamicVars["Malicious-Judge"].IntValue);

        // 2. 处理消耗手牌逻辑 (完美复刻官方《坚毅》的写法)
        CardPile handPile = PileType.Hand.GetPile(player);
        if (handPile.Cards.Count > 0)
        {
            if (IsUpgraded)
            {
                // 升级后：调用官方手动选牌界面
                var selectedCards = await CardSelectCmd.FromHand(
                    prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
                    context: choiceContext,
                    player: player,
                    filter: null,
                    source: this
                );

                var cardToExhaust = selectedCards.FirstOrDefault();
                if (cardToExhaust != null)
                {
                    await CardCmd.Exhaust(choiceContext, cardToExhaust);
                }
            }
            else
            {
                // 未升级：调用官方战斗 RNG 随机选牌
                var cardToExhaust = player.RunState.Rng.CombatCardSelection.NextItem(handPile.Cards);
                if (cardToExhaust != null)
                {
                    await CardCmd.Exhaust(choiceContext, cardToExhaust);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Malicious-Barrier"].UpgradeValueBy(1);
        DynamicVars["Malicious-Judge"].UpgradeValueBy(1);
    }
}