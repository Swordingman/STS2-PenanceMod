using BaseLib.Abstracts;
using Godot;

namespace PenanceMod.PenanceModCode.Character;

public class PenanceModRelicPool : CustomRelicPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://PenanceMod/images/charui/small_orb.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://PenanceMod/images/charui/energy_orb_p.png";
}