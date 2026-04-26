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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class JudicialDiscretion : PenanceBaseCard
{
    // 耗能 0，类型 Skill，稀有度 Common，目标 Self
    public JudicialDiscretion() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：抽牌数 (初始 1)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Judicial-Draw", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;

        // 1. 失去 5 点屏障
        var barrierPower = creature.GetPower<BarrierPower>();
        if (barrierPower != null && barrierPower.Amount > 0)
        {
            int reduceAmount = Math.Min((int)barrierPower.Amount, 5);
            await PowerCmd.Apply<BarrierPower>(creature, -reduceAmount, creature, this);
        }

        // 2. 获得 1 点能量
        await PlayerCmd.GainEnergy(1, player);

        // 3. 抽牌
        var vars = DynamicVars.Values.ToList();
        int drawAmount = vars.Count > 0 ? vars[0].IntValue : 1;
        await CardPileCmd.Draw(choiceContext, drawAmount, player);
    }

    protected override void OnUpgrade()
    {
        // 抽牌数 +1 (1 -> 2)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(1);
        }
    }
}