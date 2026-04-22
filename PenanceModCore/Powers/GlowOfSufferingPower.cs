using System;
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

public class GlowOfSufferingPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(GlowOfSufferingPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(GlowOfSufferingPower)}.png";

    // ==========================================
    // 核心一：完美解决“取最大值”的堆叠逻辑
    // ==========================================
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        // 如果系统正试图向自己(Owner)施加同名能力(GlowOfSufferingPower)
        if (target == Owner && canonicalPower.Id == this.Id)
        {
            // 如果新来的倍率比当前低或相等，直接把增加量(amount)砍成 0
            if (amount <= this.Amount)
            {
                modifiedAmount = 0m;
                return true; // 返回 true 表示我们修改了这笔交易
            }
            else
            {
                // 如果比当前高（当前 100，新来 150），我们只让它增加差值（50）
                // 这样加起来总数就完美停在 150 了
                modifiedAmount = amount - this.Amount;
                return true;
            }
        }
        
        modifiedAmount = amount;
        return false;
    }

    // ==========================================
    // 核心二：监听“由于卡牌效果导致的生命流失”
    // ==========================================
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 精准判定条件：
        // 1. 受伤的是自己 (target == Owner)
        // 2. 确实扣除了生命值 (result.UnblockedDamage > 0)
        // 3. 有卡牌作为来源溯源 (cardSource != null)
        // 4. 带有“无视格挡”标签，以此严格对应 StS1 中的 LoseHp 行为 (props.HasFlag(ValueProp.Unblockable))
        if (target == Owner && 
            result.UnblockedDamage > 0 && 
            cardSource != null && 
            props.HasFlag(ValueProp.Unblockable))
        {
            Flash(); // 闪烁受苦之光图标

            // 纯数学计算：失去的血量 * (倍率 / 100.0)
            int bonus = (int)(result.UnblockedDamage * (Amount / 100.0f));

            if (bonus > 0)
            {
                // 注意：因为我们已经在一个 async Task 里面了，所以可以直接优雅地 await，
                // 引擎会乖乖把你的给盾和给荆棘的动画排队播完，不需要加 _ = 弃用等待。
                await PowerCmd.Apply<BarrierPower>(Owner, bonus, Owner, null);
                await PowerCmd.Apply<ThornAuraPower>(Owner, bonus, Owner, null);
            }
        }
    }
}