using HarmonyLib;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using PenanceMod.PenanceModCode.Powers;
using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class PenanceBasicRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/{nameof(PenanceBasicRelic)}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/{nameof(PenanceBasicRelic)}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{nameof(PenanceBasicRelic)}.png";

    public static bool IsPotionActive = false;

    // 你自己的遗物计数器
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int StoredHeal { get; set; }

    // 让遗物右上角显示数字
    public override bool ShowCounter => StoredHeal > 0;

    // UI 显示的数字
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
            await PowerCmd.Apply<BarrierPower>(creature, startBarrier, creature, null);
        }

        await PowerCmd.Apply<JudgementPower>(creature, 1, creature, null);

        if (StoredHeal > 0)
        {
            await PowerCmd.Apply<BarrierPower>(creature, StoredHeal, creature, null);
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

    public override decimal ModifyRestSiteHealAmount(Creature creature, decimal amount)
    {
        if (creature == Owner.Creature && amount > 0)
        {
            TriggerHealingConversion((int)amount);
            return 0m;
        }
        return amount;
    }

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

    public void TriggerHealingConversion(int originalHealAmount)
    {
        var creature = Owner.Creature;

        if (CombatManager.Instance.IsInProgress)
        {
            Flash();
            _ = PowerCmd.Apply<BarrierPower>(creature, originalHealAmount, creature, null);
        }
        else
        {
            Flash();
            AddStoredHeal(originalHealAmount);
        }
    }
}

[HarmonyPatch(typeof(Creature), "HealInternal")]
public static class PotionHealPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Creature __instance, decimal amount)
    {
        if (!PenanceBasicRelic.IsPotionActive) return true;

        if (__instance.IsPlayer)
        {
            var relic = __instance.Player.GetRelic<PenanceBasicRelic>();
            if (relic != null && amount > 0)
            {
                relic.TriggerHealingConversion((int)amount);
                return false;
            }
        }

        return true;
    }
}