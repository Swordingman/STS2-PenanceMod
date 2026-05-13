using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class LastStand : PenanceBaseCard
{
    // 耗能 2，类型 Skill，稀有度 Uncommon，目标 Self
    public LastStand() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册基础关键词：消耗
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    // 🌟 注册变量：[0] 屏障值(30)，[1] 止戈层数(3)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("LastStand-Barrier", 20m),
        new DynamicVar("LastStand-Ceasefire", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int barrierAmount = vars.Count > 0 ? vars[0].IntValue : 30;
        int ceasefireAmount = vars.Count > 1 ? vars[1].IntValue : 3;

        var creature = Owner.Creature;

        // 1. 获得巨额屏障
        await PowerCmd.Apply<BarrierPower>(choiceContext,creature, barrierAmount, creature, this);

        // 停顿一下，让两股强大的力量视觉上分离开
        await Cmd.Wait(0.15f);

        // 2. 获得止戈副作用
        await PowerCmd.Apply<CeasefirePower>(choiceContext,creature, ceasefireAmount, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LastStand-Barrier"].UpgradeValueBy(10);
    }
}