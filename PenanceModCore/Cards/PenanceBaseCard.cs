using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using System.Linq;
using PenanceMod.PenanceModCode.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.GameActions;

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
        await PowerCmd.Apply<BarrierPower>(target, amount, Owner.Creature, this);
    }

    /// <summary>
    /// 快捷施加“审判” (Judgement)
    /// </summary>
    protected async Task ApplyJudgement(Creature target, int amount)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<JudgementPower>(target, amount, Owner.Creature, this);
    }

    /// <summary>
    /// 触发狼群自动打出逻辑
    /// </summary>
    protected async Task TriggerWolfAutoplay()
    {
        // 1. 视觉提示：闪烁卡牌
        // 注意：二代的视觉节点通常与 Model 分离。如果需要在 Model 层触发视觉闪烁，
        // 可能会有一个专门的 Cmd（例如 VfxCmd）或者触发一个 Event，这里暂时留空或使用官方 API
        
        // 2. 强制等待 1.0 秒 (无视游戏内的快速模式)
        // C# 原生 Task.Delay 传入的是毫秒 (1000ms = 1s)，它直接按真实时间挂起，不受游戏倍速影响！
        await Task.Delay(1000); 

        // 3. 加入打出队列
        // 在二代中，强制玩家打出一张特定的牌，可以直接向同步队列里压入一个 PlayCardAction
        // (this: 打出的卡, null: 随机/无目标)
        if (CombatManager.Instance.IsInProgress && !Owner.Creature.IsDead)
        {
            RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new PlayCardAction(this, null));
        }
    }
}