using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class BadOpera : PenanceBaseCard
{
    private const string MagicKey = "BadOpera-Magic";

    public BadOpera() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(MagicKey, 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var magicVar = DynamicVars.Values.First();
        int generateAmount = magicVar.IntValue;

        var generatedCards = CardFactory.GetDistinctForCombat(
            Owner,
            from c in Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            where c.Type == CardType.Attack && c.CanBeGeneratedInCombat
            select c,
            generateAmount,
            Owner.RunState.Rng.CombatCardGeneration
        ).ToList();

        foreach (var randomCard in generatedCards)
        {
            if (this.IsUpgraded && randomCard.IsUpgradable)
            {
                randomCard.UpgradeInternal();
                randomCard.FinalizeUpgradeInternal();
            }

            await CardPileCmd.AddGeneratedCardToCombat(randomCard, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }
}