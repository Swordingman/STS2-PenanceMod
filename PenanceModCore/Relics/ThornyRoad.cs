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
    // 🌟 稀有度定为 Boss，通常用于替换初始遗物
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/large/{nameof(ThornyRoad)}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/large/{nameof(ThornyRoad)}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{nameof(ThornyRoad)}.png";

    // 同样需要药水开关
    public static bool IsPotionActive = false;

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int StoredHeal { get; set; }

    public override bool ShowCounter => StoredHeal > 0;
    public override int DisplayAmount => StoredHeal;

    public void AddStoredHeal(int amount)
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

        // 🌟 升级点 1：开局屏障提升至 30%
        int startBarrier = (int)(creature.MaxHp * 0.30f);
        if (startBarrier > 0)
        {
            await PowerCmd.Apply<BarrierPower>(new ThrowingPlayerChoiceContext(), creature, startBarrier, creature, null);
        }

        // 🌟 升级点 2：开局裁决提升至 3 层
        await PowerCmd.Apply<JudgementPower>(new ThrowingPlayerChoiceContext(), creature, 3, creature, null);

        // 释放存储的屏障
        if (StoredHeal > 0)
        {
            await PowerCmd.Apply<BarrierPower>(new ThrowingPlayerChoiceContext(), creature, StoredHeal, creature, null);
            ClearStoredHeal();
        }
    }

    // 🌟 升级点 3：击杀敌人时，存储 10% 最大生命值的屏障
    // ⚠️ 提示：如果引擎报错说找不到 OnCreatureDied 方法，请在 public override 后敲空格，查找带有 Death / Die / Kill 字眼的钩子并替换名字。
    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        // 确保死的是敌人（不是玩家自己），且处于战斗中
        if (target != Owner.Creature && CombatManager.Instance.IsInProgress)
        {
            Flash(); // 闪烁一下遗物，给玩家正反馈
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