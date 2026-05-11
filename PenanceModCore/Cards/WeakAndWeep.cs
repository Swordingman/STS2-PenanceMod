using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class WeakAndWeep : PenanceBaseCard
{
    // 耗能 2，技能牌，稀有度 Rare，目标是单体敌人
    public WeakAndWeep() : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    // 默认附带消耗词条
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 1. 获取目标身上的虚弱层数（作为攻击段数）
        // 如果目标没虚弱，弱点层数就是 0
        int weakStacks = target.GetPower<WeakPower>()?.Amount ?? 0;
        
        // 2. 获取玩家身上的荆棘环身层数（作为单段伤害的基数）
        int thornAuraAmt = Owner.Creature.GetPower<ThornAuraPower>()?.Amount ?? 0;

        // 3. 只有当两个条件都满足（有段数 且 有伤害），才执行攻击
        if (weakStacks > 0 && thornAuraAmt > 0)
        {
            await DamageCmd.Attack(thornAuraAmt)
                .FromCard(this)
                .Targeting(target)
                .WithHitCount(weakStacks) // 👈 直接把虚弱层数塞给 HitCount
                .Unpowered()              // 技能牌造成的荆棘伤害通常不吃力量加成
                .WithHitFx(VfxCmd.slashPath) 
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}