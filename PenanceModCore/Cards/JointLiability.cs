using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class JointLiability : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Uncommon
    // 🌟 目标改为 AllEnemies
    public JointLiability() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    // 🌟 注册变量：基础伤害 6
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;

        // 1. 获取当前裁决层数
        int judgeAmount = creature.GetPower<JudgementPower>()?.Amount ?? 0;

        // 2. 将基础伤害与裁决加成合并！
        // 传入的总值会自动吃力量、易伤等各种乘区
        int totalDamage = (int)(DynamicVars.Damage.BaseValue + judgeAmount);

        // 3. 执行 AOE 伤害
        await DamageCmd.Attack(totalDamage)
            .FromCard(this)
            // 🌟 核心：使用旋风斩同款的全场敌方目标锁定 API
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
    }

    // ==========================================
    // 📝 动态 UI：实时显示裁决带来的额外伤害
    // ==========================================
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        if (IsInCombat)
        {
            int judgeAmount = Owner.Creature.GetPower<JudgementPower>()?.Amount ?? 0;

            if (judgeAmount > 0)
            {
                // 动态提取语言包中的额外文本并传入参数
                LocString extendedDesc = new LocString("cards", "PENANCEMOD-JOINT_LIABILITY.extended_description");
                extendedDesc.Add("amount", judgeAmount);
                description.Add("DynamicJudgeText", extendedDesc.GetFormattedText());
            }
            else
            {
                description.Add("DynamicJudgeText", "");
            }
        }
        else
        {
            description.Add("DynamicJudgeText", "");
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 3 (6 -> 9)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}