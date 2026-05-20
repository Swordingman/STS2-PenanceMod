using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rewards;
using PenanceMod.PenanceModCode.Encounters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Events;

public sealed class CaseFile1184Event : CustomEventModel
{
    private const int GoldReward = 99;
    private const int MaxHpReward = 5;
    private const int AttackUpgradeCount = 2;

    public override string? CustomInitialPortraitPath => "res://PenanceMod/images/events/VolsiniiCourtEvent.png";

    public override bool IsShared => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new GoldVar(GoldReward),
        new DynamicVar("CaseFile-MaxHp", MaxHpReward),
        new DynamicVar("CaseFile-Upgrades", AttackUpgradeCount)
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(
            this,
            SubmitFalseEvidence,
            new LocString("events", "PENANCEMOD-CASE_FILE1184_EVENT.pages.INITIAL.options.SUBMIT_FALSE_EVIDENCE.title"),
            new LocString("events", "PENANCEMOD-CASE_FILE1184_EVENT.pages.INITIAL.options.SUBMIT_FALSE_EVIDENCE.description"),
            "SUBMIT_FALSE_EVIDENCE",
            HoverTipFactory.FromCardWithCardHoverTips<Doubt>()
        ),
        new EventOption(
            this,
            ReleaseInCourt,
            new LocString("events", "PENANCEMOD-CASE_FILE1184_EVENT.pages.INITIAL.options.RELEASE_IN_COURT.title"),
            new LocString("events", "PENANCEMOD-CASE_FILE1184_EVENT.pages.INITIAL.options.RELEASE_IN_COURT.description"),
            "RELEASE_IN_COURT",
            HoverTipFactory.FromCardWithCardHoverTips<Regret>()
        ),
        new EventOption(
            this,
            PrivateVerdict,
            new LocString("events", "PENANCEMOD-CASE_FILE1184_EVENT.pages.INITIAL.options.PRIVATE_VERDICT.title"),
            new LocString("events", "PENANCEMOD-CASE_FILE1184_EVENT.pages.INITIAL.options.PRIVATE_VERDICT.description"),
            "PRIVATE_VERDICT",
            Array.Empty<IHoverTip>()
        )
    ];

    private async Task SubmitFalseEvidence()
    {
        await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner!);
        UpgradeRandomAttacks();
        await CardPileCmd.AddCurseToDeck<Doubt>(Owner!);
        SetEventFinished(PageDescription("SUBMITTED_FALSE_EVIDENCE"));
    }

    private async Task ReleaseInCourt()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, DynamicVars["CaseFile-MaxHp"].IntValue);
        await CreatureCmd.Heal(Owner.Creature, Owner.Creature.MaxHp);
        await CardPileCmd.AddCurseToDeck<Regret>(Owner);
        SetEventFinished(PageDescription("RELEASED_IN_COURT"));
    }

    private Task PrivateVerdict()
    {
        EnterCombatWithoutExitingEvent<CaseFile1184Encounter>(
            [new RelicReward(RelicRarity.Rare, Owner!)],
            shouldResumeAfterCombat: false
        );

        return Task.CompletedTask;
    }

    private void UpgradeRandomAttacks()
    {
        List<CardModel> candidates = Owner!.Deck.Cards
            .Where(c => c.Type == CardType.Attack && c.IsUpgradable)
            .ToList();

        for (int i = 0; i < AttackUpgradeCount && candidates.Count > 0; i++)
        {
            int index = Owner.RunState.Rng.Shuffle.NextInt(candidates.Count);
            CardModel card = candidates[index];
            candidates.RemoveAt(index);
            CardCmd.Upgrade(card);
        }
    }
}
