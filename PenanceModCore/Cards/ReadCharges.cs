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

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class ReadCharges : PenanceBaseCard
{
    // 耗能 1，类型 Skill，稀有度 Common，目标 Self
    public ReadCharges() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：抽牌数 (初始 1)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Read-Draw", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player == null) return;
        
        var creature = Owner.Creature;

        // 1. 获得 3 点裁决 (固定数值直接传字面量即可，调用你的快捷方法)
        await ApplyJudgement(creature, 3);

        // 稍微停顿，把加 Buff 的音效/特效和抽牌动作错开
        await Cmd.Wait(0.1f);

        // 2. 抽牌
        var vars = DynamicVars.Values.ToList();
        int drawAmount = vars.Count > 0 ? vars[0].IntValue : 1;

        await CardPileCmd.Draw(choiceContext, drawAmount, player);
    }

    protected override void OnUpgrade()
    {
        // 抽牌数提升 1 (1 -> 2)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(1);
        }
    }
}