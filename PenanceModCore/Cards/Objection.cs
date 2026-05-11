using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Objection : PenanceBaseCard
{
    public Objection() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册关键字：消耗，虚无
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("Objection-Cost", 5m) // 裁决需求量
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;

        // 1. 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 2. 获取当前裁决层数
        var judgePower = creature.GetPower<JudgementPower>();
        int judgeAmount = judgePower?.Amount ?? 0;
        int requiredJudge = DynamicVars["Objection-Cost"].IntValue;

        // 3. 判断是否满足条件 (>= 需求量)
        if (judgeAmount >= requiredJudge)
        {
            // 扣除裁决层数
            await PowerCmd.Apply<JudgementPower>(choiceContext, creature, -requiredJudge, creature, this);

            // 给予 1 层止戈
            await PowerCmd.Apply<CeasefirePower>(choiceContext, target, 1, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级降低裁决的消耗量 (5 -> 3)
        DynamicVars["Objection-Cost"].UpgradeValueBy(-2);
    }
}