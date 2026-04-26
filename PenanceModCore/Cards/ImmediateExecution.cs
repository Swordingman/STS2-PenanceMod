using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class ImmediateExecution : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Uncommon，目标 AnyEnemy
    public ImmediateExecution() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册变量：获得的裁决数 (初始 3)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Execution-Judge", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;
        
        // 获取当前变量值 (使用之前常用的安全提取法)
        var vars = DynamicVars.Values.ToList();
        int judgeGain = vars.Count > 0 ? vars[0].IntValue : 3;

        // ==========================================
        // 1. 获得裁决
        // ==========================================
        await PowerCmd.Apply<JudgementPower>(creature, judgeGain, creature, this);

        // 稍微停顿一下，让上 Buff 的特效播完，增加“行刑”的节奏感
        await Cmd.Wait(0.2f);

        // ==========================================
        // 2. 获取当前的裁决总层数 (此时必定已经包含了刚刚加上的层数)
        // ==========================================
        int totalJudgement = creature.GetPower<JudgementPower>()?.Amount ?? 0;

        // ==========================================
        // 3. 触发裁决伤害
        // ==========================================
        // ⚠️ 提示：如果这个裁决伤害是纯粹伤害（不吃力量、易伤），请加上 .Unpowered()
        // 如果它作为一张攻击牌需要吃力量加成，则保持原样。
        if (totalJudgement > 0)
        {
            await DamageCmd.Attack(totalJudgement)
                .FromCard(this)
                .Targeting(target)
                // .Unpowered() // 如果不吃力量加成，把这行的注释解开
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级获得的裁决数 +3 (3 -> 6)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(3);
        }
    }
}