using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class GuiltBoundAura : PenanceBaseCard
{
    public GuiltBoundAura() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Aura-Barrier", 6m),
        new DynamicVar("Aura-Thorns", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;

        // 1. 获得屏障
        await ApplyBarrier(creature, DynamicVars["Aura-Barrier"].IntValue);
        
        // 2. 获得荆棘环身
        await PowerCmd.Apply<ThornAuraPower>(choiceContext, creature, DynamicVars["Aura-Thorns"].IntValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Aura-Barrier"].UpgradeValueBy(2);
        DynamicVars["Aura-Thorns"].UpgradeValueBy(1);
    }
}