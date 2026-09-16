using System;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

namespace PenanceMod.PenanceModCode.Powers;

/// <summary>
/// 屏障伤害的纯计算器。
/// 这里只回答“这次伤害应该被屏障吸收多少、剩余多少、是否具备裁决资格”，
/// 绝不修改 Power 层数、登记状态、播放 VFX 或调用 Command。
///
/// 因此该方法可以安全地被伤害预览 / UI Forecast 重复调用。
/// </summary>
internal static class BarrierDamageResolver
{
    internal readonly record struct Resolution(
        decimal RemainingHpLoss,
        decimal AbsorbedDamage,
        int BarrierConsumed,
        bool BarrierBroken,
        Creature? JudgementTarget)
    {
        public bool Absorbed => AbsorbedDamage > 0;
    }

    public static Resolution Calculate(
        Creature owner,
        int barrierAmount,
        decimal incomingHpLoss,
        ValueProp props,
        Creature? dealer,
        bool isResolvingJudgement)
    {
        // 与原 BarrierPower 保持一致：
        // 只有不可格挡伤害绕过屏障。
        // Unpowered 本身并不绕过屏障。
        if (incomingHpLoss <= 0 ||
            barrierAmount <= 0 ||
            props.HasFlag(ValueProp.Unblockable))
        {
            return new Resolution(
                incomingHpLoss,
                0m,
                0,
                false,
                null);
        }

        decimal absorbedDamage = Math.Min(incomingHpLoss, barrierAmount);

        // 保持原代码对 Power Amount(int) 的处理语义。
        int barrierConsumed = incomingHpLoss >= barrierAmount
            ? barrierAmount
            : (int)incomingHpLoss;

        decimal remainingHpLoss = incomingHpLoss >= barrierAmount
            ? incomingHpLoss - barrierAmount
            : 0m;

        bool barrierBroken = incomingHpLoss >= barrierAmount;

        Creature? judgementTarget = null;

        // 这里只计算“有没有裁决资格”，不登记任何状态。
        // 屏障本身是否吸收伤害与 Move 无关。
        if (!isResolvingJudgement &&
            props.IsCardOrMonsterMove() &&
            dealer != null &&
            dealer != owner &&
            owner.CombatState != null &&
            owner.CombatState.Enemies.Contains(dealer))
        {
            judgementTarget = dealer;
        }

        return new Resolution(
            remainingHpLoss,
            absorbedDamage,
            barrierConsumed,
            barrierBroken,
            judgementTarget);
    }
}
