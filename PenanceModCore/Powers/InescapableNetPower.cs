using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class InescapableNetPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => false; // 隐藏图标，当作后台触发器

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != player.Creature || Amount <= 0) return;

        var combatState = player.Creature.CombatState;
        if (combatState != null)
        {
            // 统计身上有虚弱的敌人
            int weakCount = combatState.HittableEnemies.Count(e => e.GetPower<WeakPower>() != null);
            
            if (weakCount > 0)
            {
                // 因为允许叠层（如果上回合打了两张），奖励会翻倍
                int totalRewards = weakCount * Amount;
                
                await CardPileCmd.Draw(choiceContext, totalRewards, player);
                await PlayerCmd.GainEnergy(totalRewards, player);
            }
        }

        // 触发完毕后自毁
        await PowerCmd.Remove(this);
    }
}