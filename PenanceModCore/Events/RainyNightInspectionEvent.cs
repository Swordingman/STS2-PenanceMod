using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards; 
using MegaCrit.Sts2.Core.CardSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace PenanceMod.Scripts.Events;

public sealed class RainyNightInspectionEvent : CustomEventModel
{
    public override string? CustomInitialPortraitPath => "res://PenanceMod/images/events/RainyNightInspectionEvent.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new GoldVar(50)
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var options = new List<EventOption>();

        options.Add(new EventOption(this, Confiscate,
            new LocString("events", "PENANCEMOD-RAINY_NIGHT_INSPECTION_EVENT.pages.INITIAL.options.CONFISCATE.title"),
            new LocString("events", "PENANCEMOD-RAINY_NIGHT_INSPECTION_EVENT.pages.INITIAL.options.CONFISCATE.description"),
            "CONFISCATE", Array.Empty<IHoverTip>()));

        bool canRemove = Owner!.Deck.Cards.Any();
        if (canRemove) {
            options.Add(new EventOption(this, TurnBlindEye,
                new LocString("events", "PENANCEMOD-RAINY_NIGHT_INSPECTION_EVENT.pages.INITIAL.options.TURN_BLIND_EYE.title"),
                new LocString("events", "PENANCEMOD-RAINY_NIGHT_INSPECTION_EVENT.pages.INITIAL.options.TURN_BLIND_EYE.description"),
                "TURN_BLIND_EYE", Array.Empty<IHoverTip>()));
        } else {
            options.Add(new EventOption(this, null, 
                new LocString("events", "PENANCEMOD-RAINY_NIGHT_INSPECTION_EVENT.pages.INITIAL.options.LOCKED_CARDS.title"),
                new LocString("events", "PENANCEMOD-RAINY_NIGHT_INSPECTION_EVENT.pages.INITIAL.options.LOCKED_CARDS.description"),
                "LOCKED_CARDS", Array.Empty<IHoverTip>()));
        }

        return options;
    }

    private async Task Confiscate()
    {
        await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner!);
        await RewardsCmd.OfferCustom(Owner!, [new PotionReward(Owner!)]);
        SetEventFinished(PageDescription("CONFISCATED"));
    }

    // 🌟 选项 2：视而不见 (使用官方高级 API 删牌)
    private async Task TurnBlindEye()
    {
        List<CardModel> cardsToRemove = (await CardSelectCmd.FromDeckForRemoval(Owner!, new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1))).ToList();
        
        if (cardsToRemove.Count > 0)
        {
            await CardPileCmd.RemoveFromDeck(cardsToRemove);
        }

        SetEventFinished(PageDescription("TURNED_BLIND_EYE"));
    }
}