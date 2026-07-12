using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Rooms;
using PenanceMod.PenanceModCode.Relics;

namespace PenanceMod.PenanceModCode.Patches;

[HarmonyPatch(typeof(Creature), "HealInternal")]
public static class PenanceHealingPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Creature __instance, decimal amount)
    {
        // 排除非玩家和无意义的数值
        if (!__instance.IsPlayer || amount <= 0 || __instance.Player == null) return true;

        // 🌟 濒死抢救豁免：绝对不能把救命血转成屏障
        if (__instance.CurrentHp <= 0) return true;

        // 条件：玩家当前是否在篝火房间
        bool isAtCampfire = __instance.Player.RunState.CurrentRoom is RestSiteRoom;

        // --- 检查 1：基础遗物 ---
        var basicRelic = __instance.Player.GetRelic<PenanceBasicRelic>();
        if (basicRelic != null)
        {
            if (basicRelic.IsPotionActive || isAtCampfire)
            {
                basicRelic.TriggerHealingConversion((int)amount);
                return false; // 阻断原版回血
            }
        }

        // --- 检查 2：升级版遗物 ---
        var upgradedRelic = __instance.Player.GetRelic<ThornyRoad>();
        if (upgradedRelic != null)
        {
            if (upgradedRelic.IsPotionActive || isAtCampfire)
            {
                upgradedRelic.TriggerHealingConversion((int)amount);
                return false; // 阻断原版回血
            }
        }

        return true;
    }
}