using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace PenanceMod.PenanceModCode.Powers;

public class PreemptiveStrikePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    // Counter 类型允许打出多张时持续回合数自动叠加 (比如 3+3=6回合)
    public override PowerStackType StackType => PowerStackType.Counter; 
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(PreemptiveStrikePower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(PreemptiveStrikePower)}.png";

    // 🌟 在玩家回合开始时触发
    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner && Amount > 0)
        {
            Flash(); // 闪烁图标给玩家提示
            
            // 1. 获得 3 层正当防卫 (请确保你的项目中已存在 JustifiedDefensePower 类)
            await PowerCmd.Apply<JustifiedDefensePower>(choiceContext, Owner, 3, Owner, null);

            // 2. 持续回合倒计时减 1 (层数归零时，引擎底层通常会自动移除该能力)
            await PowerCmd.Apply<PreemptiveStrikePower>(choiceContext, Owner, -1, Owner, null);
        }
    }
}