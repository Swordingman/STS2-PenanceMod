using HarmonyLib;
using MegaCrit.Sts2.Core.Models; // PowerModel 所在命名空间
using PenanceMod.PenanceModCode.Powers; 
using PenanceMod.PenanceModCode.Relics; 

namespace PenanceMod.Scripts.Patches;

// 🌟 抛弃泛型！直接拦截所有能力层数发生变动的最底层方法 SetAmount
[HarmonyPatch(typeof(PowerModel), "SetAmount")]
public static class LetterOfGratitude_Barrier_Patch
{
    // 🌟 终极必杀：用 __0 代表原方法的第一个参数，绕过所有命名陷阱！
    [HarmonyPrefix]
    public static void Prefix(PowerModel __instance, ref int __0)
    {
        // 1. 确保当前正在修改的是“屏障”，且拥有者是玩家
        if (__instance is BarrierPower && __instance.Owner != null && __instance.Owner.IsPlayer)
        {
            // 2. 核心判定：只有新数值大于老数值，才说明是在“获得”屏障！
            // （如果是挨打扣除屏障，新数值会变小，绝对不能触发加成）
            if (__0 > __instance.Amount) 
            {
                var relic = __instance.Owner.Player.GetRelic<LetterOfGratitude>();
                if (relic != null)
                {
                    // 3. 计算这次具体获得了多少层屏障
                    int gainedAmount = __0 - __instance.Amount;

                    // 4. 读取遗物的变量 (20% 和 1)
                    decimal pct = relic.DynamicVars["Gratitude-Pct"].BaseValue / 100m; 
                    int minBonus = relic.DynamicVars["Gratitude-Min"].IntValue;        

                    // 5. 计算额外加成
                    int bonus = (int)(gainedAmount * pct);
                    if (bonus < minBonus)
                    {
                        bonus = minBonus;
                    }

                    // 6. 强行把加成塞进即将生效的新数值里！
                    __0 += bonus;
                    relic.Flash();
                }
            }
        }
    }
}