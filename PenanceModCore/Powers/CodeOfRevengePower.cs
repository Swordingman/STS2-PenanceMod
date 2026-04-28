using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class CodeOfRevengePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(CodeOfRevengePower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(CodeOfRevengePower)}.png";

    // 这个方法已经被你写在 JudgementPower 里的触发逻辑中调用了
    public void OnJudgementTriggered()
    {
        Flash(); // 遗物/能力闪烁提示玩家触发了
        
        // 核心：给予屏障
        // 弃用等待 (_ =) 让引擎在后台安全地排队执行，不阻塞当前的裁决伤害
        _ = PowerCmd.Apply<BarrierPower>(Owner, Amount, Owner, null);
    }
}