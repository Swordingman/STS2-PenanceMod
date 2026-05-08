using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Overrule : PenanceBaseCard
{
    public Overrule() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    // 只保留伤害变量，移除已经不需要的屏障变量
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;

        // 1. 【核心快照】：在攻击和上新状态前，记录下敌人原本的虚弱层数
        int existingWeak = target.GetPower<WeakPower>()?.Amount ?? 0;

        // 2. 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 3. 给予保底的 1 层虚弱
        // (保证即使敌人没虚弱，打完也会带上 1 层)
        await PowerCmd.Apply<WeakPower>(choiceContext, target, 1, creature, this);

        // 4. 如果原本就有虚弱，执行翻倍
        // 翻倍的本质就是：再给它挂上它原本拥有的层数
        if (existingWeak > 0)
        {
            await Cmd.Wait(0.1f); // 微小延迟，让数字跳动有节奏感
            await PowerCmd.Apply<WeakPower>(choiceContext, target, existingWeak, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 3 (6 -> 9)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}