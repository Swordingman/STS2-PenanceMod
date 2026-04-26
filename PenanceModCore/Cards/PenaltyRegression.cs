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
using MegaCrit.Sts2.Core.Entities.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class PenaltyRegression : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Uncommon，目标 AnyEnemy
    public PenaltyRegression() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册变量：基础伤害 9
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;

        // 1. 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 2. 使自身所有 Debuff 层数减少 1
        var debuffs = creature.Powers
            .Where(p => p.Type == PowerType.Debuff && p.Amount > 0)
            .ToList();

        if (debuffs.Any())
        {
            await Cmd.Wait(0.1f);

            foreach (var debuff in debuffs)
            {
                await PowerCmd.ModifyAmount(debuff, -1, creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害从 9 提升到 12
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}