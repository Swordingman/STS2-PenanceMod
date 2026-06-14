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
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Context;

namespace PenanceMod.PenanceModCode.Powers;

public class JudgementPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(JudgementPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(JudgementPower)}.png";

    // ==========================================
    // 异步伤害结算核心（被 BarrierPower 的 AfterDamageReceived 阻塞式调用）
    // ==========================================
    public async Task TriggerJudgementDamageAsync(Creature target, PlayerChoiceContext realContext)
    {
        var ownerCreature = Owner;
        var player = ownerCreature.Player ?? ownerCreature.PetOwner;

        if (player == null)
            return;

        int baseDamage = Amount;
        float calculatedDamage = baseDamage;

        if (player.GetRelic<Innocent>() != null)
        {
            calculatedDamage *= 1.2f;
        }

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

        VfxCmd.PlayOnCreatureCenter(target, VfxCmd.slashPath);

        // 【关键】：使用真实传过来的上下文 realContext，不再伪造 new BlockingPlayerChoiceContext()
        await CreatureCmd.Damage(
            choiceContext: realContext,
            targets: new[] { target },
            amount: finalDamage,
            props: ValueProp.Unpowered,
            dealer: ownerCreature,
            cardSource: null
        );

        var revenge = ownerCreature.GetPower<CodeOfRevengePower>();
        // 同样注意：如果复仇能力会造成伤害，也需要重构为 await 异步调用
        revenge?.OnJudgementTriggered();
    }
}