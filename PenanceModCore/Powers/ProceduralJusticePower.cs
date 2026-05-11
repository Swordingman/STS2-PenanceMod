using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers; // 必须引入以调用 BarrierPower
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace PenanceMod.PenanceModCode.Powers;

public class ProceduralJusticePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter; 
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(ProceduralJusticePower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(ProceduralJusticePower)}.png";

    // 🌟 核心监听：每当有卡牌被消耗时触发
    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        // 只要这个能力还在场上且层数大于 0
        if (Owner != null && Amount > 0)
        {
            Flash(); // 闪烁一下能力图标
            
            // 获得等同于当前层数(Amount)的屏障
            await PowerCmd.Apply<BarrierPower>(choiceContext, Owner, Amount, Owner, null);
        }
    }
}