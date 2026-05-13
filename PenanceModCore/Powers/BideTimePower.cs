using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers; // 活力(VigorPower) 所在命名空间
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Combat;

namespace PenanceMod.PenanceModCode.Powers;

public class BideTimePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(BideTimePower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(BideTimePower)}.png";

    // 用于记录本回合是否打出了攻击牌
    private bool _attackPlayedThisTurn = false;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
        {
            _attackPlayedThisTurn = false; // 回合开始重置标记
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack && cardPlay.Card.Owner?.Creature == Owner)
        {
            _attackPlayedThisTurn = true; // 一旦打出攻击牌，修改标记
        }
        return Task.CompletedTask;
    }

    // 在回合结束时结算
    public override async Task BeforeTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side && !_attackPlayedThisTurn && Amount > 0)
        {
            Flash(); // 闪烁能力图标
            // 获得活力 (VigorPower)
            await PowerCmd.Apply<VigorPower>(choiceContext, Owner, Amount, Owner, null);
        }
    }
}