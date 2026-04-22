using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;

// 必须引用的命名空间，用于获取原版的 力量(StrengthPower) 和 虚弱(WeakPower)
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class SilenceWrathPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 依然使用你的大图模板，你可以把原版 Anger 的图拷到这个路径下
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(SilenceWrathPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(SilenceWrathPower)}.png";

    // ==========================================
    // 核心触发：当屏障破裂时由屏障自身调用
    // ==========================================
    public void OnBarrierBroken()
    {
        Flash(); // 闪烁图标反馈

        // 1. 给予自己力量
        // 使用 _ = 弃用等待，让指令安全排队，不阻塞屏障破裂的结算
        _ = PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);

        // 2. 给予所有敌人虚弱
        // 获取所有敌对目标
        var enemies = Owner.CombatState.GetOpponentsOf(Owner);
        foreach (var enemy in enemies)
        {
            // 确保敌人还活着且可以被选中
            if (enemy.IsAlive)
            {
                // 同样弃用等待，依次施加虚弱
                _ = PowerCmd.Apply<WeakPower>(enemy, Amount, Owner, null);
            }
        }
    }
}