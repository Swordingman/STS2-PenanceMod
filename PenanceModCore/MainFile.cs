using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Hooks;
using PenanceMod.Scripts.Cards;

namespace PenanceMod.PenanceModCode;

[ModInitializer("Init")]
public partial class MainFile
{
    public static void Init()
    {
        var harmony = new Harmony("PenanceMod");
        harmony.PatchAll();
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(MainFile).Assembly);
        Log.Info("斥罪已准备好审判！");
    }
}