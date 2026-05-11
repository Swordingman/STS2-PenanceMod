using HarmonyLib;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms; // 需要引入 Room 命名空间来判断篝火
using PenanceMod.PenanceModCode.Powers;
using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class PenanceBasicRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/large/{nameof(PenanceBasicRelic)}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/large/{nameof(PenanceBasicRelic)}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{nameof(PenanceBasicRelic)}.png";

    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<ThornyRoad>();

    // 完美保留你药水检测的逻辑
    public static bool IsPotionActive = false;

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int StoredHeal { get; set; }

    public override bool ShowCounter => StoredHeal > 0;
    public override int DisplayAmount => StoredHeal;

    private void AddStoredHeal(int amount)
    {
        if (amount <= 0) return;
        StoredHeal += amount;
        InvokeDisplayAmountChanged();
    }

    private void ClearStoredHeal()
    {
        if (StoredHeal == 0) return;
        StoredHeal = 0;
        InvokeDisplayAmountChanged();
    }

    public override async Task BeforeCombatStart()
    {
        var player = Owner;
        var creature = player.Creature;

        Flash();

        int startBarrier = (int)(creature.MaxHp * 0.10f);
        if (startBarrier > 0)
        {
            await PowerCmd.Apply<BarrierPower>(new ThrowingPlayerChoiceContext(), creature, startBarrier, creature, null);
        }

        await PowerCmd.Apply<JudgementPower>(new ThrowingPlayerChoiceContext(), creature, 1, creature, null);

        if (StoredHeal > 0)
        {
            await PowerCmd.Apply<BarrierPower>(new ThrowingPlayerChoiceContext(), creature, StoredHeal, creature, null);
            ClearStoredHeal();
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var creature = Owner.Creature;
        var barrier = creature.GetPower<BarrierPower>();

        if (barrier != null && barrier.Amount > 0)
        {
            int healAmount = (int)(barrier.Amount * 0.10f);
            if (healAmount > 0)
            {
                Flash();
                await CreatureCmd.Heal(creature, healAmount);
            }
        }
    }

    // 完美保留你的药水开关
    public override Task BeforePotionUsed(PotionModel potion, Creature? target)
    {
        IsPotionActive = true;
        return Task.CompletedTask;
    }

    public override Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        IsPotionActive = false;
        return Task.CompletedTask;
    }

    // ❌ 删除了 ModifyRestSiteHealAmount 方法！这彻底杜绝了 UI 预览引发的无限触发 Bug。

    public void TriggerHealingConversion(int originalHealAmount)
    {
        var creature = Owner.Creature;

        if (CombatManager.Instance.IsInProgress)
        {
            Flash();
            _ = PowerCmd.Apply<BarrierPower>(new ThrowingPlayerChoiceContext(), creature, originalHealAmount, creature, null);
        }
        else
        {
            Flash();
            AddStoredHeal(originalHealAmount);
        }
    }
}

// ✅ 精准拦截补丁：只在“药水状态”或“身处篝火房间”时拦截
[HarmonyPatch(typeof(Creature), "HealInternal")]
public static class PotionHealPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Creature __instance, decimal amount)
    {
        // 排除非玩家和无意义的数值
        if (!__instance.IsPlayer || amount <= 0) return true;

        // 🌟 核心修正：濒死抢救豁免！
        // 如果玩家血量已经归零或更低，说明这是复活类道具（如瓶装精灵）在救命。
        // 绝对不能把救命血转成屏障，直接放行原版回血！
        if (__instance.CurrentHp <= 0) return true;

        var relic = __instance.Player.GetRelic<PenanceBasicRelic>();
        if (relic != null)
        {
            // 条件 1：药水是否正在生效
            bool isPotion = PenanceBasicRelic.IsPotionActive;

            // 条件 2：玩家当前是否在篝火房间
            bool isAtCampfire = __instance.Player.RunState.CurrentRoom is RestSiteRoom;

            // 只拦截药水和休息处
            if (isPotion || isAtCampfire)
            {
                relic.TriggerHealingConversion((int)amount);
                return false; // 阻断原版回血
            }
        }

        return true;
    }
}