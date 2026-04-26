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
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.ValueProps;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Sting : PenanceBaseCard
{
    // 耗能 0，类型 Skill，稀有度 Common，目标 Self
    public Sting() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：
    // [Sting-Thorns]：荆棘层数，初始 3
    // [Sting-Judge]：裁决数值，初始 2
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Sting-Thorns", 3m),
        new DynamicVar("Sting-Judge", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        if (creature == null) return;

        // 1. 失去生命值
        var selfDamageVar = new DamageVar(3, ValueProp.Unblockable);
        await CreatureCmd.Damage(choiceContext, creature, selfDamageVar, this);

        // 稍微停顿，让扣血的红字和后面加 Buff 的特效错开，打击感更好
        await Cmd.Wait(0.1f);

        // 2. 获得荆棘环身 (使用字典式安全读取！)
        int thornsAmt = DynamicVars["Sting-Thorns"].IntValue;
        await PowerCmd.Apply<ThornAuraPower>(creature, thornsAmt, creature, this);

        // 3. 获得裁决 (复用基类快捷方法)
        int judgeAmt = DynamicVars["Sting-Judge"].IntValue;
        await ApplyJudgement(creature, judgeAmt);
    }

    protected override void OnUpgrade()
    {
        // 荆棘提升 3 (3 -> 6)
        DynamicVars["Sting-Thorns"].UpgradeValueBy(3);

        // 裁决提升 2 (2 -> 4)
        DynamicVars["Sting-Judge"].UpgradeValueBy(2);
    }
}