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
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class InTheNameOfTheLaw : PenanceBaseCard
{
    // 耗能 1，类型 Power，稀有度 Uncommon，目标 Self
    public InTheNameOfTheLaw() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.ThornAura),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Law-Weak", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int weakAmount = DynamicVars["Law-Weak"].IntValue;
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", 0.2f);
        await PowerCmd.Apply<InTheNameOfTheLawPower>(choiceContext, Owner.Creature, weakAmount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Law-Weak"].UpgradeValueBy(1);
    }
}