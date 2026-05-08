using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;

namespace PenanceMod.Scripts.Cards;

public class TrialDamageVar : DamageVar
{
    public TrialDamageVar(decimal damage, ValueProp props) : base(damage, props)
    {
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        decimal num = this.BaseValue;
        EnchantmentModel enchantment = card.Enchantment;

        // 1. 官方附魔逻辑
        if (enchantment != null)
        {
            num += enchantment.EnchantDamageAdditive(num, Props);
            num *= enchantment.EnchantDamageMultiplicative(num, Props);
            if (!card.IsEnchantmentPreview)
            {
                this.EnchantedValue = num;
            }
        }
        else if (!card.IsEnchantmentPreview)
        {
            this.EnchantedValue = num; 
        }

        // 2. 获取当前力量
        int strAmt = 0;
        if (card.Owner?.Creature != null)
        {
            var strPower = card.Owner.Creature.GetPower<StrengthPower>();
            strAmt = strPower != null ? strPower.Amount : 0;
        }

        // 3. 算出我们想要的乘法伤害
        decimal desiredBase = strAmt > 0 ? (num * strAmt) : 0;

        // 4. 【跟 OnPlay 保持绝对一致】：骗过底层系统
        // 把提前扣除力量的数值传给系统的 ModifyDamage，这样系统加上力量后，刚好等于我们想要的！
        num = desiredBase - strAmt;

        if (runGlobalHooks && card.Owner != null)
        {
            // 注意这里改回了 All，因为我们希望 UI 能正确计算并显示“活力”、易伤和虚弱等！
            num = Hook.ModifyDamage(card.Owner.RunState, card.CombatState, target, card.Owner.Creature, num, Props, card, ModifyDamageHookType.All, previewMode, out IEnumerable<AbstractModel> _);
        }

        this.PreviewValue = num;
    }
}