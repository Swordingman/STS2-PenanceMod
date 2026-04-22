using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; // 抽牌需要用到 Player

namespace PenanceMod.PenanceModCode.Powers;

public class InescapableNetPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 已经应用最新的大图路径模板
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(InescapableNetPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(InescapableNetPower)}.png";

    // 完美对应 StS1 里的 atStartOfTurnPostDraw
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 确保只有在自己（通常是玩家）的回合开始时才触发
        if (Owner == player.Creature)
        {
            Flash(); // 闪烁图标，给玩家视觉反馈

            // 1. 获得对应层数的屏障
            await PowerCmd.Apply<BarrierPower>(Owner, Amount, Owner, null);

            // 2. 抽 1 张牌 (使用官方底层的卡牌堆叠指令)
            await CardPileCmd.Draw(choiceContext, 1, player);

            // 3. 移除自身 (因为这是个一次性的延迟生效 Buff)
            // 在异步方法中，直接 await PowerCmd.Remove 是绝对安全的！
            await PowerCmd.Remove(this);
        }
    }
}