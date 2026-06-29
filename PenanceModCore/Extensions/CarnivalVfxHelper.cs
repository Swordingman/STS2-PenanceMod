using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.GameInfo.Objects;

namespace PenanceMod.PenanceModCode.Extensions;

public static class CarnivalVfxHelper
{
    /// <summary>
    /// 播放狂欢时刻降临时的全局视听特效（暗场 -> 雷击 -> 肾上腺素 -> 恢复）
    /// </summary>
    public static async Task PlayCarnivalMomentVfx(PlayerChoiceContext choiceContext, ICombatState combatState, Creature playerCreature)
    {
        var worldEnv = NGame.Instance?.ActivateWorldEnvironment();
        float originalExposure = 1.0f; // 记录原始曝光度，以备恢复
        
        try 
        {
            // --- 演出阶段 1：渐变暗场 ---
            if (worldEnv != null && worldEnv.Environment != null)
            {
                originalExposure = worldEnv.Environment.TonemapExposure;
                var tweenDarken = worldEnv.GetTree().CreateTween();
                tweenDarken.TweenProperty(worldEnv.Environment, "tonemap_exposure", 0.15f, 0.8f); 
                await Cmd.Wait(0.8f, false); 
            }

            // --- 演出阶段 2：压迫感雷击 ---
            if (playerCreature != null)
            {
                VfxCmd.PlayOnCreatureCenter(playerCreature, VfxCmd.lightningPath);
                SfxCmd.Play("res://debug_audio/lightning_orb_evoke.mp3");
            }
            await Cmd.Wait(0.2f, false);
            
            if (combatState != null)
            {
                VfxCmd.PlayOnSide(CombatSide.Enemy, VfxCmd.lightningPath, combatState);
                SfxCmd.Play("res://debug_audio/lightning_orb_evoke.mp3");
            }
            await Cmd.Wait(0.35f, false);

            // --- 演出阶段 3：爆发特效 ---
            await AudioManager.PlayCustomSfx("res://PenanceMod/scenes/audio/trigger_wolfcurse.wav");
            VfxCmd.PlayFullScreenInCombat(VfxCmd.adrenalinePath, null);
        }
        finally
        {
            // --- 演出阶段 4：确保无论如何都会褪去暗场 ---
            if (worldEnv != null && worldEnv.Environment != null)
            {
                var tweenRestore = worldEnv.GetTree().CreateTween();
                tweenRestore.TweenProperty(worldEnv.Environment, "tonemap_exposure", originalExposure, 0.6f);
            }
        }
    }
}