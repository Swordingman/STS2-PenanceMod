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
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class FinalVerdict : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Rare，目标 AnyEnemy
    public FinalVerdict() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    // 绑定“消耗”词条
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => 
        [MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Exhaust];

    // 🌟 注册变量：索引 0 = 基础伤害 (18)， 索引 1 = 最大生命值收益 (3)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(18, ValueProp.Move),
        new DynamicVar("Verdict-MaxHp", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var vars = DynamicVars.Values.ToList();
        int maxHpGain = vars.Count > 1 ? vars[1].IntValue : 3;

        // 1. 获取当前裁决层数
        var judgePower = Owner.Creature.GetPower<JudgementPower>();
        int judgeAmount = judgePower?.Amount ?? 0;

        // 2. 核心：计算总基础伤害 (基础值 + 裁决值)
        // DamageCmd.Attack 会拿这个总值自动去吃力量、易伤等修饰，完美平替一代的 applyPowers 临时修改逻辑！
        int totalBaseDamage = (int)(DynamicVars.Damage.BaseValue + judgeAmount);

        // 3. 造成伤害，并接收结算结果
        var attackCmd = await DamageCmd.Attack(totalBaseDamage)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 4. 判定是否击杀！(利用二代官方的 WasTargetKilled 属性)
        bool wasKilled = attackCmd.Results.Any(r => r.WasTargetKilled);

        // 6. 结算最大生命值增加
        if (wasKilled)
        {
            // ⚠️ 提示：假设增加最大生命值的 API 为 CreatureCmd.IncreaseMaxHp
            // 如果报错，尝试 PlayerCmd.IncreaseMaxHp 或 Owner.Creature.IncreaseMaxHp
            await CreatureCmd.GainMaxHp(Owner.Creature, maxHpGain);
        }
    }

    // ==========================================
    // 动态 UI：实时显示裁决加成后的伤害值
    // ==========================================
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        if (IsInCombat)
        {
            var judgePower = Owner.Creature.GetPower<JudgementPower>();
            int judgeAmount = judgePower?.Amount ?? 0;

            if (judgeAmount > 0)
            {
                LocString extendedDesc = new LocString("cards", "PENANCEMOD-FINAL_VERDICT.extended_description");
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
        // 升级后最大生命值收益增加 (3 -> 4)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 1)
        {
            vars[1].UpgradeValueBy(1);
        }
    }
}