using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class NoSentenceNeeded : PenanceBaseCard
{
    public NoSentenceNeeded() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(50, ValueProp.Move)
    ];

    // ==========================================
    // 🛑 核心：通过重写属性来实现自定义打出限制
    // ==========================================
    protected override bool IsPlayable
    {
        get
        {
            // 1. 如果基类（或其他底层状态）已经不允许打出了，直接返回 false
            if (!base.IsPlayable) return false;

            // 确保 Owner 存在 (在牌库查看等非战斗状态下 Owner 可能为空，做个安全保护)
            var creature = Owner?.Creature;
            if (creature == null) return true; 

            // 2. 检查是否有屏障
            bool hasBarrier = (creature.GetPower<BarrierPower>()?.Amount ?? 0) > 0;

            // 3. 检查生命值是否不足一半 (直接调用你在 PenanceBaseCard 里写好的完美辅助方法！)
            bool isHalfHealth = IsHalfHealth(creature);

            // 只有当“没有屏障” 且 “血量不足一半” 时，才允许打出！
            return !hasBarrier && isHalfHealth;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 造成 50 (升级 60) 点的基础伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10);
    }
}