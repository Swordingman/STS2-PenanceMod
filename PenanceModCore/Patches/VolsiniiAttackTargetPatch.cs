using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Models;
using PenanceMod.PenanceModCode.Monsters;
using System.Reflection;

namespace PenanceMod.PenanceModCode.Patches;

[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.FromMonster))]
public static class VolsiniiAttackTargetPatch
{
    private static readonly FieldInfo? CombatStateField = AccessTools.Field(typeof(AttackCommand), "_combatState");

    [HarmonyPostfix]
    public static void Postfix(AttackCommand __instance, MonsterModel monster)
    {
        if (monster is not VolsiniiMobAgile && monster is not VolsiniiMobHeavy)
            return;

        CombatStateField?.SetValue(__instance, null);
    }
}