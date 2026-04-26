using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class PendingJudgment : PenanceBaseCard
{
    // 耗能 1，类型 Skill，稀有度 Uncommon，目标 Self
    public PendingJudgment() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 每个攻击意图敌人提供的屏障数：8
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Pending-Barrier", 8m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        var combatState = creature.CombatState;

        if (combatState == null)
            return;

        int baseBarrier = GetBaseBarrierAmount();
        int attackIntentCount = GetAttackIntentEnemyCount();

        if (attackIntentCount <= 0)
            return;

        int totalBarrier = baseBarrier * attackIntentCount;
        await ApplyBarrier(creature, totalBarrier);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        string dynamicText = "";

        if (IsInCombat)
        {
            int baseBarrier = GetBaseBarrierAmount();
            int attackIntentCount = GetAttackIntentEnemyCount();
            int totalBarrier = baseBarrier * attackIntentCount;

            if (totalBarrier > 0)
            {
                LocString extendedDesc = new LocString(
                    "cards",
                    "PENANCEMOD-PENDING_JUDGMENT.extended_description"
                );

                extendedDesc.Add("amount", totalBarrier);
                dynamicText = extendedDesc.GetFormattedText() ?? "";
            }
        }

        description.Add("DynamicBarrierText", dynamicText);
    }

    protected override void OnUpgrade()
    {
        var vars = DynamicVars.Values.ToList();

        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(3);
        }
    }

    private int GetBaseBarrierAmount()
    {
        var vars = DynamicVars.Values.ToList();
        return vars.Count > 0 ? vars[0].IntValue : 8;
    }

    private int GetAttackIntentEnemyCount()
    {
        if (!IsInCombat)
            return 0;

        var creature = Owner.Creature;
        var combatState = creature.CombatState;

        if (combatState == null)
            return 0;

        return combatState.GetOpponentsOf(creature)
            .Count(enemy => enemy.IsAlive && IsAttackingIntent(enemy));
    }

    private bool IsAttackingIntent(Creature enemy)
    {
        return enemy.Monster?.IntendsToAttack == true;
    }
}