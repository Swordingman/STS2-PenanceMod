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

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class FamilyArbitration : PenanceBaseCard
{
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
        if (target == null) return;

        // 1. 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 2. 调用你写好的 Helper 拿到一张随机狼群诅咒！
        // ⚠️ 提示：之前你写的 Helper 签名是 GetRandomWolfCurse(Player player)
        var randomCurse = WolfCurseHelper.GetRandomWolfCurse(Owner);

        if (randomCurse != null)
        {
            // 3. 将其置入弃牌堆 (PileType.Discard)
            await CardPileCmd.AddGeneratedCardToCombat(randomCurse, PileType.Discard, addedByPlayer: false);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 3 (14 -> 17)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}