using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Linq;

namespace PenanceMod.PenanceModCode.Monsters;

public static class VolsiniiCourtTargeting
{
    public static Creature GetAttackTarget(Creature attacker, Creature originalTarget)
    {
        if (!originalTarget.IsPlayer)
            return originalTarget;

        Creature? civilian = originalTarget.CombatState?.Creatures.FirstOrDefault(c =>
            c.IsAlive && c.Monster is VolsiniiCivilian
        );

        if (civilian == null)
            return originalTarget;

        SurroundedPower? surrounded = originalTarget.GetPower<SurroundedPower>();

        if (surrounded == null)
            return originalTarget;

        bool playerFacingAttacker =
            surrounded.Facing == SurroundedPower.Direction.Left && attacker.HasPower<BackAttackLeftPower>() ||
            surrounded.Facing == SurroundedPower.Direction.Right && attacker.HasPower<BackAttackRightPower>();

        return playerFacingAttacker ? originalTarget : civilian;
    }
}