using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Players;

namespace PenanceMod.PenanceModCode.Powers;

public class ThornyPathPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 大图/小图路径模板
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(ThornyPathPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(ThornyPathPower)}.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 确保当前回合的主角是这个能力的拥有者（即玩家自己）
        if (Owner != null && player.Creature == Owner && Amount > 0)
        {
            // 1. 获取所有存活的敌人
            var aliveEnemies = Owner.CombatState.GetOpponentsOf(Owner).Where(e => !e.IsDead).ToList();
            
            if (aliveEnemies.Count > 0)
            {
                // 2. 核心逻辑：一键统计所有存活敌人身上的虚弱层数之和
                int totalWeak = aliveEnemies.Sum(enemy => enemy.GetPower<WeakPower>()?.Amount ?? 0);

                // 3. 计算获得量：虚弱总和 * 此能力的层数
                int gainAmount = totalWeak * Amount;

                if (gainAmount > 0)
                {
                    Flash(); // 闪烁图标反馈

                    // 4. 给予荆棘环身 (ThornAuraPower)
                    await PowerCmd.Apply<ThornAuraPower>(choiceContext, Owner, gainAmount, Owner, null);
                }
            }
        }
    }
}