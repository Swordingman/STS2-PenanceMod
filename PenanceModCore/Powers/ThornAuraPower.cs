using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; // 引入 ValueProp 命名空间

namespace PenanceMod.PenanceModCode.Powers;

public class ThornAuraPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(ThornAuraPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(ThornAuraPower)}.png";

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        // 确保只在拥有者（通常是玩家）的回合结束时触发
        if (Owner != null && side == Owner.Side && Amount > 0)
        {
            Flash(); // 闪烁特效

            var enemies = Owner.CombatState.GetOpponentsOf(Owner);
            
            // 建立一个任务列表，用来收集所有即将造成的伤害任务
            List<Task> damageTasks = new List<Task>();

            foreach (var enemy in enemies)
            {
                if (enemy.IsHittable)
                {
                    // 不直接 await，而是把任务装进列表里
                    damageTasks.Add(CreatureCmd.Damage(
                        choiceContext,        // 直接传入钩子自带的上下文，比 null! 更安全
                        enemy,                // 目标
                        Amount,               // 伤害数值
                        ValueProp.Unpowered,  // 伤害属性：非攻击牌造成的伤害
                        Owner,                // 伤害来源
                        null                  // 来源卡牌为空
                    ));
                }
            }

            // 万箭齐发：如果列表里有任务，让它们在同一帧瞬间全部结算！
            if (damageTasks.Count > 0)
            {
                await Task.WhenAll(damageTasks);
            }
        }
    }
}