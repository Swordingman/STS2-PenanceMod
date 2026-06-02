using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using PenanceMod.PenanceModCode.UI;

namespace PenanceMod.PenanceModCode.Patches;

[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
public static class InjectBarrierUIPatch
{
    private static PackedScene _barSceneCache;

    private const string PenanceModelId = "CHARACTER.PENANCEMOD-PENANCE_MOD";
    private const string BarrierBarScenePath = "res://PenanceMod/scenes/BarrierBar.tscn";
    private const string BarrierBarNodeName = "Penance_BarrierBarUI";
    private const string BarrierBarInjectedMeta = "Penance_BarrierBar_Injected";

    public static void Postfix(NCreature __instance)
    {
        if (__instance == null)
            return;

        var creature = __instance.Entity;
        if (creature == null)
            return;

        string modelId = creature.ModelId.ToString();

        if (!creature.IsPlayer || modelId != PenanceModelId)
            return;

        if (__instance.HasMeta(BarrierBarInjectedMeta))
            return;

        __instance.SetMeta(BarrierBarInjectedMeta, true);

        if (_barSceneCache == null)
            _barSceneCache = GD.Load<PackedScene>(BarrierBarScenePath);

        if (_barSceneCache == null)
        {
            GD.PushWarning($"[PenanceMod] Failed to load barrier bar scene: {BarrierBarScenePath}");
            return;
        }

        var barInstance = _barSceneCache.Instantiate<BarrierBar>();
        if (barInstance == null)
        {
            GD.PushWarning("[PenanceMod] Failed to instantiate BarrierBarUI.");
            return;
        }

        barInstance.Name = BarrierBarNodeName;
        barInstance.TargetCreature = creature;

        Node parent = FindHealthBarNode(__instance);

        if (parent == null)
            parent = __instance.Visuals;

        if (parent == null)
        {
            GD.PushWarning("[PenanceMod] Could not find a parent node for BarrierBarUI.");
            barInstance.QueueFree();
            return;
        }

        if (parent is Control controlParent)
            controlParent.ClipContents = false;

        parent.CallDeferred(Node.MethodName.AddChild, barInstance);

        GD.Print("[PenanceMod] BarrierBar injected. ModelId=", modelId, ", Parent=", parent.Name);
    }

    private static Node FindHealthBarNode(Node root)
    {
        if (root == null)
            return null;

        string[] keywords =
        {
            "HealthBar",
            "HpBar",
            "HPBar",
            "Health",
            "Hp",
            "HP",
            "Block",
            "Bar"
        };

        foreach (string keyword in keywords)
        {
            Node found = FindNodeRecursive(root, node =>
            {
                string name = node.Name.ToString();
                return name.Contains(keyword, System.StringComparison.OrdinalIgnoreCase)
                    && IsGoodUiParent(node);
            });

            if (found != null)
                return found;
        }

        return null;
    }

    private static Node FindNodeRecursive(Node root, System.Func<Node, bool> predicate)
    {
        if (root == null)
            return null;

        if (predicate(root))
            return root;

        foreach (Node child in root.GetChildren())
        {
            Node found = FindNodeRecursive(child, predicate);
            if (found != null)
                return found;
        }

        return null;
    }

    private static bool IsGoodUiParent(Node node)
    {
        return node is Control || node is Node2D;
    }
}