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
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class GuiltyAsCharged : PenanceBaseCard
{
    // 耗能 2，类型 Attack，稀有度 Rare，目标 AnyEnemy
    public GuiltyAsCharged() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册变量：基础伤害 15
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(15, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;

        // 1. 获取当前力量值，决定循环次数
        int strengthAmount = creature.GetPower<StrengthPower>()?.Amount ?? 0;
        int repeatTimes = strengthAmount >= 10 ? 2 : 1;

        // 2. 击碎护甲 (移除所有格挡)
        // ⚠️ 提示：假定官方的底层移除格挡 API 如下。如果报错，请尝试 target.Hp.Block = 0 或查找 RemoveBlock 相关方法。
        if (target.Block > 0)
        {
            await CreatureCmd.LoseBlock(target, target.Block);
            await Cmd.Wait(0.1f); // 碎甲后稍作停顿，增加打击感
        }

        // 3. 核心循环判定
        for (int i = 0; i < repeatTimes; i++)
        {
            // 3.1 基础伤害
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);

            // 3.2 判定裁决：实时拉取层数
            int judgeAmount = creature.GetPower<JudgementPower>()?.Amount ?? 0;
            if (judgeAmount >= 10)
            {
                await Cmd.Wait(0.1f);
                // 使用 Unpowered 模拟一代的 Thorns (荆棘伤害，不吃力量等修饰)
                await DamageCmd.Attack(judgeAmount)
                    .FromCard(this)
                    .Targeting(target)
                    .Unpowered() 
                    .Execute(choiceContext);
            }

            // 3.3 判定荆棘环身：实时拉取层数
            int thornsAmount = creature.GetPower<ThornAuraPower>()?.Amount ?? 0;
            if (thornsAmount >= 10)
            {
                await PowerCmd.Apply<CeasefirePower>(target, 1, creature, this);
            }

            // 如果触发了力量的双倍效果，在两套连击之间加上演出停顿
            if (i < repeatTimes - 1)
            {
                await Cmd.Wait(0.2f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 5 (15 -> 20)
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}