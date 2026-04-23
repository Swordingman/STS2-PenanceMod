using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Character;
using System.Linq;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class BloodDebtClause : PenanceBaseCard
{
    // 耗能 1，类型为 Attack，稀有度 Common，目标为 AllEnemies (全体敌人)
    public BloodDebtClause() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    // 基础伤害 12
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取当前存活的所有敌人
        var aliveEnemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();

        // 1. 造成全体伤害
        // 塔 2 的命令系统支持针对每个敌人逐一派发伤害请求，引擎会自动把它们合并成 AOE 的视觉效果
        foreach (var enemy in aliveEnemies)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(enemy)
                .Execute(choiceContext);
        }

        // 2. 结算诅咒 (还原一代中“伤害结算后存活”的判定)
        // 再次过滤一遍，找出被刚才的 AOE 刮完之后依然存活的敌人
        var survivingEnemies = aliveEnemies.Where(e => e.IsAlive).ToList();

        foreach (var enemy in survivingEnemies)
        {
            // 直接调用我们写好的强力 Helper
            var randomCurse = WolfCurseHelper.GetRandomWolfCurse(Owner);
            
            // 将生成的诅咒卡加入弃牌堆 (PileType.Discard)
            await CardPileCmd.Add(randomCurse, PileType.Discard);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 (12 -> 15)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}