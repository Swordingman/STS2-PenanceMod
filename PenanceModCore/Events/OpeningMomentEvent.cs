using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.CardSelection;
using PenanceMod.PenanceModCode.Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.HoverTips;

namespace PenanceMod.Scripts.Events;

public sealed class OpeningMomentEvent : CustomEventModel
{
    public override string? CustomInitialPortraitPath => "res://PenanceMod/images/events/OpeningMomentEvent.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new GoldVar(150)
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var options = new List<EventOption>();

        bool hasGold = Owner!.Gold >= DynamicVars.Gold.BaseValue;
        bool canUpgrade = Owner!.Deck.Cards.Any(c => c.IsUpgradable);
        bool canRemove = Owner!.Deck.Cards.Any(); 

        if (hasGold) {
            options.Add(new EventOption(this, JoinCarnival, 
                new LocString("events", "PENANCEMOD-OPENING_MOMENT_EVENT.pages.INITIAL.options.JOIN_CARNIVAL.title"),
                new LocString("events", "PENANCEMOD-OPENING_MOMENT_EVENT.pages.INITIAL.options.JOIN_CARNIVAL.description"),
                "JOIN_CARNIVAL", HoverTipFactory.FromRelic<CarnivalMoment>()));
        } else {
            options.Add(new EventOption(this, null, 
                new LocString("events", "PENANCEMOD-OPENING_MOMENT_EVENT.pages.INITIAL.options.LOCKED_GOLD.title"),
                new LocString("events", "PENANCEMOD-OPENING_MOMENT_EVENT.pages.INITIAL.options.LOCKED_GOLD.description"),
                "LOCKED_GOLD", Array.Empty<IHoverTip>())); 
        }

        if (canUpgrade && canRemove) {
            options.Add(new EventOption(this, StayAlert, 
                new LocString("events", "PENANCEMOD-OPENING_MOMENT_EVENT.pages.INITIAL.options.STAY_ALERT.title"),
                new LocString("events", "PENANCEMOD-OPENING_MOMENT_EVENT.pages.INITIAL.options.STAY_ALERT.description"),
                "STAY_ALERT", Array.Empty<IHoverTip>()));
        } else {
            options.Add(new EventOption(this, null, 
                new LocString("events", "PENANCEMOD-OPENING_MOMENT_EVENT.pages.INITIAL.options.LOCKED_CARDS.title"),
                new LocString("events", "PENANCEMOD-OPENING_MOMENT_EVENT.pages.INITIAL.options.LOCKED_CARDS.description"),
                "LOCKED_CARDS", Array.Empty<IHoverTip>()));
        }

        return options;
    }

    private async Task JoinCarnival()
    {
        await PlayerCmd.LoseGold(DynamicVars.Gold.BaseValue, Owner!, GoldLossType.Lost);
        await RelicCmd.Obtain(ModelDb.Relic<CarnivalMoment>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("JOINED_CARNIVAL"));
    }

    // 🌟 选项 2：保持警惕 (使用官方高级 API 升级和删牌)
    private async Task StayAlert()
    {
        // 1. 升级卡牌 (系统会自动过滤可升级的卡牌，并自带“选择要升级的牌”文本)
        CardModel cardToUpgrade = (await CardSelectCmd.FromDeckForUpgrade(Owner!, new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1))).FirstOrDefault();
        if (cardToUpgrade != null)
        {
            CardCmd.Upgrade(cardToUpgrade);
        }

        // 2. 移除卡牌 (自带“选择要移除的牌”文本)
        List<CardModel> cardsToRemove = (await CardSelectCmd.FromDeckForRemoval(Owner!, new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1))).ToList();
        if (cardsToRemove.Count > 0)
        {
            // 使用标准的 PileCmd 移除，兼容各种遗物触发器
            await CardPileCmd.RemoveFromDeck(cardsToRemove);
        }

        SetEventFinished(PageDescription("STAYED_ALERT"));
    }
}