using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class GuardianOfTheLawPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(GuardianOfTheLawPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(GuardianOfTheLawPower)}.png";

    // 这个方法会被你的 BarrierPower 自动拦截并调用
    public void OnBarrierDamaged()
    {
        Flash(); // 闪烁图标，给玩家视觉反馈
        
        // 核心：给予裁决
        // 使用 _ = 进行弃用等待，让赋予动作在底层安全排队，不阻塞屏障的扣血流水线
        _ = PowerCmd.Apply<JudgementPower>(Owner, Amount, Owner, null);
    }
}