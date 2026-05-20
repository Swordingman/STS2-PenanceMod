using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Powers;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Powers;

public class InescapableNetPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    // Amount = 剩余触发次数
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath =>
        $"res://PenanceMod/images/powers/{nameof(InescapableNetPower)}.png";

    public override string? CustomBigIconPath =>
        $"res://PenanceMod/images/powers/large/{nameof(InescapableNetPower)}.png";

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || Amount <= 0)
        {
            return;
        }

        int hpLost = result.UnblockedDamage;
        if (hpLost <= 0)
        {
            return;
        }

        var thornAura = Owner.GetPower<ThornAuraPower>();
        var judgement = Owner.GetPower<JudgementPower>();

        int thornAmount = thornAura?.Amount ?? 0;
        int judgementAmount = judgement?.Amount ?? 0;
        int totalAvailable = thornAmount + judgementAmount;

        if (totalAvailable >= hpLost)
        {
            Flash();

            int remainingCost = hpLost;

            // 优先消耗荆棘环身
            int thornToConsume = System.Math.Min(thornAmount, remainingCost);
            if (thornToConsume > 0)
            {
                await PowerCmd.Apply<ThornAuraPower>(
                    choiceContext,
                    Owner,
                    -thornToConsume,
                    Owner,
                    null
                );

                remainingCost -= thornToConsume;
            }

            // 不足部分消耗裁决
            int judgementToConsume = System.Math.Min(judgementAmount, remainingCost);
            if (judgementToConsume > 0)
            {
                await PowerCmd.Apply<JudgementPower>(
                    choiceContext,
                    Owner,
                    -judgementToConsume,
                    Owner,
                    null
                );

                remainingCost -= judgementToConsume;
            }

            await CreatureCmd.Heal(Owner, hpLost);
        }

        // 不管是否成功抵消，这次“失去生命值”都消耗一次触发次数
        if (Amount <= 1)
        {
            await PowerCmd.Remove(this);
        }
        else
        {
            await PowerCmd.Apply<InescapableNetPower>(
                choiceContext,
                Owner,
                -1,
                Owner,
                null
            );
        }
    }
}