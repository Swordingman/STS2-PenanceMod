using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;

#if STS2_BETA
using MegaCrit.Sts2.Core.Entities.Cards;
#endif

namespace PenanceMod.PenanceModCode.Patches;

#if STS2_BETA
[HarmonyPatch(
    typeof(CreatureCmd),
    nameof(CreatureCmd.Damage),
    typeof(PlayerChoiceContext),
    typeof(IEnumerable<Creature>),
    typeof(decimal),
    typeof(ValueProp),
    typeof(Creature),
    typeof(CardModel),
    typeof(CardPlay))]
#else
[HarmonyPatch(
    typeof(CreatureCmd),
    nameof(CreatureCmd.Damage),
    typeof(PlayerChoiceContext),
    typeof(IEnumerable<Creature>),
    typeof(decimal),
    typeof(ValueProp),
    typeof(Creature),
    typeof(CardModel))]
#endif
public static class CivilianMeatShieldPatch
{
    [HarmonyPrefix]
    public static void Prefix(
        ref IEnumerable<Creature> __1,
        ref decimal __2,
        ValueProp __3,
        Creature? __4)
    {
        IEnumerable<Creature> targets = __1;
        decimal amount = __2;
        Creature? dealer = __4;

        if (dealer == null || !dealer.IsMonster)
            return;

        List<Creature> targetList = targets.ToList();
        bool redirectedToCivilian = false;
        bool hasBackstab = false;

        for (int i = 0; i < targetList.Count; i++)
        {
            Creature target = targetList[i];

            if (!target.IsPlayer)
                continue;

            SurroundedPower? surroundedPower = target.GetPower<SurroundedPower>();

            if (surroundedPower == null)
                continue;

            bool currentTargetBackstab =
                surroundedPower.Facing == SurroundedPower.Direction.Right
                && dealer.HasPower<BackAttackLeftPower>()
                ||
                surroundedPower.Facing == SurroundedPower.Direction.Left
                && dealer.HasPower<BackAttackRightPower>();

            if (!currentTargetBackstab)
                continue;

            Creature? civilian = target.CombatState?.Creatures.FirstOrDefault(
                creature =>
                    creature.IsAlive
                    && creature.Monster is VolsiniiCivilian);

            if (civilian == null)
                continue;

            targetList[i] = civilian;
            redirectedToCivilian = true;
            hasBackstab = true;
        }

        if (!redirectedToCivilian)
            return;

        __1 = targetList;

        if (hasBackstab)
            __2 = Math.Floor(amount * 1.5m);
    }
}