using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Hooks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class TheTrial : PenanceBaseCard
{
    public TheTrial() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new TrialDamageVar(10, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        if (creature == null || cardPlay.Target == null) return;

        int realBaseDamage = (int)DynamicVars.Damage.BaseValue;
        
        var strPower = creature.GetPower<StrengthPower>();
        int strAmt = strPower != null ? strPower.Amount : 0;

        // 1. 我们真正想要的基于力量乘算的基础伤害
        int desiredBaseDamage = strAmt > 0 ? (realBaseDamage * strAmt) : 0;

        // 2. 【返璞归真】：既然系统必定会加上 strAmt 的力量，我们在这里提前减掉它！
        // 剩下的不管什么活力、遗物，统统交给正常的 Attack 指令去处理。
        int damageToPass = desiredBaseDamage - strAmt;

        // 3. 正常打出（去掉 Unpowered）
        await DamageCmd.Attack(damageToPass)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx(VfxCmd.heavyBluntPath)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}