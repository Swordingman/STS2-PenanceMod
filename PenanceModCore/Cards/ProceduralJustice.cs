using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class ProceduralJustice : PenanceBaseCard
{
    // 耗能 1，类�?Power，稀有度 Uncommon，目�?Self
    public ProceduralJustice() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册动态变量：基础�?3 屏障
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Justice-Barrier", 3m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取当前应该给的屏障数量 (3 �?4)
        int barrierAmount = DynamicVars["Justice-Barrier"].IntValue;

        // 挂载能力：把屏障量作�?Amount 传给 Power�?
        // 如果打了多张，这�?Amount 会自动叠�?(比如 3+3=6)�?
        await PowerCmd.Apply<ProceduralJusticePower>(choiceContext, Owner.Creature, barrierAmount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级后不再减费，而是屏障量提�?1 (3 -> 4)
        DynamicVars["Justice-Barrier"].UpgradeValueBy(1);
    }
}
