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
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Overrule : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Common，目标 AnyEnemy
    public Overrule() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册变量：[0] 伤害(6)，[1] 屏障奖励(5)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("Overrule-Barrier", 5m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;

        // 1. 核心判定：在给予新虚弱之前，判定目标是否已经拥有虚弱
        // 注意：二代内置的虚弱通常是 WeakPower
        bool alreadyWeak = target.GetPower<WeakPower>() != null;

        // 2. 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 3. 给予 1 层虚弱
        await PowerCmd.Apply<WeakPower>(target, 1, creature, this);

        // 4. 如果判定满足，获得屏障
        if (alreadyWeak)
        {
            var vars = DynamicVars.Values.ToList();
            int barrierAmt = vars.Count > 1 ? vars[1].IntValue : 5;
            
            await Cmd.Wait(0.1f); // 稍微等待一下视觉反馈
            await ApplyBarrier(creature, barrierAmt);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 3 (6 -> 9)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}