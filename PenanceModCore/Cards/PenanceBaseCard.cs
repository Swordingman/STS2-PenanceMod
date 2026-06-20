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
using PenanceMod.PenanceModCode.Extensions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Localization;

namespace PenanceMod.Scripts.Cards;

public abstract class PenanceBaseCard : CustomCardModel
{
    // 自动获取卡图路径
    public override string PortraitPath => $"res://PenanceMod/images/cards/{GetType().Name}.png";

    public string WolfCurseSfx => "res://PenanceMod/scenes/audio/trigger_wolfcurse.wav";

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
                entry.HappenedThisTurn(CombatState) &&
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

    // ==========================================
    // 狼群诅咒相关逻辑 (Wolf Curse Logic)
    // ==========================================

    /// <summary>
    /// 定义每回合狼群诅咒的自动触发上限
    /// 可以将其设为 virtual，以便部分特殊卡牌或遗物在未来可以修改这个上限
    /// </summary>
    protected virtual int MaxWolfCursesPerTurn => 20;

    /// <summary>
    /// 获取本回合已经打出的狼群诅咒数量
    /// </summary>
    protected int GetWolfCursesTriggeredThisTurn()
    {
        if (Owner == null || CombatState == null) return 0;

        return CombatManager.Instance.History.CardPlaysStarted
            .Count(entry => 
                entry.HappenedThisTurn(CombatState) &&
                entry.CardPlay.Card.Owner == Owner &&
                entry.CardPlay.Card.Tags.Contains(PenanceCardTags.CurseOfWolves));
    }

    /// <summary>
    /// 触发狼群自动打出逻辑 (附带上限检测)
    /// </summary>
    protected async Task TriggerWolfAutoplay(PlayerChoiceContext choiceContext, CardModel card, Creature? target = null)
    {
        if (card == null || card.Owner == null || card.Owner.Creature == null) return;
        if (CombatManager.Instance.IsOverOrEnding || card.Owner.Creature.IsDead) return;

        // --- 核心修改：上限拦截 ---
        int triggeredCount = GetWolfCursesTriggeredThisTurn();
        if (triggeredCount >= MaxWolfCursesPerTurn)
        {
            TalkCmd.Play(new LocString("characters", "PENANCEMOD-WOLF_CURSE_LIMITER"), card.Owner.Creature, VfxColor.White);
            return; 
        }

        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", 0.2f);

        // 获取角色节点
        var creatureNode = card.Owner.Creature.GetCreatureNode();
        
        if (creatureNode != null)
        {
            await AudioManager.PlayCustomSfx(WolfCurseSfx, creatureNode);
        }

        await CardCmd.AutoPlay(
            choiceContext,
            card,
            target
        );
    }
}