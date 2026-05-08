using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.HoverTips;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class FamilyArbitration : PenanceBaseCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves),
        ..WolfCurseHelper.GetWolfCurseHoverTips(IsUpgraded)
    ];
    
    // 耗能 1，类型 Attack，稀有度 Common，目标 AnyEnemy
    public FamilyArbitration() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册变量：基础伤害 14
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(14, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        var randomCurse = WolfCurseHelper.GetRandomWolfCurse(Owner, CombatState, IsUpgraded);

        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(randomCurse, PileType.Discard, Owner), 2.2f);
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 3 (14 -> 17)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}