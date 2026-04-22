using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class LawIsCanonPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 采用最新的大图路径模板
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(LawIsCanonPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(LawIsCanonPower)}.png";

    // 对应 StS1 的 atEndOfTurn(boolean isPlayer)
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        // 1. 判定是否为拥有者（通常是玩家）的回合结束
        if (Owner != null && side == Owner.Side)
        {
            // 2. 检查是否有屏障 (BarrierPower)
            var barrier = Owner.GetPower<BarrierPower>();
            if (barrier != null && barrier.Amount > 0)
            {
                // 3. 检查是否有力量 (StrengthPower)
                var strength = Owner.GetPower<StrengthPower>();
                if (strength != null && strength.Amount > 0)
                {
                    Flash(); // 闪烁图标反馈

                    // 4. 计算获得量： 力量值 * 此能力层数 (Amount)
                    int gainAmount = (int)strength.Amount * (int)Amount;

                    if (gainAmount > 0)
                    {
                        // 5. 给予裁决 (使用异步 Apply)
                        await PowerCmd.Apply<JudgementPower>(Owner, gainAmount, Owner, null);
                    }
                }
            }
        }
    }
}