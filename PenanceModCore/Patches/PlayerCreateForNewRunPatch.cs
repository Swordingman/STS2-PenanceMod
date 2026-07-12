using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using PenanceMod.PenanceModCode.Relics;
using PenanceMod.Scripts.Utils;
using System;

namespace PenanceMod.Patches;

[HarmonyPatch(typeof(Player), nameof(Player.CreateForNewRun), new Type[] { typeof(CharacterModel), typeof(UnlockState), typeof(ulong) })]
public static class Player_CreateForNewRun_Patch
{
    public static void Postfix(Player __result)
    {
        // 兼容原版 ID 自动加前缀的情况 (日志里显示是 PENANCEMOD-PENANCE_MOD)
        if (!__result.Character.Id.Entry.Contains("PenanceMod") && !__result.Character.Id.Entry.Contains("PENANCE_MOD")) return;

        if (PenanceConfig.EnabledChallenges.Count > 0)
        {
            // 🌟 先看看玩家身上是不是已经被游戏塞了这个初始遗物
            var existingRelic = __result.GetRelic<ChapterOfPenance>();
            
            if (existingRelic != null)
            {
                // 如果有，直接把数据烙印进去
                existingRelic.SavedChallenges = string.Join(",", PenanceConfig.EnabledChallenges);
                MegaCrit.Sts2.Core.Logging.Log.Info($"[PenanceMod] 找到初始遗物，成功附魔挑战数据: {existingRelic.SavedChallenges}");
            }
            else
            {
                // 如果没有，再新建一个塞进去
                var penanceRelic = ModelDb.Relic<ChapterOfPenance>().ToMutable() as ChapterOfPenance;
                if (penanceRelic != null)
                {
                    penanceRelic.SavedChallenges = string.Join(",", PenanceConfig.EnabledChallenges);
                    MegaCrit.Sts2.Core.Logging.Log.Info($"[PenanceMod] 新建遗物，成功附魔挑战数据: {penanceRelic.SavedChallenges}");
                    __result.AddRelicInternal(penanceRelic, -1, true);
                }
            }
        }
    }
}