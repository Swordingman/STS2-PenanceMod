using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using PenanceMod.PenanceModCode.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Combat;
using System.Threading.Tasks;
using PenanceMod.PenanceModCode.Character;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class ThornyRoad : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/large/{nameof(ThornyRoad)}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/large/{nameof(ThornyRoad)}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{nameof(ThornyRoad)}.png";

    public bool IsPotionActive { get; set; } = false;

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int PenanceMod_StoredHeal { get; set; }

    public override bool ShowCounter => PenanceMod_StoredHeal > 0;
    public override int DisplayAmount => PenanceMod_StoredHeal;

    public void AddStoredHeal(int amount)
    {
        if (amount <= 0) return;
        PenanceMod_StoredHeal += amount;
        InvokeDisplayAmountChanged();
    }

    private void ClearStoredHeal()
    {
        if (PenanceMod_StoredHeal == 0) return;
        PenanceMod_StoredHeal = 0;
        InvokeDisplayAmountChanged();
    }

    public override async Task BeforeCombatStart()
    {
        var player = Owner;
        var creature = player.Creature;

        Flash();

        int startBarrier = (int)(creature.MaxHp * 0.30f);
        if (startBarrier > 0)
        {
            await PowerCmd.Apply<BarrierPower>(new ThrowingPlayerChoiceContext(), creature, startBarrier, creature, null);
        }

        await PowerCmd.Apply<JudgementPower>(new ThrowingPlayerChoiceContext(), creature, 3, creature, null);

        if (PenanceMod_StoredHeal > 0)
        {
            await PowerCmd.Apply<BarrierPower>(new ThrowingPlayerChoiceContext(), creature, PenanceMod_StoredHeal, creature, null);
            ClearStoredHeal();
        }
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (target != Owner.Creature && CombatManager.Instance.IsInProgress)
        {
            Flash(); 
            int killReward = (int)(Owner.Creature.MaxHp * 0.10f);
            AddStoredHeal(killReward);
        }
        return Task.CompletedTask;
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
            _ = PowerCmd.Apply<BarrierPower>(new ThrowingPlayerChoiceContext(), creature, originalHealAmount, creature, null);
        }
        else
        {
            Flash();
            AddStoredHeal(originalHealAmount);
        }
    }
}