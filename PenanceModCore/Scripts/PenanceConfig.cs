using BaseLib.Config;

namespace PenanceMod.Scripts.Utils;

public sealed class PenanceConfig : SimpleModConfig
{
    [ConfigHideInUI]
    public static int CurrentSkinIndex { get; set; } = 0;
}