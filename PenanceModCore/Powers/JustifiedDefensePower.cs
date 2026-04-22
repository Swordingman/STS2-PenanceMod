using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace PenanceMod.PenanceModCode.Powers;

public class JustifiedDefensePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(JustifiedDefensePower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(JustifiedDefensePower)}.png";

    // ==========================================
    // 核心一：回合开始时移除自身
    // ==========================================
    // 完美对应 StS1 里的 atStartOfTurn 
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        // 确保只有在能力拥有者自己的回合开始时才移除
        if (Owner != null && side == Owner.Side)
        {
            // 使用之前刚学到的泛型移除指令，安全高效
            await PowerCmd.Remove<JustifiedDefensePower>(Owner);
        }
    }

    // ==========================================
    // 核心二：受到攻击时触发
    // ==========================================
    // 完美替代 StS1 里的 onAttacked
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 多段攻击保险：如果同一帧内被打多次且层数已空，直接略过
        if (Amount <= 0) return;

        // 判定条件严格对齐原版：
        // 1. 被打的是自己 (target == Owner)
        // 2. 有攻击者且不是自己打自己 (dealer != null && dealer != Owner)
        // 3. 不是因为中毒等造成的纯生命流失
        if (target == Owner && dealer != null && dealer != Owner && !props.HasFlag(ValueProp.Unblockable))
        {
            Flash(); // 闪烁图标

            // 1. 扣除层数 (如果减到了0，StS2 底层会自动移除该能力，免去了你的后顾之忧)
            SetAmount(Amount - 1);

            // 2. 给予触发后的奖励能力 (JustifiedDefenseTriggeredPower)
            // 因为在 async 内部，直接 await 排队即可
            await PowerCmd.Apply<JustifiedDefenseTriggeredPower>(Owner, 1, Owner, null);
        }
    }
}