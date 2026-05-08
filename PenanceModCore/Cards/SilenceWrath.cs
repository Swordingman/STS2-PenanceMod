using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class SilenceWrath : PenanceBaseCard
{
    public SilenceWrath() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 只有一个通用变量
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("SilenceWrath-Amt", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner?.Creature;
        if (creature == null) return;

        int amount = DynamicVars["SilenceWrath-Amt"].IntValue;
        await PowerCmd.Apply<SilenceWrathPower>(choiceContext, creature, amount, creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级层数 +1
        DynamicVars["SilenceWrath-Amt"].UpgradeValueBy(1);
    }
}