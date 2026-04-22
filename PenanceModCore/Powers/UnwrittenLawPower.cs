using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace PenanceMod.PenanceModCode.Powers;

public class UnwrittenLawPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None; // 一次性生效，不需要显示层数
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(UnwrittenLawPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(UnwrittenLawPower)}.png";

    // ==========================================
    // 替代 UUID：直接保存目标卡牌的内存引用
    // ==========================================
    public CardModel TargetCard { get; set; } = null!;

    // 施加此能力时调用该方法传入目标卡
    public void Initialize(CardModel targetCard)
    {
        TargetCard = targetCard;
    }

    // ==========================================
    // 核心一：原生双发神技 (修改打出次数)
    // ==========================================
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (TargetCard != null && card == TargetCard)
        {
            Flash(); // 闪烁图标
            return playCount + 1; // 直接让它多打一次！引擎全自动搞定剩下的事。
        }
        return playCount;
    }

    // ==========================================
    // 核心二：打出后移除自身
    // ==========================================
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 卡牌打完之后，把这个一次性 Buff 删掉
        if (TargetCard != null && cardPlay.Card == TargetCard)
        {
            await PowerCmd.Remove<UnwrittenLawPower>(Owner);
        }
    }

    // ==========================================
    // 核心三：回合结束移除
    // ==========================================
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Owner != null && side == Owner.Side)
        {
            await PowerCmd.Remove<UnwrittenLawPower>(Owner);
        }
    }
}