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
public class InescapableNet : PenanceBaseCard
{
    // 耗能 1，类型 Skill，稀有度 Uncommon，目标 Self
    public InescapableNet() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：下回合获得的屏障数 (初始 12)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Net-Barrier", 12m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int barrierGain = vars.Count > 0 ? vars[0].IntValue : 12;

        // 给自己挂上“法网恢恢”状态，层数等于屏障数值
        // (抽 1 张牌的逻辑是固定的，所以不需要通过变量传给 Power，直接在 Power 内部写死抽 1 张即可)
        await PowerCmd.Apply<InescapableNetPower>(Owner.Creature, barrierGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级后获得的屏障提升 3 (12 -> 15)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(3);
        }
    }
}