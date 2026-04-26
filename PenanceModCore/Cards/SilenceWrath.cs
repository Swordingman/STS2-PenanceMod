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
    // 耗能 1，类型 Power，稀有度 Uncommon，目标 Self
    public SilenceWrath() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：获得的力量/虚弱层数 (初始 2)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("SilenceWrath-Amt", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner?.Creature;
        if (creature == null) return;

        var vars = DynamicVars.Values.ToList();
        int amount = vars.Count > 0 ? vars[0].IntValue : 2;

        // 挂载核心 Power
        // 传入 this 作为 source，保持良好的数据追溯性
        await PowerCmd.Apply<SilenceWrathPower>(creature, amount, creature, this);
    }

    protected override void OnUpgrade()
    {
        // 层数提升 1 (2 -> 3)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(1);
        }
    }
}