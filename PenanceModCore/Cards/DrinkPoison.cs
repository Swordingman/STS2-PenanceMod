using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class DrinkPoison : PenanceBaseCard
{
    private const string PenaltyKey = "Drink-Loss";
    public int CopyCount { get; set; } = 0;

    public DrinkPoison() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(PenaltyKey, 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;
        
        int hpToLose = DynamicVars[PenaltyKey].IntValue;

        // 1. 失去生命值 (采用 Unblockable 直接扣血)
        if (hpToLose > 0)
        {
            await CreatureCmd.Damage(choiceContext, creature, hpToLose, ValueProp.Unblockable, this);
        }

        // 2. 获得 1 能量，抽 1 张牌
        await PlayerCmd.GainEnergy(1, player);
        await CardPileCmd.Draw(choiceContext, 1, player);

        // 3. 生成复制品
        var copy = (DrinkPoison)this.CreateClone();
        copy.CopyCount = this.CopyCount + 1;

        if (this.IsUpgraded)
        {
            copy.UpgradeInternal();
            copy.FinalizeUpgradeInternal();
        }
        
        // 修改复制品的扣血量：直接增加基础值
        var copyPenaltyVar = copy.DynamicVars.Values.First();
        copyPenaltyVar.BaseValue += 2m; 

        // 赋予虚无
        copy.AddKeyword(CardKeyword.Ethereal);

        // 洗入手牌
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, player);
    }

    protected override void OnUpgrade()
    {
        // 升级扣血量降低 (3 -> 2)
        DynamicVars[PenaltyKey].UpgradeValueBy(-1m); 
    }

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        if (CloneOf is DrinkPoison clone)
        {
            this.CopyCount = clone.CopyCount;
        }
    }
}