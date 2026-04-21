using System;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat; 
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps; // 原版能力的命名空间（如力量）可能在这里

namespace PenanceMod.PenanceModCode.Powers;

public class JudgementPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(JudgementPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(JudgementPower)}.png";

    // ==========================================
    // 同步触发钩子（被 BarrierPower 调用）
    // ==========================================
    public void OnBarrierDamaged(Creature attacker)
    {
        if (attacker != null && attacker != Owner && Amount > 0)
        {
            // 【关键黑科技】：使用弃用等待（_ =）启动一个异步任务。
            // 这样它就会像 StS1 里的 addToTop 一样，安全地排队去造成伤害，
            // 而不会在屏障扣血的瞬间卡死游戏引擎。
            _ = TriggerJudgementDamageAsync(attacker);
        }
    }

    // ==========================================
    // 异步伤害结算核心（彻底取代 TriggerJudgementAction）
    // ==========================================
    private async Task TriggerJudgementDamageAsync(Creature target)
    {
        var player = Owner;
        int baseDamage = Amount;
        int finalDamage = baseDamage;

        // // --- 1. 力量加成 ---
        // // (注：InTheNameOfTheLawPower 记得替换为你实际的类名)
        // if (player.GetPower<InTheNameOfTheLawPower>() != null)
        // {
        //     // 获取原版的力量能力 (注意引用的命名空间要对)
        //     var strength = player.GetPower<StrengthPower>(); 
        //     if (strength != null)
        //     {
        //         baseDamage += strength.Amount;
        //     }
        // }

        // // --- 2. 乘区加成 ---
        // float calculatedDamage = baseDamage;
        // if (player.GetRelic<InnocentRelic>() != null)
        // {
        //     calculatedDamage *= 1.2f;
        // }

        // // --- 3. 固定增伤 ---
        // int finalDamage = (int)Math.Floor(calculatedDamage); // 显式取整更安全
        // var shopVoucher = player.GetRelic<ShopVoucherRelic>();
        // if (shopVoucher != null)
        // {
        //     finalDamage += 2;
        //     shopVoucher.Flash(); // 遗物闪烁
        // }

        // 防跌破底线
        if (finalDamage < 0) finalDamage = 0;

        // --- 4. 造成伤害与后续触发 ---
        if (finalDamage > 0)
        {
            Flash(); // 裁决能力自身闪烁

            // 造成反伤。使用 Thorns 荆棘伤害类型，避免触发易伤/虚弱等标准攻击判定
            await CreatureCmd.Damage(
                null!,                    // PlayerChoiceContext
                target,                   // 目标
                finalDamage,              // 伤害数值
                ValueProp.Unpowered,      // 关键：更接近你想要的“非普通攻击伤害”
                player,                   // dealer / 伤害来源
                null                      // cardSource
            );

            // 触发复仇法典
            // var revenge = player.GetPower<CodeOfRevengePower>();
            // revenge?.OnJudgementTriggered();
        }
    }
}