using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Localization; // 确保引入了 LocString
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Cards; 
using PenanceMod.PenanceModCode.Relics;
using PenanceMod.Scripts.Cards; 
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // 必须引入 Linq，用于 Concat 合并 HoverTips
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace PenanceMod.Scripts.Events;

public sealed class GreyDealEvent : CustomEventModel
{
    public override string? CustomInitialPortraitPath => "res://PenanceMod/images/events/GreyDealEvent.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new GoldVar(200)
    ];

    public override bool IsAllowed(IRunState runState)
    {
        return runState.CurrentActIndex >= 1;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() 
    {
        var justiceHoverTips = HoverTipFactory.FromCardWithCardHoverTips<DeadlyEnemy>()
            .Concat(HoverTipFactory.FromRelic<Innocent>());

        return [
            // 选项 1：受禄
            new EventOption(
                this, 
                AcceptBribe,
                new LocString("events", "PENANCEMOD-GREY_DEAL_EVENT.pages.INITIAL.options.ACCEPT_BRIBE.title"),
                new LocString("events", "PENANCEMOD-GREY_DEAL_EVENT.pages.INITIAL.options.ACCEPT_BRIBE.description"),
                "ACCEPT_BRIBE",
                HoverTipFactory.FromCardWithCardHoverTips<Normality>()
            ),

            // 选项 2：秉公执法
            new EventOption(
                this, 
                EnforceJustice, 
                new LocString("events", "PENANCEMOD-GREY_DEAL_EVENT.pages.INITIAL.options.ENFORCE_JUSTICE.title"), 
                new LocString("events", "PENANCEMOD-GREY_DEAL_EVENT.pages.INITIAL.options.ENFORCE_JUSTICE.description"), 
                "ENFORCE_JUSTICE", 
                justiceHoverTips
            )
        ];
    }

    // 选项 1：受禄
    private async Task AcceptBribe()
    {
        await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner!);
        await CardPileCmd.AddCurseToDeck<Normality>(Owner!);
        SetEventFinished(PageDescription("ACCEPTED"));
    }

    // 选项 2：秉公执法
    private async Task EnforceJustice()
    {
        await RelicCmd.Obtain(ModelDb.Relic<Innocent>().ToMutable(), Owner!);
        await CardPileCmd.AddCurseToDeck<DeadlyEnemy>(Owner!);
        SetEventFinished(PageDescription("ENFORCED"));
    }
}