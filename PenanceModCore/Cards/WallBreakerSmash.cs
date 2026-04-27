using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class WallBreakerSmash : PenanceBaseCard
{
    public WallBreakerSmash() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(25, ValueProp.Move),
        new DynamicVar("WallBreaker-Cost", 10m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner?.Creature;
        if (creature == null || cardPlay.Target == null) return;

        int barrierCost = DynamicVars["WallBreaker-Cost"].IntValue;
        var barrierPower = creature.GetPower<BarrierPower>();

        if (barrierPower != null && barrierPower.Amount > 0)
        {
            int reduceAmt = System.Math.Min(barrierPower.Amount, barrierCost);
            
            if (reduceAmt == barrierPower.Amount)
            {
                await PowerCmd.Remove(barrierPower);
            }
            else
            {
                // 若底层有专门的 Reduce 接口，可替换此处的负数叠加逻辑
                await PowerCmd.Apply<BarrierPower>(creature, -reduceAmt, creature, this);
            }
            
            await Cmd.Wait(0.1f);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt_heavy")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(7);
    }
}