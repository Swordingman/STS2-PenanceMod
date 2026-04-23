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

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class ASip : PenanceBaseCard
{
    // 仅仅用来初始化占个坑
    private const string PenaltyKey = "ASip-Penalty";

    public int CopyCount { get; set; } = 0;

    public ASip() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(PenaltyKey, 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        
        // ✅ 记吃记打版：直接拿第一个变量，绝不去碰那个不存在的 Key
        var penaltyVar = DynamicVars.Values.First();
        int penalty = penaltyVar.IntValue;

        var barrierPower = creature.GetPower<BarrierPower>();
        int currentBarrier = barrierPower != null ? barrierPower.Amount : 0;

        int barrierToLose = Math.Min(currentBarrier, penalty);
        int hpToLose = penalty - barrierToLose;

        if (barrierToLose > 0)
        {
            await PowerCmd.Apply<BarrierPower>(creature, -barrierToLose, creature, this);
        }

        if (hpToLose > 0)
        {
            await CreatureCmd.Damage(choiceContext, creature, hpToLose, ValueProp.Unblockable, this);
        }

        await PowerCmd.Apply<StrengthPower>(creature, 2, creature, this);

        var copy = (ASip)ModelDb.GetById<CardModel>(this.Id).ToMutable();
        
        copy.CopyCount = this.CopyCount + 1;
        
        // ✅ 记吃记打版：获取衍生卡的第一个变量并累加
        var copyPenaltyVar = copy.DynamicVars.Values.First();
        copyPenaltyVar.UpgradeValueBy(copy.CopyCount * 2);

        if (this.IsUpgraded)
        {
            copy.UpgradeInternal();
            copy.FinalizeUpgradeInternal();
        }

        copy.AddKeyword(CardKeyword.Ethereal);

        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, addedByPlayer: true);
    }

    protected override void OnUpgrade()
    {
        // ✅ 记吃记打版：依然直接拿第一个变量升级
        var penaltyVar = DynamicVars.Values.First();
        penaltyVar.UpgradeValueBy(-1); 
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