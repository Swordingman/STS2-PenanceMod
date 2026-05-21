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
public class JustifiedRetaliation : PenanceBaseCard
{
    // 耗能 1，类�?Skill，稀有度 Uncommon，目�?Self
    public JustifiedRetaliation() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：获得的屏障�?(初始 7)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Justified-Barrier", 7m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int barrierAmount = vars.Count > 0 ? vars[0].IntValue : 7;

        var creature = Owner.Creature;

        // 1. 获得指定层数的屏�?
        await PowerCmd.Apply<BarrierPower>(choiceContext,creature, barrierAmount, creature, this);

        // 稍微停顿一下特效，让连招看起来更舒服（可选）
        await Cmd.Wait(0.1f);

        // 2. 获得 1 层“正当防卫�?
        await PowerCmd.Apply<JustifiedDefensePower>(choiceContext,creature, 1, creature, this);
    }

    protected override void OnUpgrade()
    {
        // 屏障数�?+3 (7 -> 10)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(3);
        }
    }
}
