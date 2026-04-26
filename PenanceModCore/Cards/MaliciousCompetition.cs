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
public class MaliciousCompetition : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Common，目标 AnyEnemy
    public MaliciousCompetition() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册变量：[0] 伤害(12), [1] 屏障(8)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("Malicious-Barrier", 8m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;

        // 1. 判定条件计算：记录攻击前的血量状态
        // ⚠️ 提示：假定二代的血量属性为 Hp (或者是 CurrentHp)
        float threshold = (float)creature.CurrentHp; 
        
        // 升级后，阈值放宽为自己血量的一半
        if (IsUpgraded)
        {
            threshold /= 2.0f;
        }

        // 提前得出是否满足“目标生命值 > 阈值”
        bool meetsCondition = (float)target.CurrentHp > threshold;

        // 2. 造成基础伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 3. 如果满足条件，获得屏障
        if (meetsCondition)
        {
            var vars = DynamicVars.Values.ToList();
            int barrierAmount = vars.Count > 1 ? vars[1].IntValue : 8;

            // 稍微停顿，分离攻击和加防特效
            await Cmd.Wait(0.15f);
            await PowerCmd.Apply<BarrierPower>(creature, barrierAmount, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}