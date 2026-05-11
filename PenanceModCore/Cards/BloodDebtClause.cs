using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Character;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class BloodDebtClause : PenanceBaseCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves),
        ..WolfCurseHelper.GetWolfCurseHoverTips(IsUpgraded)
    ];

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
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        // 1. 抛弃傻乎乎的 for 循环！直接把所有存活敌人作为一个集合喂给 Targeting。
        // 引擎会自动把它渲染成一次极其爽快的、同时跳字的 AoE 攻击！
        var attackCmd = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .WithHitFx(VfxCmd.giantHorizontalSlashPath)
            .TargetingAllOpponents(combatState)
            .Execute(choiceContext);

        // 2. 统计实际命中了多少个敌人
        // 利用老熟人“嵌套列表”，直接数一数里面一共生成了几个受击结果
        int hitCount = attackCmd.Results.Sum(hitList => hitList.Count);

        // 3. 每击中一个敌人，就生成一张狼群诅咒放入弃牌堆
        for (int i = 0; i < hitCount; i++)
        {
            var randomCurse = WolfCurseHelper.GetRandomWolfCurse(Owner, combatState, IsUpgraded);
            
            // 使用官方指令添加卡牌，完美触发联动
            var addedCard = await CardPileCmd.AddGeneratedCardToCombat(randomCurse, PileType.Discard, Owner);
            
            // 完美保留你写的华丽预览特效
            CardCmd.PreviewCardPileAdd(addedCard, 2.2f);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 (12 -> 15)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}