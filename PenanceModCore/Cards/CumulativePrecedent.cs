using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Extensions;
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
public class CumulativePrecedent : PenanceBaseCard
{
    // 耗能 1，类�?Power，稀有度 Uncommon，目�?Self
    public CumulativePrecedent() : base(1, CardType.Power, CardRarity.Ancient, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Cumulative-Magic", 4m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int barrierGain = vars.Count > 0 ? vars[0].IntValue : 3;

        // 挂载判例累积能力
        await PowerCmd.Apply<CumulativePrecedentPower>(choiceContext,Owner.Creature, barrierGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级逻辑：标准难度下，屏障获取量 +1 (3 -> 4)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(1);
        }
    }
}
