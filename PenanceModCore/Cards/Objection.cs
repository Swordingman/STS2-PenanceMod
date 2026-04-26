using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Objection : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Common，目标 AnyEnemy
    public Objection() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册变量：[0] 伤害(9), [1] 小效果数值(4), [2] 大效果数值(8)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("Obj-Small", 4m),
        new DynamicVar("Obj-Large", 8m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        var vars = DynamicVars.Values.ToList();
        int smallAmt = vars.Count > 1 ? vars[1].IntValue : 4;
        int largeAmt = vars.Count > 2 ? vars[2].IntValue : 8;

        await Cmd.Wait(0.15f);

        if (IsAttackingIntent(target))
        {
            await ApplyJudgement(creature, smallAmt);
        }
        else if (IsDefendingIntent(target))
        {
            await PowerCmd.Apply<ThornAuraPower>(creature, smallAmt, creature, this);
        }
        else
        {
            await ApplyBarrier(creature, largeAmt);
        }
    }

    protected override void OnUpgrade()
    {
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 2)
        {
            vars[1].UpgradeValueBy(1);
            vars[2].UpgradeValueBy(2);
        }
    }

    // ==========================================
    // 🔍 辅助方法：二代的意图判定逻辑
    // ==========================================
    private bool IsAttackingIntent(Creature target)
    {
        return target.Monster?.IntendsToAttack == true;
    }

    private bool IsDefendingIntent(Creature target)
    {
        if (target.Monster == null)
            return false;

        return target.Monster.NextMove.Intents.Any(intent =>
            intent.IntentType == IntentType.Defend);
    }
}