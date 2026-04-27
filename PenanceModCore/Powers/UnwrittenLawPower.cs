using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class UnwrittenLawPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    // 不显示层数，不需要叠层。
    // 这个 Power 只是一个“本回合标记器”。
    public override PowerStackType StackType => PowerStackType.None;

    public override string? CustomPackedIconPath =>
        $"res://PenanceMod/images/powers/{nameof(UnwrittenLawPower)}.png";

    public override string? CustomBigIconPath =>
        $"res://PenanceMod/images/powers/large/{nameof(UnwrittenLawPower)}.png";

    // 记录被“不成文的法律”标记的卡。
    //
    // 用法说明：
    // 这里保存的是 CardModel 的实例引用。
    // 所以 UnwrittenLaw 那边必须移动抽牌堆中的原卡，
    // 不能 ToMutable() 复制一张新卡。
    private readonly List<CardModel> _targetCards = new();

    public IReadOnlyList<CardModel> TargetCards => _targetCards;

    // 第一次应用 Power 前调用。
    public void Initialize(CardModel targetCard)
    {
        _targetCards.Clear();
        AddTargetInternal(targetCard, flash: false);
    }

    // 已经存在 UnwrittenLawPower 时，用这个方法追加目标。
    public void AddTarget(CardModel targetCard)
    {
        AddTargetInternal(targetCard, flash: true);
    }

    private void AddTargetInternal(CardModel targetCard, bool flash)
    {
        if (!_targetCards.Any(card => ReferenceEquals(card, targetCard)))
        {
            _targetCards.Add(targetCard);
        }

        if (flash)
        {
            Flash();
        }
    }

    private bool IsTargetCard(CardModel card)
    {
        return _targetCards.Any(target => ReferenceEquals(target, card));
    }

    // 核心：如果打出的牌是被标记的那张，就让它额外打出一次。
    //
    // 用法说明：
    // ModifyCardPlayCount 会在卡牌正式执行前修改打出次数。
    // return playCount + 1 就等价于“额外打出一次”。
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (IsTargetCard(card))
        {
            Flash();
            return playCount + 1;
        }

        return playCount;
    }

    // 被标记的卡打出后，移除对应标记。
    //
    // 注意：
    // 如果没有剩余目标牌，就移除整个 Power。
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (!IsTargetCard(cardPlay.Card))
            return;

        _targetCards.RemoveAll(card => ReferenceEquals(card, cardPlay.Card));

        if (_targetCards.Count == 0)
        {
            await PowerCmd.Remove(this);
        }
    }

    // 回合结束时移除。
    //
    // 用法说明：
    // 这个效果只持续到本回合结束。
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Owner != null && side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}