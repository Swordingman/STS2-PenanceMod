using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using PenanceMod.Scripts.Utils; 
using PenanceMod.PenanceModCode.Relics;
using System.Collections.Generic;

namespace PenanceMod.Patches;

[HarmonyPatch(typeof(RelicModel), "get_DynamicDescription")]
public static class ChapterOfPenance_Description_Patch
{
    static void Postfix(RelicModel __instance, ref LocString __result)
    {
        if (__instance is ChapterOfPenance penanceRelic)
        {
            LocString baseLoc = new LocString("relics", "PENANCEMOD-CHAPTER_OF_PENANCE.description");
            string allChallengesText = "";
            
            // 🌟 核心判断：数据源的选择
            List<int> challengesToDisplay = new List<int>();

            // 修复：读取刚写的 SavedChallenges 字符串
            if (!string.IsNullOrEmpty(penanceRelic.SavedChallenges))
            {
                challengesToDisplay = penanceRelic.GetChallengeList();
            }
            else 
            {
                challengesToDisplay = PenanceConfig.EnabledChallenges;
            }
            
            foreach (int id in challengesToDisplay)
            {
                string key = $"PENANCEMOD-CHAPTER_OF_PENANCE.challenge.description.{id}";
                allChallengesText += "- " + new LocString("relics", key).GetFormattedText() + "\n";
            }

            if (challengesToDisplay.Count == 0)
            {
                allChallengesText = "";
            }

            baseLoc.Add("challenges", allChallengesText.TrimEnd());
            __result = baseLoc;
        }
    }
}