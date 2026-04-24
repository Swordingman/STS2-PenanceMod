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

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class ClearTheScene : PenanceBaseCard
{
    private const int HpCost = 3;

    // 耗能 1，类型 Attack，稀有度 Common，目标 AllEnemies
    public ClearTheScene() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    // 基础伤害 13
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(13, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 失去生命值
        await CreatureCmd.Damage(choiceContext, Owner.Creature, HpCost, ValueProp.Unblockable, this);

        // 2. 对所有存活敌人造成伤害
        var aliveEnemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        
        foreach (var enemy in aliveEnemies)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(enemy)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 (13 -> 16) 
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}