using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace PenanceMod.PenanceModCode.Powers;

public class IroncladDoctrinePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(IroncladDoctrinePower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(IroncladDoctrinePower)}.png";

    // 基础回复量常量
    private const int BaseHealAmt = 3;

    // 自定义属性：用于记录当前堆叠的回血量
    // 设为 public 属性，方便底层系统反射和文本读取
    public int HealAmount { get; set; } = BaseHealAmt;

    // ==========================================
    // 核心一：自定义双层堆叠逻辑
    // ==========================================
    // 当这个能力的层数发生改变（通常是被再次施加叠加时）触发
    public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // 确保是自己这个能力发生改变，且 amount > 0 表示“叠加增加”而不是扣除
        if (power == this && amount > 0)
        {
            // 每次叠加，回血量增加 3
            HealAmount += BaseHealAmt;
        }
        return Task.CompletedTask;
    }

    // ==========================================
    // 核心二：回合开始结算
    // ==========================================
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == player.Creature)
        {
            // 1. 获取玩家身上的屏障
            var barrier = Owner.GetPower<BarrierPower>();

            // 2. 判断屏障是否存在，且层数是否满足消耗条件 (Amount)
            if (barrier != null && barrier.Amount >= Amount)
            {
                Flash(); // 闪烁图标

                // 3. 消耗屏障
                int remainingBarrier = barrier.Amount - (int)Amount;
                if (remainingBarrier <= 0)
                {
                    barrier.SetAmount(0); // 视觉归零
                    await PowerCmd.Remove(barrier); // 彻底移除
                }
                else
                {
                    barrier.SetAmount(remainingBarrier); // 扣减层数
                }

                // 4. 恢复生命 (调用原生 Heal 指令，自带绿字飘动和音效)
                await CreatureCmd.Heal(Owner, HealAmount, true);
            }
        }
    }
}