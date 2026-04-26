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
public class GlowOfSuffering : PenanceBaseCard
{
    // 耗能 1，类型 Power，稀有度 Uncommon，目标 Self
    public GlowOfSuffering() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：倍率数值 (初始 1)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Glow-Mult", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int multiplier = vars.Count > 0 ? vars[0].IntValue : 1;

        // 挂载苦难辉光能力
        // 这里的层数直接代表倍率。
        // 如果你的 Power 类内部逻辑是：获得层数 * 失去生命值的效果，那么传 multiplier 即可。
        await PowerCmd.Apply<GlowOfSufferingPower>(Owner.Creature, multiplier, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级逻辑：标准难度下，倍率从 1 倍提升到 2 倍
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(1);
        }
    }
}