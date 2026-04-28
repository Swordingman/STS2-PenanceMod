using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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
    public TangledThreads() : base(-1, CardType.Curse, CardRarity.Curse, TargetType.AllEnemies, true)
    {
    }

    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
        PenanceKeywords.CurseOfWolves
    ];

    protected override HashSet<CardTag> CanonicalTags => [
        PenanceCardTags.CurseOfWolves
    ];

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this)
        {
            await Cmd.Wait(0.25f);
            await TriggerWolfAutoplay();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;
        var combatState = creature.CombatState;

        if (combatState == null)
            return;

        int x = ResolveEnergyXValue();
        if (x <= 0)
            return;

        var hand = PileType.Hand.GetPile(player);

        var candidates = hand.Cards
            .Where(card => !ReferenceEquals(card, this))
            .ToList();

        int exhaustCount = System.Math.Min(x, candidates.Count);
        var cardsToExhaust = PickRandomCards(candidates, exhaustCount);

        int totalCost = 0;

        foreach (CardModel card in cardsToExhaust)
        {
            totalCost += GetPositiveCurrentEnergyCost(card);
            await CardCmd.Exhaust(choiceContext, card, causedByEthereal: false);
        }

        int hitCount = IsUpgraded ? x + x : x;

        if (totalCost <= 0 || hitCount <= 0)
            return;

        await Cmd.Wait(0.1f);

        await DamageCmd.Attack(totalCost)
            .FromCard(this)
            .TargetingRandomOpponents(combatState, allowDuplicates: true)
            .WithHitCount(hitCount)
            .Unpowered()
            .WithNoAttackerAnim()
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
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
        // 升级效果写在 OnPlay：
        // 未升级：hitCount = X
        // 已升级：hitCount = X + X
    }
}