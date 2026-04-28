using System.Reflection;
using System.Reflection.Emit;
using BaseLib.Hooks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace PenanceMod.Scripts;

public static class MaxHandSizePatch
{
	public const int DefaultMaxHandSize = 10;

	internal static readonly MethodInfo GetMaxHandSizeMethod = AccessTools.Method(typeof(MaxHandSizePatch), "GetMaxHandSize");

	internal static bool IsDefaultMaxHandSizeConst(CodeInstruction ins)
	{
		if (!(ins.opcode == OpCodes.Ldc_I4_S) || !(ins.operand is sbyte b) || b != 10)
		{
			if (ins.opcode == OpCodes.Ldc_I4 && ins.operand is int num)
			{
				return num == 10;
			}
			return false;
		}
		return true;
	}

	public static int GetMaxHandSize(Player player)
	{
		IRunState obj = player.RunState ?? NullRunState.Instance;
		CombatState combatState = player.Creature.CombatState;
		int num = 10;
		List<IMaxHandSizeModifier> list = new List<IMaxHandSizeModifier>();
		foreach (AbstractModel item2 in obj.IterateHookListeners(combatState))
		{
			if (item2 is IMaxHandSizeModifier item)
			{
				list.Add(item);
			}
		}
		foreach (IMaxHandSizeModifier item3 in list)
		{
			num = item3.ModifyMaxHandSize(player, num);
		}
		foreach (IMaxHandSizeModifier item4 in list)
		{
			num = item4.ModifyMaxHandSizeLate(player, num);
		}
		return Math.Max(0, num);
	}

	internal static int GetMaxHandSizeFromCard(CardModel? card)
	{
		Player player = card?.Owner;
		if (player == null)
		{
			return 10;
		}
		return GetMaxHandSize(player);
	}
}
