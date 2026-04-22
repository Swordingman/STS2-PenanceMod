using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;

// 无论如何都要引用的命名空间
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace PenanceMod.PenanceModCode.Powers;

public class JustifiedDefenseTriggeredPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(JustifiedDefenseTriggeredPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(JustifiedDefenseTriggeredPower)}.png";

    // ==========================================
    // 核心逻辑：回合开始时爆发
    // ==========================================
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 确保只有当拥有者是当前玩家且层数大于 0 时才触发
        if (Owner == player.Creature && Amount > 0)
        {
            Flash(); // 闪烁图标

            // 1. 获得能量 (使用你之前查到的接口，记得传 Owner.Player)
            // 这里 Amount 是 decimal，EnergyGain 也是 decimal
            await PlayerCmd.GainEnergy(Amount, Owner.Player);

            // 2. 抽牌 (数量等同于层数)
            await CardPileCmd.Draw(choiceContext, (int)Amount, player);

            // 3. 移除自身
            await PowerCmd.Remove<JustifiedDefenseTriggeredPower>(Owner);
        }
    }
}