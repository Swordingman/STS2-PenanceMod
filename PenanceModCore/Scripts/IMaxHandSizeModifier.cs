using MegaCrit.Sts2.Core.Entities.Players;

namespace PenanceMod.Scripts;

public interface IMaxHandSizeModifier
{
	int ModifyMaxHandSize(Player player, int currentMaxHandSize)
	{
		return currentMaxHandSize;
	}

	int ModifyMaxHandSizeLate(Player player, int currentMaxHandSize)
	{
		return currentMaxHandSize;
	}
}
