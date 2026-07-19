using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class BideTime : PenanceBaseCard
{
    public BideTime() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Bide-Vigor", 14m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 挂载厚积薄发能力，层数即为给的活力数值
        int vigorAmount = DynamicVars["Bide-Vigor"].IntValue;
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", 0.2f);
        await PowerCmd.Apply<BideTimePower>(choiceContext, Owner.Creature, vigorAmount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bide-Vigor"].UpgradeValueBy(6);
    }
}