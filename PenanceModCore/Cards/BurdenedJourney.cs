using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class BurdenedJourney : PenanceBaseCard
{
    // 耗能 1，类�?Skill，稀有度 Common，目�?Self
    public BurdenedJourney() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    // 基础数值为 3
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Burden-Magic", 3m)
            .WithTooltip("PENANCEMOD-JUDGEMENT")
            .WithTooltip("PENANCEMOD-THORN_AURA")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        
        // 安全获取第一个动态变�?
        var magicVar = DynamicVars.Values.First();
        int amount = magicVar.IntValue;

        // 依次施加两层能力
        await PowerCmd.Apply<JudgementPower>(choiceContext,creature, amount, creature, this);
        await PowerCmd.Apply<ThornAuraPower>(choiceContext,creature, amount, creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级提升 2 �?(3 -> 5)
        var magicVar = DynamicVars.Values.First();
        magicVar.UpgradeValueBy(1);
    }
}
