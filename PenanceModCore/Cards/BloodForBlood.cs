using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; // 假设正当防卫 Power 在这里

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class BloodForBlood : PenanceBaseCard
{
    // 耗能 0，类型 Attack，稀有度 Common，目标 AnyEnemy
    public BloodForBlood() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    // 绑定消耗属性
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    // 基础伤害 4
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        var creature = Owner.Creature; // 玩家自己

        if (target != null)
        {
            // 1. 造成伤害
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);
        }

        // 2. 获得 1 层正当防卫 (JustifiedDefensePower)
        await PowerCmd.Apply<JustifiedDefensePower>(creature, 1, creature, this);
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 (4 -> 7)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}