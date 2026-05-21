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
public class PunishmentForTransgression : PenanceBaseCard
{
    // 耗能 1，类�?Power，稀有度 Uncommon，目�?Self
    public PunishmentForTransgression() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：获得的“正当防卫”层�?(初始 1)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Transgression-Amt", 1m).WithTooltip("PENANCEMOD-JUSTIFIED_DEFENSE")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 提取变量数�?
        var vars = DynamicVars.Values.ToList();
        int amount = vars.Count > 0 ? vars[0].IntValue : 1;

        // 挂载核心 Power
        await PowerCmd.Apply<PunishmentForTransgressionPower>(choiceContext,Owner.Creature, amount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级提升层数 (1 -> 2)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(1);
        }
    }
}
