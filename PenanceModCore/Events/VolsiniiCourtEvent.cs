using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PenanceMod.PenanceModCode.Encounters;
using PenanceMod.PenanceModCode.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Localization; // 引入本地化
using MegaCrit.Sts2.Core.HoverTips;    // 引入悬浮提示

namespace PenanceMod.PenanceModCode.Events;

public sealed class VolsiniiCourtEvent : CustomEventModel
{
    public override string? CustomInitialPortraitPath => "res://PenanceMod/images/events/VolsiniiCourtEvent.png";

    public override bool IsShared => true;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        // 选项 1：挺身而出
        new EventOption(
            this,
            Fight,
            new LocString("events", "PENANCEMOD-VOLSINII_COURT_EVENT.pages.INITIAL.options.FIGHT.title"),
            new LocString("events", "PENANCEMOD-VOLSINII_COURT_EVENT.pages.INITIAL.options.FIGHT.description"),
            "FIGHT",
            Array.Empty<IHoverTip>() // 没有特殊悬浮提示
        ),
        
        // 选项 2：转身离去
        new EventOption(
            this,
            Leave,
            new LocString("events", "PENANCEMOD-VOLSINII_COURT_EVENT.pages.INITIAL.options.LEAVE.title"),
            new LocString("events", "PENANCEMOD-VOLSINII_COURT_EVENT.pages.INITIAL.options.LEAVE.description"),
            "LEAVE",
            HoverTipFactory.FromCardWithCardHoverTips<Regret>() // 🌟 悬浮提示：悔恨诅咒
        )
    ];

    private Task Fight()
    {
        // 实例化具体的遭遇战并传入
        EnterCombatWithoutExitingEvent(ModelDb.Encounter<VolsiniiCourtEncounter>().ToMutable(), Array.Empty<Reward>(), shouldResumeAfterCombat: true);
        return Task.CompletedTask;
    }

    private async Task Leave()
    {
        await CardPileCmd.AddCurseToDeck<Regret>(Owner!);
        
        // 流转到“离开”页面
        SetEventState(PageDescription("LEAVE_PAGE"), [
            new EventOption(
                this,
                WalkAway,
                new LocString("events", "PENANCEMOD-VOLSINII_COURT_EVENT.pages.LEAVE_PAGE.options.WALK_AWAY.title"),
                new LocString("events", "PENANCEMOD-VOLSINII_COURT_EVENT.pages.LEAVE_PAGE.options.WALK_AWAY.description"),
                "WALK_AWAY",
                Array.Empty<IHoverTip>()
            )
        ]);
    }

    private Task WalkAway()
    {
        SetEventFinished(PageDescription("WALK_AWAY_END"));
        return Task.CompletedTask;
    }

    // ==========================================
    // 🌟 核心：战斗回调与后续页面
    // ==========================================
    public override Task Resume(AbstractRoom room)
    {
        if (room is not CombatRoom combatRoom ||
            combatRoom.Encounter is not VolsiniiCourtEncounter encounter)
        {
            SetEventFinished(PageDescription("WALK_AWAY_END"));
            return Task.CompletedTask;
        }

        // 分支 A：平民存活
        if (!encounter.CivilianDied)
        {
            SetEventState(PageDescription("CIVILIAN_SURVIVED"), [
                new EventOption(
                    this,
                    ReadLetter,
                    new LocString("events", "PENANCEMOD-VOLSINII_COURT_EVENT.pages.CIVILIAN_SURVIVED.options.READ_LETTER.title"),
                    new LocString("events", "PENANCEMOD-VOLSINII_COURT_EVENT.pages.CIVILIAN_SURVIVED.options.READ_LETTER.description"),
                    "READ_LETTER",
                    HoverTipFactory.FromRelic<LetterOfGratitude>() // 🌟 悬浮提示：感谢信遗物
                )
            ]);
        }
        // 分支 B：平民死亡
        else
        {
            SetEventState(PageDescription("CIVILIAN_DIED"), [
                new EventOption(
                    this,
                    PickUpCloth,
                    new LocString("events", "PENANCEMOD-VOLSINII_COURT_EVENT.pages.CIVILIAN_DIED.options.PICK_UP_CLOTH.title"),
                    new LocString("events", "PENANCEMOD-VOLSINII_COURT_EVENT.pages.CIVILIAN_DIED.options.PICK_UP_CLOTH.description"),
                    "PICK_UP_CLOTH",
                    HoverTipFactory.FromRelic<BloodstainedCloth>() // 🌟 悬浮提示：染血的布料遗物
                )
            ]);
        }

        return Task.CompletedTask;
    }

    private async Task ReadLetter()
    {
        // 🌟 修复模型报错：获取遗物模板后必须转化为 Mutable
        await RelicCmd.Obtain(ModelDb.Relic<LetterOfGratitude>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("LETTER_READ_END"));
    }

    private async Task PickUpCloth()
    {
        // 🌟 修复模型报错：获取遗物模板后必须转化为 Mutable
        await RelicCmd.Obtain(ModelDb.Relic<BloodstainedCloth>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("CLOTH_PICKED_END"));
    }
}