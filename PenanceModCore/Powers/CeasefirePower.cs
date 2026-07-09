using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures; // 用于判断 CardType

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
    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        // 🌟 核心修复：如果这张牌的主人（打牌者）不是当前能力的持有者，直接放行！
        if (card.Owner != Owner.Player)
        {
            return true;
        }

        // 只有当能力持有者自己想打出攻击牌时，才进行拦截
        if (card.Type == CardType.Attack)
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
            command.WithHitCount(0);
        }
        
        return Task.CompletedTask;
    }

    // ==========================================
    // 回合结束：层数自动衰减
    // ==========================================
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 确保只在能力拥有者自己的回合结束时衰减
        if (side == Owner.Side)
        {
            Flash(); // 闪烁特效
            
            if (Amount <= 1)
            {
                // ✅ 调用官方 API 彻底将该能力从实体身上摘除！
                await PowerCmd.Remove(this); 
            }
            else
            {
                // 层数减 1，保留 SetAmount 是可以的，因为它会自动触发数值改变的动画
                SetAmount(Amount - 1); 
            }
        }
    }
}