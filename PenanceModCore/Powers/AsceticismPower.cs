using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class AsceticismPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(AsceticismPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(AsceticismPower)}.png";

    // 这个方法会被你的 BarrierPower 在扣减层数时同步调用
    public void OnBarrierDamaged()
    {
        // 闪烁特效
        Flash();
        
        // 核心：给予荆棘环身 (ThornAuraPower)
        // 使用 _ = 进行“弃用等待”，让 Apply 动作在底层安全排队，不阻塞当前的伤害结算
        _ = PowerCmd.Apply<ThornAuraPower>(Owner, Amount, Owner, null);
    }
}