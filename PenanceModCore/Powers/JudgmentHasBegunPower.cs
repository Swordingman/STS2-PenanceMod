using System;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards; 

// 你强调必须要引用的命名空间（用于原版力量等能力）
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class JudgmentHasBegunPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    // 对应原版 amount = -1 的情况，不在图标右下角显示层数
    public override PowerStackType StackType => PowerStackType.None;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(JudgmentHasBegunPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(JudgmentHasBegunPower)}.png";

    // 内部常数与状态
    private int _attacksPlayed = 0;
    private const int Threshold = 3;
    private const int BarrierGain = 5;
    private const int StrJudgeGain = 1;
    private const int EnergyGain = 2;

    // 暴露给动态文本读取的属性：动态计算还剩几次触发
    public int AttacksRemaining => Math.Max(0, Threshold - _attacksPlayed);

    // ==========================================
    // 核心一：打出攻击牌触发
    // ==========================================
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 判定：打出者是自己，且卡牌类型是攻击牌
        if (Owner != null && cardPlay.Card.Owner?.Creature == Owner && cardPlay.Card.Type == CardType.Attack)
        {
            Flash(); // 闪烁反馈
            _attacksPlayed++;

            // 1. 获得 5屏障, 1力量, 1裁决 (直接异步依次施加，安全顺滑)
            await PowerCmd.Apply<BarrierPower>(Owner, BarrierGain, Owner, null);
            await PowerCmd.Apply<StrengthPower>(Owner, StrJudgeGain, Owner, null);
            await PowerCmd.Apply<JudgementPower>(Owner, StrJudgeGain, Owner, null);

            // 2. 达到阈值时，获得能量
            if (_attacksPlayed == Threshold)
            {
                if (Owner.IsPlayer)
                {
                    _ = PlayerCmd.GainEnergy(EnergyGain, Owner.Player);
                }
            }
        }
    }

    // ==========================================
    // 核心二：回合结束时自动移除（使用你提供的指令）
    // ==========================================
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        // 确保是自己的回合结束
        if (side == Owner.Side)
        {
            // 使用泛型指令精准移除自身！
            await PowerCmd.Remove<JudgmentHasBegunPower>(Owner);
        }
    }
}