using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using System.Collections.Generic;
using System.Threading.Tasks;
using PenanceMod.PenanceModCode.Encounters;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Animation;
using BaseLib.Utils.NodeFactories;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace PenanceMod.PenanceModCode.Monsters;

public sealed class VolsiniiCivilian : CustomPetModel
{
    public VolsiniiCivilian() : base(true)
    {
    }

    public override int MinInitialHp => 30;
    public override int MaxInitialHp => 30;

    public override bool HasDeathSfx => false;

    public override NCreatureVisuals? CreateCustomVisuals() =>
        NodeFactory<NCreatureVisuals>.CreateFromScene("res://PenanceMod/scenes/VolsiniiCivilian.tscn");

    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        var skin = skeleton.GetData().FindSkin("default");
        if (skin != null)
        {
            skeleton.SetSkin(skin);
        }

        skeleton.SetSlotsToSetupPose();
    }

    public override CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller)
    {
        return SetupAnimationState(
            controller,
            idleName: "Idle",
            attackName: "Attack"
        );
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();

        await Cmd.Wait(0.2f);

        TalkCmd.Play(
            new LocString(
                "monsters",
                "PENANCEMOD-VOLSINII_CIVILIAN.dialogs.START"
            ),
            Creature,
            VfxColor.White
        );
    }

    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature == Creature && delta < 0m)
        {
            TalkCmd.Play(new LocString("monsters", "PENANCEMOD-VOLSINII_CIVILIAN.dialogs.HURT"), Creature, VfxColor.White);
        }

        return Task.CompletedTask;
    }

    public override Task BeforeDeath(Creature creature)
    {
        if (creature == Creature)
        {
            try
            {
                TalkCmd.Play(new LocString("monsters", "PENANCEMOD-VOLSINII_CIVILIAN.dialogs.DIE"), Creature, VfxColor.White);
            }
            catch
            {
            }

            if (Creature.CombatState?.Encounter is VolsiniiCourtEncounter encounter)
            {
                encounter.CivilianDied = true;
            }
        }

        return Task.CompletedTask;
    }
}