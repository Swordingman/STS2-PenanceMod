using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using PenanceMod.PenanceModCode.Monsters;
using System.Collections.Generic;

namespace PenanceMod.PenanceModCode.Encounters;

public sealed class CaseFile1184Encounter : CustomEncounterModel
{
    public CaseFile1184Encounter() : base(RoomType.Monster)
    {
    }

    public override string? CustomScenePath =>
        "res://PenanceMod/scenes/CaseFile1184Encounter.tscn";

    public override bool IsValidForAct(ActModel act) => false;

    public override IReadOnlyList<string> Slots => [
        "cleaner1", "cleaner2", "godfather"
    ];

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<MafiaGodfather>(),
        ModelDb.Monster<FamilyCleaner>()
    ];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
    [
        (ModelDb.Monster<MafiaGodfather>().ToMutable(), "godfather")
    ];
}
