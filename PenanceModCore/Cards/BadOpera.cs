using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Utils;
using BaseLib.Extensions;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class BadOpera : PenanceBaseCard
{
    public BadOpera() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Opera-CurseCount", 1m)
        .WithTooltip("PENANCEMOD-JUDGEMENT")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;
        
        // 1. 获取抽牌堆中的诅咒
        var drawPile = PileType.Draw.GetPile(player);
        var availableCurses = drawPile.Cards.Where(c => c.Type == CardType.Curse).ToList();

        // 2. 如果升级了，将弃牌堆的诅咒也加入到可用池中
        if (this.IsUpgraded)
        {
            var discardPile = PileType.Discard.GetPile(player);
            var cursesInDis = discardPile.Cards.Where(c => c.Type == CardType.Curse).ToList();
            availableCurses.AddRange(cursesInDis); 
        }

        // 3. 检查可用池中是否有诅咒
        if (availableCurses.Count > 0)
        {
            await CreatureCmd.TriggerAnim(creature, "Cast", 0.2f);

            int maxToExhaust = DynamicVars["Opera-CurseCount"].IntValue;

            // 4. 将整个可用池打乱，并取出指定数量（1张）的诅咒
            var cursesToExhaust = availableCurses
                .OrderBy(_ => player.RunState.Rng.Shuffle.NextInt()) 
                .Take(maxToExhaust)
                .ToList();

            foreach (var curse in cursesToExhaust)
            {
                // 5. 消耗诅咒
                await CardCmd.Exhaust(choiceContext, curse, false);
                
                // 6. 获得奖励：1 力量 和 2 裁决
                await PowerCmd.Apply<StrengthPower>(choiceContext, creature, 1, creature, this);
                await ApplyJudgement(creature, 2);
                
                await Cmd.Wait(0.1f);
            }
        }
    }

    protected override void OnUpgrade(){}
}