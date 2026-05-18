using HarmonyLib;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; // 引入你的屏障能力所在的命名空间

namespace PenanceMod.PenanceModCode.Patches;

[HarmonyPatch(typeof(ImbalancedPower), nameof(ImbalancedPower.AfterDamageGiven))]
public static class ImbalancedBarrierPatch
{
    [HarmonyPrefix]
    public static bool Prefix(
        ImbalancedPower __instance, 
        ref Task __result, // 🌟 核心技巧：接管原版方法的返回值
        PlayerChoiceContext choiceContext, 
        Creature? dealer, 
        DamageResult result, 
        ValueProp props, 
        Creature target, 
        CardModel? cardSource)
    {
        // 1. 如果原版已经判定为“完全格挡”了，直接放行，让原版自己处理
        if (result.WasFullyBlocked) return true;

        // 2. 如果攻击者不是怪物自己，直接放行
        if (dealer != __instance.Owner) return true;

        // 3. 核心判定：经过层层削减，玩家最终的掉血量是不是 0？
        if (result.UnblockedDamage <= 0)
        {
            // 4. 检查被打的 target（通常是玩家）身上有没有我们的特有屏障
            var barrier = target.GetPower<BarrierPower>();
            
            if (barrier != null)
            {
                // 5. 条件全部满足！屏障立大功！
                // 我们调用自己写的异步逻辑赋值给 __result，然后阻断原版
                __result = CustomStunLogic(__instance);
                return false; 
            }
        }

        // 其他情况一律放行
        return true;
    }

    // 将原版源码里的击晕逻辑原封不动地搬过来，包装成一个异步任务
    private static async Task CustomStunLogic(ImbalancedPower instance)
    {
        AccessTools.Method(typeof(PowerModel), "Flash")?.Invoke(instance, null);
        
        // 完美复刻原版：判断是不是特定的盛碗虫
        if (!(instance.Owner.Monster is BowlbugRock bowlbugRock))
        {
            await CreatureCmd.Stun(instance.Owner);
        }
        else
        {
            bowlbugRock.IsOffBalance = true;
        }
    }
}