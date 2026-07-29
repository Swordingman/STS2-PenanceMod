using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.HoverTips;
using PenanceMod.PenanceModCode.Powers;
using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class StandAsEquals : PenanceBaseCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public StandAsEquals() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies, true)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.Barrier)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("StandAsEquals-Barrier", 30m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner?.Creature;
        if (creature == null || CombatState == null) return;

        var vars = DynamicVars.Values.ToList();
        int barrierAmount = vars.Count > 0 ? vars[0].IntValue : 30;

        // 获取所有友方单位，包括自己
        var allies = (from c in CombatState.GetTeammatesOf(creature)
                    where c != null && c.IsAlive && c.IsPlayer && c != creature
                    select c).ToList();

        allies.Insert(0, creature);

        // 获取自己当前已有的屏障，并加上本卡提供的屏障
        var currentBarrier = creature.GetPower<BarrierPower>();
        int previousBarrier = currentBarrier?.Amount ?? 0;
        int totalBarrierToSplit = previousBarrier + barrierAmount;

        // 先移除自己原有的屏障，再重新平均分配
        if (currentBarrier != null)
        {
            await PowerCmd.Remove(currentBarrier);
        }

        int splitAmount = totalBarrierToSplit / allies.Count;
        int remainder = totalBarrierToSplit % allies.Count;

        foreach (var ally in allies)
        {
            int amountToGive = splitAmount;

            // 无法整除的余数优先留给自己，然后依次分配给其他友方
            if (remainder > 0)
            {
                amountToGive++;
                remainder--;
            }

            if (amountToGive > 0)
            {
                await PowerCmd.Apply<BarrierPower>(choiceContext, ally, amountToGive, creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(5);
        }
    }
}