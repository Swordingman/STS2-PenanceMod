using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers; // 力量 Power
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Utils;

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
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;
        
        // 1. 寻找抽牌堆中的所有诅咒
        var drawPile = PileType.Draw.GetPile(player);
        var cursesInDraw = drawPile.Cards.Where(c => c.Type == CardType.Curse).ToList();

        if (cursesInDraw.Count > 0)
        {
            // 2. 随机决定消耗哪些诅咒 (使用战斗随机数)
            int maxToExhaust = DynamicVars["Opera-CurseCount"].IntValue;
            var cursesToExhaust = cursesInDraw
                .OrderBy(_ => player.RunState.Rng.Shuffle.NextInt()) 
                .Take(maxToExhaust)
                .ToList();

            foreach (var curse in cursesToExhaust)
            {
                // 3. 消耗诅咒
                await CardCmd.Exhaust(choiceContext, curse, false);
                
                // 4. 获得奖励：1 力量 和 2 裁决
                await PowerCmd.Apply<StrengthPower>(choiceContext, creature, 1, creature, this);
                await ApplyJudgement(creature, 2);
                
                await Cmd.Wait(0.1f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Opera-CurseCount"].UpgradeValueBy(1);
    }
}