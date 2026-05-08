using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers;
using MegaCrit.Sts2.Core.Entities.Powers; // 🌟 必须引入官方能力库以调用 WeakPower
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class BarbedRebuke : PenanceBaseCard
{
    public BarbedRebuke() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;

        // 1. 造成基础伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 2. 获取双方层数
        int thornAmt = creature.GetPower<ThornAuraPower>()?.Amount ?? 0;
        int weakAmt = target.GetPower<WeakPower>()?.Amount ?? 0;

        // 如果两者相等（或都为0），什么都不用做
        if (thornAmt == weakAmt) return;

        // 为了视觉表现更好，加一个微小的停顿，让玩家看清伤害打完后状态飞升的过程
        await Cmd.Wait(0.1f);

        // 3. 比较大小，补齐差值
        if (thornAmt > weakAmt)
        {
            // 荆棘 > 虚弱，给敌人补虚弱
            int diff = thornAmt - weakAmt;
            await PowerCmd.Apply<WeakPower>(choiceContext, target, diff, creature, this);
        }
        else
        {
            // 虚弱 > 荆棘，给自己补荆棘
            int diff = weakAmt - thornAmt;
            await PowerCmd.Apply<ThornAuraPower>(choiceContext, creature, diff, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级增加 3 点伤害 (6 -> 9)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}