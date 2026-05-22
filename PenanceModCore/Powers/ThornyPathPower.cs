using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace PenanceMod.PenanceModCode.Powers;

public class ThornyPathPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 大图/小图路径模板
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(ThornyPathPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(ThornyPathPower)}.png";

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 确保当前回合的主角是这个能力的拥有者，并且层数大于0
        if (Owner != null && side == Owner.Side && Amount > 0)
        {
            bool isAnyEnemyWeak = CombatState.Enemies
                .Any(enemy => enemy.IsAlive && enemy.Powers.OfType<WeakPower>().Any());

            if (isAnyEnemyWeak)
            {
                Flash();
                await PowerCmd.Apply<ThornAuraPower>(choiceContext, Owner, Amount, Owner, null);
            }
        }
    }
}