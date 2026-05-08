using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using System.Linq;
using PenanceMod.PenanceModCode.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.Scripts.Cards;

public abstract class PenanceBaseCard : CustomCardModel
{
    // 自动获取卡图路径
    public override string PortraitPath => $"res://PenanceMod/images/cards/{GetType().Name}.png";

    public PenanceBaseCard(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) 
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // ==========================================
    // 常用工具方法 (Utility Methods)
    // ==========================================

    /// <summary>
    /// 判断目标生物是否处于半血或以下状态
    /// </summary>
    protected bool IsHalfHealth(Creature creature)
    {
        // 提示：如果在你的 IDE 中报错，请将 CurrentHealth / MaxHealth 替换为代码提示中的正确属性（例如 HpCurrent 等）
        return creature.CurrentHp <= (creature.MaxHp / 2f);
    }

    /// <summary>
    /// 获取当前玩家本回合打出的卡牌数量
    /// </summary>
    protected int GetCardsPlayedThisTurn()
    {
        if (Owner == null || CombatState == null) return 0;

        return CombatManager.Instance.History.CardPlaysStarted
            .Count(entry => 
                entry.RoundNumber == CombatState.RoundNumber && 
                entry.CardPlay.Card.Owner == Owner);
    }

    /// <summary>
    /// 快捷施加“屏障” (Barrier)
    /// </summary>
    protected async Task ApplyBarrier(Creature target, int amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<BarrierPower>(new ThrowingPlayerChoiceContext(), target, amount, Owner.Creature, this);
    }

    /// <summary>
    /// 快捷施加“审判” (Judgement)
    /// </summary>
    protected async Task ApplyJudgement(Creature target, int amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<JudgementPower>(new ThrowingPlayerChoiceContext(), target, amount, Owner.Creature, this);
    }

        /// <summary>
    /// 快捷施加“荆棘环身” (ThornAura)
    /// </summary>
    protected async Task ApplyThornAura(Creature target, int amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<ThornAuraPower>(new ThrowingPlayerChoiceContext(), target, amount, Owner.Creature, this);
    }

    /// <summary>
    /// 触发狼群自动打出逻辑
    /// </summary>
    protected async Task TriggerWolfAutoplay(PlayerChoiceContext choiceContext, CardModel card, Creature? target = null)
    {
        if (card == null)
            return;

        if (card.Owner == null || card.Owner.Creature == null)
            return;

        if (CombatManager.Instance.IsOverOrEnding)
            return;

        if (card.Owner.Creature.IsDead)
            return;

        await CardCmd.AutoPlay(
            choiceContext,
            card,
            target
        );
    }
}