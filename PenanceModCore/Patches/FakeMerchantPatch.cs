using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Random;

[HarmonyPatch(typeof(NFakeMerchant), "StartCharacterAnimation")]
public static class NFakeMerchant_StartCharacterAnimation_Patch
{
    static bool Prefix(NCreatureVisuals visuals)
    {
        if (visuals == null)
            return true;

        MegaTrackEntry? entry = TrySetAnimation(visuals, "relaxed_loop");

        if (entry == null)
            entry = TrySetAnimation(visuals, "idle_loop");

        if (entry != null)
        {
            entry.SetLoop(loop: true);
            entry.SetTimeScale(Rng.Chaotic.NextFloat(0.9f, 1.1f));

            float animationEnd = entry.GetAnimationEnd();
            if (animationEnd > 0f)
                entry.SetTrackTime((animationEnd + Rng.Chaotic.NextFloat(-0.5f, 0.5f)) % animationEnd);
        }

        return false;
    }

    private static MegaTrackEntry? TrySetAnimation(NCreatureVisuals visuals, string animationName)
    {
        try
        {
            return visuals.SpineAnimation.SetAnimation(animationName);
        }
        catch (Exception e)
        {
            GD.PushWarning($"[PenanceMod]斥罪Mod报错： 设置动画失败： '{animationName}': {e.Message}");
            return null;
        }
    }
}