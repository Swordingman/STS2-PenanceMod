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
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Purge : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Common，目标 AllEnemy
    public Purge() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    // 🌟 注册变量：基础伤害 6 (升级 +3)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        var combatState = creature.CombatState;

        if (combatState == null)
            return;
            
        SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_whirlwind");

        // 1. 执行群体攻击
        var attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .WithHitFx("vfx/vfx_giant_horizontal_slash")
            .TargetingAllOpponents(combatState)
            .Execute(choiceContext);

        // 2. 计算实际扣血量，也就是未被格挡的伤害
        int totalUnblockedDamage = attackCommand.Results
            .Sum(r => r.UnblockedDamage);

        // 3. 根据未格挡伤害获得屏障
        if (totalUnblockedDamage > 0)
        {
            await Cmd.Wait(0.15f);
            await ApplyBarrier(creature, totalUnblockedDamage);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 3 (6 -> 9)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}