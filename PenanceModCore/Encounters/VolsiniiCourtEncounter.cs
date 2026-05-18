using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using PenanceMod.PenanceModCode.Monsters;
using System.Collections.Generic;

namespace PenanceMod.PenanceModCode.Encounters;

public sealed class VolsiniiCourtEncounter : CustomEncounterModel
{
    private bool _civilianDied;

    public VolsiniiCourtEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsValidForAct(ActModel act) => false;

    public override bool FullyCenterPlayers => true;

    public override string? CustomScenePath =>
        "res://PenanceMod/scenes/VolsiniiCourtEncounter.tscn";

    public override IReadOnlyList<string> Slots => [
        "first", "second", "civilian"
    ];

    public bool CivilianDied
    {
        get => _civilianDied;
        set { AssertMutable(); _civilianDied = value; }
    }

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<VolsiniiCivilian>(),
        ModelDb.Monster<VolsiniiMobAgile>(),
        ModelDb.Monster<VolsiniiMobHeavy>()
    ];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        CivilianDied = false;

        return [
            (ModelDb.Monster<VolsiniiMobAgile>().ToMutable(), "first"),
            (ModelDb.Monster<VolsiniiMobHeavy>().ToMutable(), "second")
        ];
    }

    public override Dictionary<string, string> SaveCustomState() => new()
    {
        ["CivilianDied"] = CivilianDied.ToString()
    };

    public override void LoadCustomState(Dictionary<string, string> state)
    {
        if (state.TryGetValue("CivilianDied", out string val))
        {
            CivilianDied = bool.Parse(val);
        }
    }
}