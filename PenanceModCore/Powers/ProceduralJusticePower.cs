using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.PenanceModCode.Powers;

public class ProceduralJusticePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter; 
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(ProceduralJusticePower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(ProceduralJusticePower)}.png";
}