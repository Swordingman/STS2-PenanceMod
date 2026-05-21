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
public class GuardianOfTheLaw : PenanceBaseCard
{
    // 耗能 2，类�?Power，稀有度 Rare，目�?Self
    public GuardianOfTheLaw() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：获得的裁决点数 (3)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Guardian-Magic", 3m).WithTooltip("PENANCEMOD-JUDGEMENT")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int magicGain = vars.Count > 0 ? vars[0].IntValue : 3;

        // 挂载律法卫士能力
        await PowerCmd.Apply<GuardianOfTheLawPower>(choiceContext,Owner.Creature, magicGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
