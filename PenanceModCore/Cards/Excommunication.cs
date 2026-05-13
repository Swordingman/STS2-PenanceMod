using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers; // 力量Power
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Excommunication : PenanceBaseCard
{
    public Excommunication() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Excom-Barrier", 3m),
        new DynamicVar("Excom-Str", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        
        // 1. 获取三个牌库里的所有诅咒
        var handCurses = PileType.Hand.GetPile(player).Cards.Where(c => c.Type == CardType.Curse).ToList();
        var drawCurses = PileType.Draw.GetPile(player).Cards.Where(c => c.Type == CardType.Curse).ToList();
        var discardCurses = PileType.Discard.GetPile(player).Cards.Where(c => c.Type == CardType.Curse).ToList();

        // 将它们合并成一个列表
        var allCurses = handCurses.Concat(drawCurses).Concat(discardCurses).ToList();
        int curseCount = allCurses.Count;

        if (curseCount > 0)
        {
            // 2. 消耗所有找到的诅咒
            foreach (var curse in allCurses)
            {
                await CardCmd.Exhaust(choiceContext, curse, causedByEthereal: false);
            }

            // 让消耗特效飞一会儿，演出效果更好
            await Cmd.Wait(0.2f);

            // 3. 计算并获得收益
            int totalBarrier = curseCount * DynamicVars["Excom-Barrier"].IntValue;
            int totalStr = curseCount * DynamicVars["Excom-Str"].IntValue;

            await ApplyBarrier(player.Creature, totalBarrier);
            await PowerCmd.Apply<StrengthPower>(choiceContext, player.Creature, totalStr, player.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1); // 2 -> 1 费
    }
}