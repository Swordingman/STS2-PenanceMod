using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using BaseLib.Extensions;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class ASip : PenanceBaseCard
{
    private const string PenaltyKey = "ASip-Penalty";

    public int CopyCount { get; set; } = 0;

    public ASip() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(PenaltyKey, 3m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner?.Creature;
        if (creature == null)
        {
            return;
        }

        var penaltyVar = DynamicVars[PenaltyKey];
        int penalty = penaltyVar.IntValue;

        var barrierPower = creature.GetPower<BarrierPower>();
        int currentBarrier = barrierPower != null ? barrierPower.Amount : 0;

        int barrierToLose = Math.Min(currentBarrier, penalty);
        int hpToLose = penalty - barrierToLose;

        if (barrierToLose > 0)
        {
            await PowerCmd.Apply<BarrierPower>(
                choiceContext,
                creature,
                -barrierToLose,
                creature,
                this
            );
        }

        if (hpToLose > 0)
        {
            await CreatureCmd.Damage(
                choiceContext,
                creature,
                hpToLose,
                ValueProp.Unblockable,
                this
            );
        }

        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            creature,
            2,
            creature,
            this
        );

        var copy = (ASip)CreateClone();

        copy.CopyCount = CopyCount + 1;

        // 关键：复制品在“当前数值”的基础上 +2
        copy.DynamicVars[PenaltyKey].UpgradeValueBy(2);

        copy.AddKeyword(CardKeyword.Ethereal);

        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[PenaltyKey].UpgradeValueBy(-1);
    }

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        var clone = CloneOf as ASip;
        if (clone != null)
        {
            this.CopyCount = clone.CopyCount;
        }
    }
}
