using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using PenanceMod.PenanceModCode.UI; // 引入你的 UI 脚本命名空间

namespace PenanceMod.PenanceModCode.Patches;

[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
public static class InjectBarrierUIPatch
{
    public static void Postfix(NCreature __instance)
    {
        // 拿到这个节点的内存数据本体
        var creature = __instance.Entity;
        if (creature == null) return;

        // 加载你的紫血条场景
        var barScene = GD.Load<PackedScene>("res://PenanceMod/scenes/ui/BarrierBar.tscn");
        if (barScene != null)
        {
            // 实例化
            var barInstance = barScene.Instantiate<BarrierBarUI>();
            
            // 关键：告诉这个雷达 UI，你要盯着这个 creature 看！
            barInstance.TargetCreature = creature;
            
            // 把紫血条挂到角色的 Visuals 画面节点上
            __instance.Visuals.AddChild(barInstance);
        }
    }
}