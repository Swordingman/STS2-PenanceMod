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
public class CrownOfThorns : PenanceBaseCard
{
    // 耗能 1，类型 Power，稀有度 Uncommon，目标 Self
    public CrownOfThorns() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：获得荆棘的数量 (3)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Crown-Magic", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int thornsGain = vars.Count > 0 ? vars[0].IntValue : 3;

        // 挂载荆棘冠冕能力
        await PowerCmd.Apply<CrownOfThornsPower>(Owner.Creature, thornsGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级后荆棘获取量增加 (3 -> 5)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(2);
        }
    }
}