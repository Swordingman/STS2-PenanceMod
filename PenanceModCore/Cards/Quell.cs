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
public class Quell : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Common，目标 AnyEnemy
    public Quell() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册变量：[0] 伤害(8)，[1] 虚弱层数(1)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Move),
        new DynamicVar("Quell-Weak", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;

        // 1. 核心时序：在造成伤害前，快照当前的屏障状态
        bool hasBarrier = (creature.GetPower<BarrierPower>()?.Amount ?? 0) > 0;

        // 2. 造成基础伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 3. 如果打出时拥有屏障，给予虚弱
        if (hasBarrier)
        {
            var vars = DynamicVars.Values.ToList();
            int weakAmount = vars.Count > 1 ? vars[1].IntValue : 1;

            // 稍微停顿一下特效，让连招看起来更舒服
            await Cmd.Wait(0.1f);
            
            // 注意：二代的虚弱通常是内置的 WeakPower
            await PowerCmd.Apply<WeakPower>(target, weakAmount, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 3 (8 -> 11)
        DynamicVars.Damage.UpgradeValueBy(3);

        // 虚弱层数提升 1 (1 -> 2)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 1)
        {
            vars[1].UpgradeValueBy(1);
        }
    }
}