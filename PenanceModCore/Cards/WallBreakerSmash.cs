using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
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
        new DamageVar(24, ValueProp.Move),
        new DynamicVar("WallBreaker-Cost", 10m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    // ==========================================
    // �?官方标准拦截：使�?ShouldPlay 钩子
    // ==========================================
    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        // 既然这是个全局广播的钩子，我们只需要管“这张牌自己”要被打出时的情�?
        if (card == this)
        {
            int barrierCost = DynamicVars["WallBreaker-Cost"].IntValue;
            var barrierPower = Owner?.Creature.GetPower<BarrierPower>();

            // 屏障不足，直接无情拒绝！
            // 引擎会自动把 this (本卡) 登记�?preventer�?
            if (barrierPower == null || barrierPower.Amount < barrierCost)
            {
                return false; 
            }
        }

        return base.ShouldPlay(card, autoPlayType);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner?.Creature;
        if (creature == null || cardPlay.Target == null) return;

        // 既然能走�?OnPlay，说�?ShouldPlay 已经通过，屏障绝对够扣，闭眼直扣�?
        int barrierCost = DynamicVars["WallBreaker-Cost"].IntValue;
        var barrierPower = creature.GetPower<BarrierPower>();

        if (barrierPower != null)
        {
            if (barrierPower.Amount == barrierCost)
            {
                await PowerCmd.Remove(barrierPower);
            }
            else
            {
                await PowerCmd.Apply<BarrierPower>(choiceContext, creature, -barrierCost, creature, this);
            }
        }

        // 大锤落下
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx(VfxCmd.heavyBluntPath)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8);
    }
}
