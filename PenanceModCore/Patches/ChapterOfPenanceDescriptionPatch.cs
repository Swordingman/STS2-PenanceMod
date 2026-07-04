using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using PenanceMod.Scripts.Utils; // 确保引入了你的 PenanceConfig
using PenanceMod.PenanceModCode.Relics;

namespace PenanceMod.Patches
{
    // 拦截 RelicModel 的 DynamicDescription 属性的 Getter 方法
    [HarmonyPatch(typeof(RelicModel), "get_DynamicDescription")]
    public static class ChapterOfPenance_Description_Patch
    {
        static void Postfix(RelicModel __instance, ref LocString __result)
        {
            // 如果当前读取描述的遗物是我们的“苦修之章”
            if (__instance is ChapterOfPenance)
            {
                LocString baseLoc = new LocString("relics", "PENANCEMOD-CHAPTER_OF_PENANCE.description");
                
                string allChallengesText = "";
                
                // 遍历玩家勾选的所有挑战 ID，拼接具体的描述
                foreach (int id in PenanceConfig.EnabledChallenges)
                {
                    string key = $"PENANCEMOD-CHAPTER_OF_PENANCE.challenge.description.{id}";
                    // 确保换行符拼接正确
                    allChallengesText += "- " + new LocString("relics", key).GetFormattedText() + "\n";
                }

                // 如果玩家一个挑战都没选，给个默认提示，防止留空难看
                if (PenanceConfig.EnabledChallenges.Count == 0)
                {
                    allChallengesText = "无附加苦修。";
                }

                // 把拼接好的文本塞进占位符
                baseLoc.Add("challenges", allChallengesText.TrimEnd());
                
                // 替换掉原版返回的 LocString
                __result = baseLoc;
            }
        }
    }
}