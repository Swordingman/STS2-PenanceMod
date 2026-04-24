using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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
public class FlailSmash : PenanceBaseCard
{
    // 耗能 2，类型 Attack，稀有度 Uncommon，目标 AnyEnemy
    public FlailSmash() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    // 绑定消耗词条 (写全路径防报错)
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => 
        [MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Exhaust];

    // 🌟 注册变量：基础伤害 7
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 1. 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 2. 给予 1 层止戈
        await PowerCmd.Apply<CeasefirePower>(target, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 3 (7 -> 10)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}