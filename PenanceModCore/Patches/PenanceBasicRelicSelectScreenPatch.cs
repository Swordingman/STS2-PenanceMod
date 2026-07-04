using HarmonyLib;
using Godot;
using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace PenanceMod.Patches
{
    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))]
    public class PenanceBasicRelic_SelectScreen_Patch
    {
        static void Postfix(NCharacterSelectScreen __instance, NCharacterSelectButton charSelectButton, CharacterModel characterModel)
        {
            if (charSelectButton.IsLocked || charSelectButton.IsRandom)
                return;

            // 使用 Contains 忽略大小写进行匹配，防止游戏底层修改了 ID 字符串格式
            if (characterModel.Id.Entry.Contains("Penance", System.StringComparison.OrdinalIgnoreCase))
            {
                FieldInfo? relicDescField = typeof(NCharacterSelectScreen).GetField("_relicDescription", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (relicDescField != null)
                {
                    object? relicDescriptionUI = relicDescField.GetValue(__instance);
                    
                    // 将 UI 转换为 Godot.Node，直接利用 Godot 引擎底层的 Set 方法赋值，这比 C# 纯反射稳得多
                    if (relicDescriptionUI is Godot.Node godotNode)
                    {
                        string shortDesc = new LocString("relics", "PENANCEMOD-PENANCE_BASIC_RELIC.short_description").GetFormattedText();
                        
                        // Godot 的 Node 属性在底层通常用小写 snake_case
                        godotNode.Set("text", shortDesc);
                    }
                    else
                    {
                        GD.PrintErr("[PenanceMod] 失败：未能将 _relicDescription 识别为 Godot 节点。");
                    }
                }
                else
                {
                    GD.PrintErr("[PenanceMod] 失败：找不到 _relicDescription 字段。");
                }
            }
        }
    }
}