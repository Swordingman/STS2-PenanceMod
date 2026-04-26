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
public class JudgementBacklash : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Uncommon，目标 AnyEnemy
    public JudgementBacklash() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册变量：基础伤害 3
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;

        // 1. 计算总攻击次数
        // 逻辑：基础 1 次 + (裁决层数 / 3)
        int judgeAmount = creature.GetPower<JudgementPower>()?.Amount ?? 0;
        int extraHits = judgeAmount / 3;
        int totalHits = 1 + extraHits;

        // 2. 依次执行攻击
        for (int i = 0; i < totalHits; i++)
        {
            // 每次循环都检查一下目标是否还活着（防止鞭尸）
            if (target.IsDead) break;

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);

            // 如果还有下一发，稍微等待一下，营造“连发”的视觉效果
            if (i < totalHits - 1)
            {
                await Cmd.Wait(0.15f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 基础伤害从 3 提升到 5
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}