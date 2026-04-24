using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Censure : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Basic，目标 AnyEnemy
    public Censure() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 按顺序注册：索引 0 为伤害 (6)，索引 1 为裁决数值 (2)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("Censure-Judgement", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 1. 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 2. 获得裁决 (抓取索引 1 的变量)
        var vars = DynamicVars.Values.ToList();
        int judgementAmount = vars.Count > 1 ? vars[1].IntValue : 2;

        await PowerCmd.Apply<JudgementPower>(Owner.Creature, judgementAmount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 3 (6 -> 9)
        DynamicVars.Damage.UpgradeValueBy(3);

        // 裁决提升 1 (2 -> 3)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 1)
        {
            vars[1].UpgradeValueBy(1);
        }
    }
}