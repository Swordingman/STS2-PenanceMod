using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers;

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

        // 1. 基础伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 获取荆棘层数
        var thornPower = Owner.Creature.GetPower<ThornAuraPower>();
        int thorns = thornPower != null ? thornPower.Amount : 0;

        if (thorns > 0)
        {
            // 2. 触发荆棘伤害 
            await CreatureCmd.Damage(choiceContext, target, thorns, ValueProp.Unpowered, Owner.Creature);

            // 3. 检查攻击意图
            bool isAttacking = target.IsMonster && target.Monster != null && target.Monster.IntendsToAttack;
            
            if (isAttacking)
            {
                int extraDmg = thorns / 2;
                if (extraDmg > 0)
                {
                    await CreatureCmd.Damage(choiceContext, target, extraDmg, ValueProp.Unpowered, Owner.Creature);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级增加 3 点伤害
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}