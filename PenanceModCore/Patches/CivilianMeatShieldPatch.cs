using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using PenanceMod.PenanceModCode.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PenanceMod.PenanceModCode.Patches;

[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage),
    typeof(PlayerChoiceContext),
    typeof(IEnumerable<Creature>),
    typeof(decimal),
    typeof(ValueProp),
    typeof(Creature),
    typeof(CardModel))]
public static class CivilianMeatShieldPatch
{
    [HarmonyPrefix]
    public static void Prefix(
        ref IEnumerable<Creature> __1, // targets
        ref decimal __2,               // amount
        ValueProp __3,                 // props
        Creature? __4)                 // dealer
    {
        var targets = __1;
        var amount = __2;
        var dealer = __4;

        if (dealer == null || !dealer.IsMonster)
        {
            return;
        }

        var targetList = targets.ToList();
        bool redirectedToCivilian = false;
        bool backstab = false;

        for (int i = 0; i < targetList.Count; i++)
        {
            var target = targetList[i];

            if (!target.IsPlayer)
            {
                continue;
            }

            var surroundedPower = target.GetPower<SurroundedPower>();
            if (surroundedPower == null)
            {
                continue;
            }

            backstab =
                surroundedPower.Facing == SurroundedPower.Direction.Right &&
                dealer.HasPower<BackAttackLeftPower>()
                ||
                surroundedPower.Facing == SurroundedPower.Direction.Left &&
                dealer.HasPower<BackAttackRightPower>();

            if (!backstab)
            {
                continue;
            }

            var civilian = target.CombatState?.Creatures.FirstOrDefault(c =>
                c.IsAlive &&
                c.Monster is VolsiniiCivilian
            );

            if (civilian == null)
            {
                continue;
            }

            targetList[i] = civilian;
            redirectedToCivilian = true;
        }

        if (!redirectedToCivilian)
        {
            return;
        }

        __1 = targetList;

        if (backstab)
        {
            __2 = Math.Floor(amount * 1.5m);
        }
    }
}