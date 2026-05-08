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
public class SceneInvestigation : PenanceBaseCard
{
    public SceneInvestigation() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Scene-Energy", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SceneInvestigationPower>(choiceContext,Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}