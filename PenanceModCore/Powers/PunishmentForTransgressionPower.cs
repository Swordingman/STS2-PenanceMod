using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards; // 用于判断 CardType

// 你强调必须引用的命名空间
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;

namespace PenanceMod.PenanceModCode.Powers;

public class PunishmentForTransgressionPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 应用大图路径新模板
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(PunishmentForTransgressionPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(PunishmentForTransgressionPower)}.png";

    // 内部标记：本回合是否打出过攻击牌
    private bool _attackPlayed = false;

    // ==========================================
    // 1. 回合开始时：重置标记
    // ==========================================
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == player.Creature)
        {
            _attackPlayed = false;
        }
        return Task.CompletedTask;
    }

    // ==========================================
    // 2. 每次出牌时：检查攻击
    // ==========================================
    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 如果打出的卡牌类型是攻击，则标记为 true
        if (cardPlay.Card.Type == CardType.Attack)
        {
            _attackPlayed = true;
        }
        return Task.CompletedTask;
    }

    // ==========================================
    // 3. 回合结束时：发放奖励
    // ==========================================
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        // 判定：是玩家回合结束，且本回合从未打出过攻击牌
        if (Owner != null && side == Owner.Side && !_attackPlayed)
        {
            Flash(); // 闪烁图标反馈

            // 给予等同于此能力层数的“正当防卫”
            // 使用异步 Apply
            await PowerCmd.Apply<JustifiedDefensePower>(Owner, Amount, Owner, null);
        }
    }
}