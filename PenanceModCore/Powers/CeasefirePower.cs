using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; // 用于判断 CardType

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
    // 敌人侧效果：跳过攻击回合
    // ==========================================
    // 在敌方回合开始时进行拦截判定
    public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (Owner.IsEnemy && side == Owner.Side)
        {
            // 【提示】由于怪物的“意图(Intent)”数据储存在 MonsterModel 中，
            // 你需要在 IDE 里敲下 Owner.Monster. 看看弹出的意图变量名叫什么。
            
            if (Owner.Monster.BeforeAttack) 
            {
                // 方案 A: 直接给予 StS2 原生的“眩晕(Stun)”能力，最省事且有 UI 提示
                _ = PowerCmd.Apply<StunPower>(Owner, 1, Owner, null);

                // 方案 B: 或者强制清空它的行动队列跳过回合
                // Owner.Monster.ClearActionQueue();
            }
            */
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