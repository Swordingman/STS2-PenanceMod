using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class WeightOfLaw : PenanceBaseCard
{
    public WeightOfLaw() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("WeightOfLaw-MaxHp", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner?.Creature;
        if (creature == null) return;

        // 1. 失去 6 点生命值
        var selfDamageVar = new DamageVar(6, ValueProp.Unblockable);
        await CreatureCmd.Damage(choiceContext, creature, selfDamageVar, this);

        await Cmd.Wait(0.1f);

        // 2. 获得最大生命值
        int maxHpGain = DynamicVars["WeightOfLaw-MaxHp"].IntValue;
        await CreatureCmd.GainMaxHp(creature, maxHpGain);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WeightOfLaw-MaxHp"].UpgradeValueBy(1);
    }
}