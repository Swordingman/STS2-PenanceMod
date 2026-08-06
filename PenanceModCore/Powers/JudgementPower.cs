using System;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Relics;

namespace PenanceMod.PenanceModCode.Powers;

public class JudgementPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath =>
        $"res://PenanceMod/images/powers/{nameof(JudgementPower)}.png";

    public override string? CustomBigIconPath =>
        $"res://PenanceMod/images/powers/large/{nameof(JudgementPower)}.png";

    public async Task TriggerJudgementDamageAsync(Creature target, PlayerChoiceContext choiceContext)
    {
        var owner = Owner;
        if (owner == null || target == null || !target.IsAlive || Amount <= 0) return;

        var player = owner.Player ?? owner.PetOwner;
        if (player == null) return;

        int finalDamage = Amount;

        if (player.GetRelic<Innocent>() != null)
            finalDamage = (int)Math.Floor(finalDamage * 1.2f);

        var shopVoucher = player.GetRelic<ShopVoucher>();
        if (shopVoucher != null)
        {
            finalDamage += 2;
            shopVoucher.Flash();
        }

        if (finalDamage <= 0) return;

        Flash();
        VfxCmd.PlayOnCreatureCenter(target, VfxCmd.slashPath);

        #if STS2_BETA
        await CreatureCmd.Damage(
            choiceContext,
            targets: new[] { target },
            finalDamage,
            ValueProp.Unpowered,
            owner,
            null,
            null);
        #else
        await CreatureCmd.Damage(
            choiceContext,
            targets: new[] { target },
            finalDamage,
            ValueProp.Unpowered,
            owner,
            null);
        #endif

        var revenge = owner.GetPower<CodeOfRevengePower>();
        revenge?.OnJudgementTriggered();
    }
}