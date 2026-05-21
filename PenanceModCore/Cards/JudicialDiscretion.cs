using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Extensions;
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
    // 耗能 0，类�?Skill，稀有度 Common，目�?Self
    public JudicialDiscretion() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：抽牌数 (初始 1)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Judicial-Loss", 5m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;

        // 1. 失去 5 点屏�?
        var barrierPower = creature.GetPower<BarrierPower>();
        if (barrierPower != null && barrierPower.Amount > 0)
        {
            var vars = DynamicVars.Values.ToList();
            int lostAmount = vars.Count > 0 ? vars[0].IntValue : 1;
            int reduceAmount = Math.Min((int)barrierPower.Amount, lostAmount);
            await PowerCmd.Apply<BarrierPower>(choiceContext,creature, -reduceAmount, creature, this);
        }

        // 2. 获得 1 点能�?
        await PlayerCmd.GainEnergy(1, player);

        // 3. 抽牌
        await CardPileCmd.Draw(choiceContext, 1, player);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Judicial-Loss"].UpgradeValueBy(-2);
    }
}
