using System;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat; 
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers;
using PenanceMod.PenanceModCode.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; // 原版能力的命名空间（如力量）可能在这里

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
        // Owner 是 Creature，不是 Player。
        // 这里为了避免混淆，命名为 ownerCreature。
        var ownerCreature = Owner;

        // 如果裁决是在玩家身上，Owner.Player 就是玩家。
        // 如果以后有宠物/召唤物持有裁决，可以用 PetOwner 兜底。
        var player = ownerCreature.Player ?? ownerCreature.PetOwner;

        // 没有玩家来源就不处理遗物逻辑，直接退出更安全。
        if (player == null)
            return;

        int baseDamage = Amount;

        // 1. 力量加成
        //
        // Power 在 Creature 身上，所以用 ownerCreature.GetPower<T>()。
        // 这里不要用 player.GetPower<T>()，Player 本身不是 Creature。
        if (ownerCreature.GetPower<InTheNameOfTheLawPower>() != null)
        {
            var strength = ownerCreature.GetPower<StrengthPower>();
            if (strength != null)
            {
                baseDamage += strength.Amount;
            }
        }

        // 2. 乘区加成
        float calculatedDamage = baseDamage;

        // 遗物在 Player 身上，所以用 player.GetRelic<T>()。
        if (player.GetRelic<Innocent>() != null)
        {
            calculatedDamage *= 1.2f;
        }

        // 3. 固定增伤
        int finalDamage = (int)Math.Floor(calculatedDamage);

        var shopVoucher = player.GetRelic<ShopVoucher>();
        if (shopVoucher != null)
        {
            finalDamage += 2;
            shopVoucher.Flash();
        }

        if (finalDamage <= 0)
            return;

        Flash();

        // 4. 造成伤害
        //
        // 推荐用命名参数版本，和 AttackCommand 内部调用 CreatureCmd.Damage 的方式一致。
        // AttackCommand 也是传 choiceContext、targets、props、dealer、cardSource 进去结算伤害。
        await CreatureCmd.Damage(
            choiceContext: new BlockingPlayerChoiceContext(),
            targets: new[] { target },
            amount: finalDamage,
            props: ValueProp.Unpowered,
            dealer: ownerCreature,
            cardSource: null
        );

        var revenge = ownerCreature.GetPower<CodeOfRevengePower>();
        revenge?.OnJudgementTriggered();
    }
}