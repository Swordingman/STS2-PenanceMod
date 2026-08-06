using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.PenanceModCode.Powers;

public class IroncladDoctrinePower : CustomPowerModel
{
    private const int BaseHealAmount = 3;

    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override string? CustomPackedIconPath =>
        $"res://PenanceMod/images/powers/{nameof(IroncladDoctrinePower)}.png";

    public override string? CustomBigIconPath =>
        $"res://PenanceMod/images/powers/large/{nameof(IroncladDoctrinePower)}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(
            "HealAmount",
            BaseHealAmount)
    ];

    private IntVar HealAmountVar =>
        (IntVar)DynamicVars["HealAmount"];

    public int HealAmount =>
        HealAmountVar.IntValue;

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (!ReferenceEquals(power, this))
            return Task.CompletedTask;

        /*
         * 此回调执行时，能力的 Amount 已经完成更新。
         *
         * 1 层能力恢复 3 点
         * 2 层能力恢复 6 点
         * 3 层能力恢复 9 点
         */
        HealAmountVar.BaseValue =
            BaseHealAmount * Amount;

        /*
         * Amount 本身变化时已经触发过一次 UI 更新，
         * 但当时 HealAmount 可能尚未同步。
         * 再通知一次，确保已打开的能力提示刷新。
         */
        InvokeDisplayAmountChanged();

        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (Owner != player.Creature)
            return;

        BarrierPower? barrier =
            Owner.GetPower<BarrierPower>();

        if (barrier == null || barrier.Amount < Amount)
            return;

        Flash();

        int remainingBarrier =
            barrier.Amount - Amount;

        if (remainingBarrier <= 0)
        {
            barrier.SetAmount(0);
            await PowerCmd.Remove(barrier);
        }
        else
        {
            barrier.SetAmount(
                remainingBarrier);
        }

        await CreatureCmd.Heal(
            Owner,
            HealAmount,
            true);
    }
}