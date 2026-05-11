using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class CourtMajestyPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    // 设为 None，意味着即便打出多张，减伤比例也固定在 25%，不会叠成 50% 或 100% 的无敌状态
    public override PowerStackType StackType => PowerStackType.None; 
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(CourtMajestyPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(CourtMajestyPower)}.png";

    // 🌟 核心监听：在乘算阶段拦截伤害
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 确保受击者是玩家自己（挂载了这个能力的生物），且攻击者存在
        if (target == Owner && dealer != null)
        {
            // 判定条件 1：攻击者是否有虚弱 (WeakPower)
            bool hasWeak = dealer.GetPower<WeakPower>() != null;
            
            // 判定条件 2：玩家自身是否有屏障 (BarrierPower)
            bool hasBarrier = Owner.GetPower<BarrierPower>() != null;

            if (hasWeak && hasBarrier)
            {
                Flash(); // 触发时闪烁能力图标
                
                // 返回 0.75m，引擎会自动把这个系数乘到当前伤害中，实现 25% 的减伤
                return 0.75m;
            }
        }
        
        // 如果不满足条件，或者不是自己在挨打，默认返回 1m (100% 伤害，不加不减)
        return 1m;
    }
}