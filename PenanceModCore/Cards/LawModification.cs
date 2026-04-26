using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class LawModification : PenanceBaseCard
{
    // 耗能 1，类型 Skill，稀有度 Uncommon，目标 Self
    public LawModification() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册基础关键词：消耗，保留
    public override IEnumerable<CardKeyword> CanonicalKeywords => 
        [CardKeyword.Exhaust, CardKeyword.Retain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        
        // 获取当前格挡值
        int currentBlock = creature.Block;

        if (currentBlock > 0)
        {
            // 1. 失去所有格挡
            await CreatureCmd.LoseBlock(creature, currentBlock);

            // 稍微停顿，让碎甲特效和上屏障特效分离开，手感更好
            await Cmd.Wait(0.15f);

            // 2. 获得等量的屏障
            await PowerCmd.Apply<BarrierPower>(creature, currentBlock, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级减费 (1 -> 0)
        EnergyCost.UpgradeBy(-1);
    }
}