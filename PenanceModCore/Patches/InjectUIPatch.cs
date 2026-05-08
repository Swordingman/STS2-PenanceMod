// using HarmonyLib;
// using Godot;
// using MegaCrit.Sts2.Core.Nodes.Combat;
// using PenanceMod.PenanceModCode.UI; 

// namespace PenanceMod.PenanceModCode.Patches;

// [HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
// public static class InjectBarrierUIPatch
// {
//     // 依然保留缓存，避免每次读取硬盘掉帧
//     private static PackedScene _barSceneCache;

//     private const string PenanceId = "PenanceMod"; 

//     public static void Postfix(NCreature __instance)
//     {
//         var creature = __instance.Entity;
//         if (creature == null) return;

//         // 【终极过滤】：必须是玩家，且 ModelId 必须是你的自定义角色
//         if (!creature.IsPlayer || creature.ModelId.ToString() != PenanceId) 
//         {
//             return; 
//         }

//         if (_barSceneCache == null)
//         {
//             _barSceneCache = GD.Load<PackedScene>("res://PenanceMod/scenes/ui/BarrierBar.tscn");
//         }

//         if (_barSceneCache != null)
//         {
//             var barInstance = _barSceneCache.Instantiate<BarrierBarUI>();
//             barInstance.TargetCreature = creature;
            
//             // 延迟挂载，避免时序问题导致的不显示或报错
//             __instance.Visuals.CallDeferred(Node.MethodName.AddChild, barInstance);
//         }
//     }
// }