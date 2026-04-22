using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class UnwaveringPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(UnwaveringPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(UnwaveringPower)}.png";

    // StS2 推荐使用 decimal (m) 来处理所有的战斗数值运算
    private const decimal DamageReductionRate = 0.30m;

    // ==========================================
    // 效果 1：让所有攻击牌消耗
    // ==========================================
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (Owner != null &&
            cardPlay.Card.Owner?.Creature == Owner &&
            cardPlay.Card.Type == CardType.Attack)
        {
            Flash();
            cardPlay.Card.ExhaustOnNextPlay = true;
        }

        return Task.CompletedTask;
    }

    // ==========================================
    // 效果 2：受到的伤害减少 30%
    // ==========================================
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (Owner != null &&
            target == Owner &&
            !props.HasFlag(ValueProp.Unblockable))
        {
            return amount * 0.70m;
        }

        return amount;
    }
    // ==========================================
    // 效果 3：回合结束获得荆棘
    // ==========================================
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Owner != null && side == Owner.Side)
        {
            Flash(); // 闪烁反馈
            await PowerCmd.Apply<ThornAuraPower>(Owner, Amount, Owner, null);
        }
    }
}