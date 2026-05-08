using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class TangledThreads : PenanceBaseCard
{
    public TangledThreads() : base(1, CardType.Curse, CardRarity.Curse, TargetType.AllEnemies, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves)
    ];
    
    private bool _autoPlaying;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this)
            return;

        if (_autoPlaying)
            return;

        _autoPlaying = true;

        try
        {
            await TriggerWolfAutoplay(choiceContext, card);
        }
        finally
        {
            _autoPlaying = false;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;
        var combatState = creature.CombatState;

        if (combatState == null)
            return;

        var hand = PileType.Hand.GetPile(player);

        var candidates = hand.Cards
            .Where(card => !ReferenceEquals(card, this))
            .ToList();

        int targetExhaustCount = 3;
        int actualExhaustCount = System.Math.Min(targetExhaustCount, candidates.Count);
        var cardsToExhaust = PickRandomCards(candidates, actualExhaustCount);
        int totalCost = 0;

        foreach (CardModel card in cardsToExhaust)
        {
            totalCost += GetPositiveCurrentEnergyCost(card);
            await CardCmd.Exhaust(choiceContext, card, causedByEthereal: false);
        }

        int hitCount = IsUpgraded ? targetExhaustCount * 2 : targetExhaustCount;

        if (totalCost > 0 && hitCount > 0)
        {
            await Cmd.Wait(0.1f);

            await DamageCmd.Attack(totalCost)
                .FromCard(this)
                .TargetingRandomOpponents(combatState, allowDuplicates: true)
                .WithHitCount(hitCount)
                .Unpowered()
                .WithNoAttackerAnim()
                .WithHitFx(VfxCmd.giantHorizontalSlashPath) 
                .Execute(choiceContext);
        }

        await CardPileCmd.Draw(choiceContext, 5, player);
    }

    private List<CardModel> PickRandomCards(List<CardModel> source, int count)
    {
        var pool = source.ToList();
        var result = new List<CardModel>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Owner.RunState.Rng.Shuffle.NextInt(pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    private static int GetPositiveCurrentEnergyCost(CardModel card)
    {
        if (card.EnergyCost.CostsX)
            return 0;

        int cost = card.EnergyCost.GetWithModifiers(CostModifiers.All);
        return System.Math.Max(0, cost);
    }

    protected override void OnUpgrade()
    {
    }
}