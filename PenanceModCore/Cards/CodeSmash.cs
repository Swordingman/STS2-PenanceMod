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
public class CodeSmash : PenanceBaseCard
{
    private const int Threshold = 7;

    // 耗能 0，类型 Attack，稀有度 Common，目标 AnyEnemy
    public CodeSmash() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => 
        [CardKeyword.Exhaust];

    // 基础伤害 6
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var attackCmd = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        int unblockedDamage = attackCmd.Results.Sum(r => r.UnblockedDamage);

        int energyGain = unblockedDamage / Threshold;

        if (energyGain > 0)
        {
            await PlayerCmd.GainEnergy(energyGain, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 3 (6 -> 9)
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}