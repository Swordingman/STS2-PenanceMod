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
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player.Creature != Owner || Amount <= 0)
            return;

        Flash();

        await PowerCmd.Apply<JustifiedDefensePower>(choiceContext, Owner, 3, Owner, null);
        await PowerCmd.Decrement(this);
    }
}