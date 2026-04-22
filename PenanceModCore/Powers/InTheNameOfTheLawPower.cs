using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.PenanceModCode.Powers;

public class InTheNameOfTheLawPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    // 如果这个能力获得多次没有任何额外效果，你可以把它设为 None。
    // 如果你想让它显示层数（虽然不参与计算），可以保留 Counter。
    public override PowerStackType StackType => PowerStackType.None; 
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(InTheNameOfTheLawPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(InTheNameOfTheLawPower)}.png";
}