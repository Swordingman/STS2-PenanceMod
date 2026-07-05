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
            // 1. 先执行设置动画的操作（因为它是 void，所以单列一行）
            visuals.SpineAnimation.SetAnimation(animationName);

            // 2. 然后获取刚刚设置的动画轨道并返回。
            return visuals.SpineAnimation.GetCurrentTrack(0); 
        }
        catch (Exception e)
        {
            GD.PushWarning($"[PenanceMod]斥罪Mod报错： 设置动画失败： '{animationName}': {e.Message}");
            return null;
        }
    }
}