using MegaCrit.Sts2.Core.Entities.Players;

namespace PenanceMod.Scripts.Utils;

public static class PenanceSkinState
{
    private const int SkinCount = 3;
    private static readonly Dictionary<ulong, int> SkinIndices = new();

    public static int Normalize(int index)
    {
        index %= SkinCount;
        if (index < 0) index += SkinCount;
        return index;
    }

    public static void SetSkin(ulong playerNetId, int skinIndex)
    {
        SkinIndices[playerNetId] = Normalize(skinIndex);
    }

    public static int GetSkin(ulong playerNetId)
    {
        return SkinIndices.TryGetValue(playerNetId, out int skinIndex)
            ? Normalize(skinIndex)
            : 0;
    }

    public static int GetSkin(Player? player)
    {
        return player == null ? 0 : GetSkin(player.NetId);
    }

    public static void RemovePlayer(ulong playerNetId)
    {
        SkinIndices.Remove(playerNetId);
    }

    public static void Clear()
    {
        SkinIndices.Clear();
    }
}