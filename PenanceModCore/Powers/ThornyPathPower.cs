using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace PenanceMod.PenanceModCode.Powers;

public class ThornyPathPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 大图/小图路径模板
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(ThornyPathPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(ThornyPathPower)}.png";

    // 完美对应原版的 atEndOfTurn
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        // 确保只有在自己的回合结束时触发
        if (Owner != null && side == Owner.Side)
        {
            // 1. 获取身上的裁决 (JudgementPower) 层数
            var judgement = Owner.GetPower<JudgementPower>();
            
            if (judgement != null && judgement.Amount > 0)
            {
                // 2. 计算获得量： 裁决 * (此能力层数 / 100)
                // 在 C# 中，100m 代表这是一个 decimal 类型的 100，避免整数相除变 0 的问题
                int gainAmount = (int)(judgement.Amount * (Amount / 100m));

                if (gainAmount > 0)
                {
                    Flash(); // 闪烁图标反馈

                    // 3. 给予荆棘环身 (ThornAuraPower)
                    await PowerCmd.Apply<ThornAuraPower>(Owner, gainAmount, Owner, null);
                }
            }
        }
    }
}