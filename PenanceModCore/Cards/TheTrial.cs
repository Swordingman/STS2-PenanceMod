using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class TheTrial : PenanceBaseCard
{
    public TheTrial() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 完美挂载官方接口
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(10m), // 基础伤害 10
        new ExtraDamageVar(1m),      // 额外乘数固定为 1（作为抵消公式的基底）
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateStrengthMultiplier)
    ];

    // 🌟 纯静态方法，通过官方 Multiplier 计算抵消值
    private static decimal CalculateStrengthMultiplier(CardModel card, Creature? target)
    {
        var player = card.Owner;
        if (player == null) return 0m;

        // 拿取当前力量
        var strPower = player.Creature.GetPower<StrengthPower>();
        int strAmt = strPower != null ? strPower.Amount : 0;

        // 获取卡牌基础伤害 (没升级是10，升级是15)
        decimal baseDamage = card.DynamicVars.CalculationBase.BaseValue;

        return (baseDamage * strAmt) - baseDamage - strAmt;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(target)
            .WithHitFx(VfxCmd.heavyBluntPath)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 升级时只需要改 BaseValue，抵消公式会自动适配出 15、30、45 的乘算伤害！
        DynamicVars.CalculationBase.UpgradeValueBy(5);
    }
}