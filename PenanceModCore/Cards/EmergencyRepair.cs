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
public class EmergencyRepair : PenanceBaseCard
{
    // 耗能 1，类�?Skill，稀有度 Common，目�?Self
    public EmergencyRepair() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：屏障获取量 (初始 4)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Repair-Barrier", 6m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int barrierGain = vars.Count > 0 ? vars[0].IntValue : 4;

        // 1. 获得屏障
        await PowerCmd.Apply<BarrierPower>(choiceContext,Owner.Creature, barrierGain, Owner.Creature, this);

        // 2. �?1 张牌 (复用之前在查阅卷宗学到的底层 API)
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }

    protected override void OnUpgrade()
    {
        // 升级屏障获取�?(6 -> 9)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(3);
        }
    }
}
