using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards; // 用于判断 CardType

// 按照你的要求引入
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.ValueProps;

namespace PenanceMod.PenanceModCode.Powers;

public class CumulativePrecedentPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 已经应用最新的 powers/large/ 模板
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(CumulativePrecedentPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(CumulativePrecedentPower)}.png";

    // 完美替代 StS1 里的 onUseCard
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 判断这是否是由该能力的拥有者（通常是玩家）打出的，并且是不是技能牌
        if (Owner != null && cardPlay.Card.Owner?.Creature == Owner && cardPlay.Card.Type == CardType.Skill)
        {
            Flash(); // 闪烁图标

            // 获得与此能力层数相等的屏障
            await PowerCmd.Apply<BarrierPower>(Owner, Amount, Owner, null);
        }
    }
}