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
public class IroncladDoctrine : PenanceBaseCard
{
    // 耗能 2，类�?Power，稀有度 Uncommon，目�?Self
    public IroncladDoctrine() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：需要消耗的屏障�?(初始 10)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Doctrine-Cost", 8m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int costAmt = vars.Count > 0 ? vars[0].IntValue : 8;

        // 挂载铁律护体状态，层数即为“需要消耗的屏障数量�?
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", 0.2f);
        await PowerCmd.Apply<IroncladDoctrinePower>(choiceContext,Owner.Creature, costAmt, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
