using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using BaseLib.Extensions;
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
        new DynamicVar("Malicious-Barrier", 7m).WithTooltip("PENANCEMOD-BARRIER"),
        new DynamicVar("Malicious-Judge", 2m).WithTooltip("PENANCEMOD-JUDGEMENT")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player == null)
        {
            return;
        }

        var creature = player.Creature;
        CardPile drawPile = PileType.Draw.GetPile(player);

        CardModel? cardToExhaust;

        if (IsUpgraded)
        {
            // 升级后：从抽牌堆顶部开始，找第一张诅咒牌
            cardToExhaust = drawPile.Cards.FirstOrDefault(c => c.Type == CardType.Curse);
        }
        else
        {
            // 未升级：消耗抽牌堆顶部�?
            cardToExhaust = drawPile.Cards.FirstOrDefault();
        }

        // 没有可消耗的牌，就不给增�?
        if (cardToExhaust == null)
        {
            return;
        }

        // 先消�?
        await CardCmd.Exhaust(choiceContext, cardToExhaust);

        // 消耗成功后再给增益
        await ApplyBarrier(creature, DynamicVars["Malicious-Barrier"].IntValue);
        await ApplyJudgement(creature, DynamicVars["Malicious-Judge"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Malicious-Barrier"].UpgradeValueBy(1);
        DynamicVars["Malicious-Judge"].UpgradeValueBy(1);
    }
}
