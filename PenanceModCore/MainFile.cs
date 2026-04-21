using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Logging;

namespace PenanceMod.PenanceModCode;

[ModInitializer("Init")]
public partial class MainFile
{
    public static void Init()
    {
        // 打patch（即修改游戏代码的功能）用
        // 传入参数随意，只要不和其他人撞车即可
        var harmony = new Harmony("PenanceMod");
        harmony.PatchAll();
        // 使得tscn可以加载自定义脚本
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(MainFile).Assembly);
        Log.Info("斥罪已准备好审判！");
    }
}