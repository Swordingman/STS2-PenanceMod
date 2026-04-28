using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands.Builders; // 用于判断 CardType

namespace PenanceMod.PenanceModCode.Powers;

public class CeasefirePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff; 
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(CeasefirePower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(CeasefirePower)}.png";

    // ==========================================
    // 玩家侧效果：拦截攻击牌打出
    // ==========================================
    // 完美替代 StS1 的 canPlayCard
    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        // 如果这能力的拥有者是玩家，并且你想打出的牌是“攻击牌”，直接拦截！
        if (Owner.IsPlayer && card.Type == CardType.Attack)
        {
            return false;
        }
        return true; 
    }

// ==========================================
    // 敌人侧效果：拦截真正的攻击动作
    // ==========================================
    public override Task BeforeAttack(AttackCommand command)
    {
        // 如果拥有者是敌人，且这次攻击的【发起者】是这个敌人
        if (Owner.IsEnemy && command.Attacker == Owner)
        {
            Flash(); // 闪烁停火图标
            
            // 没收攻击！
            // 由于具体的 AttackCommand API 需要你看一下智能提示，通常有以下几种暴力破解法：
            
            // 方案 A：直接清空它的攻击次数（最推荐，引擎连动画都不会播了）
            command.WithHitCount(0);
            
            // 方案 B：如果它没有 HitCount 属性，可以清空它的目标列表
            // command.Targets.Clear();
            
            // 方案 C：如果这是一个只读 Command，你还可以配合另一个钩子：
            // public override int ModifyAttackHitCount(AttackCommand attack, int hitCount) { return 0; }
        }
        
        return Task.CompletedTask;
    }

    // ==========================================
    // 回合结束：层数自动衰减
    // ==========================================
    public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        // 确保只在能力拥有者自己的回合结束时衰减
        if (side == Owner.Side)
        {
            Flash(); // 闪烁特效
            
            // SetAmount 会自动触发飘字的动画，且归零时引擎会自动移除该能力
            if (Amount <= 1)
            {
                SetAmount(0); // 直接移除
            }
            else
            {
                SetAmount(Amount - 1); // 层数减 1
            }
        }
        return Task.CompletedTask;
    }
}