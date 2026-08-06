using BaseLib.Config;
using Godot.Bridge;
using HarmonyLib;
using PenanceMod.Scripts.Cards;
using PenanceMod.Scripts.Utils;

namespace PenanceMod.PenanceModCode;

public partial class MainFile
{
    private static bool _initialized;

    public static void Init()
    {
        if (_initialized)
        {
            Console.WriteLine(
                "[PenanceMod] MainFile 已初始化，跳过重复调用。");

            return;
        }

        try
        {
            var assembly = typeof(MainFile).Assembly;

            #if !STS2_BETA
            /*
             * Stable 的 ModManager 最初会把 Loader 程序集登记为
             * PenanceMod 的主程序集。
             *
             * 这里注册一个一次性事件处理器。等 ModManager 完成本次
             * Mod 初始化并触发 OnModDetected 时，再把登记程序集替换为
             * PenanceMod.Stable.dll。
             */
            StableAssemblyRegistration.Install(assembly);
            #endif

            var harmony = new Harmony("PenanceMod");

            harmony.PatchAll(assembly);

            ScriptManagerBridge.LookupScriptsInAssembly(assembly);

            ModConfigRegistry.Register(
                "PenanceMod",
                new PenanceConfig());

            _initialized = true;

            Console.WriteLine(
                $"[PenanceMod] MainFile 初始化完成，程序集："
                + $"{assembly.GetName().Name}");
        }
        catch (Exception exception)
        {
            _initialized = false;

            Console.Error.WriteLine(
                $"[PenanceMod] MainFile 初始化失败：{exception}");

            throw;
        }
    }
}