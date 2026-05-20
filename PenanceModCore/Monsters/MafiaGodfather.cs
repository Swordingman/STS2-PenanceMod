using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Monsters;

public sealed class MafiaGodfather : CustomMonsterModel
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 120, 112);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 128, 120);

    private const int StrengthAmount = 2;
    private const int BlockAmount = 15;

    public override bool HasDeathSfx => false;

    public override NCreatureVisuals? CreateCustomVisuals() =>
        NodeFactory<NCreatureVisuals>.CreateFromScene("res://PenanceMod/scenes/MafiaGodfather.tscn");

    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        var skin = skeleton.GetData().FindSkin("default");
        if (skin != null)
        {
            skeleton.SetSkin(skin);
        }

        skeleton.SetSlotsToSetupPose();
    }

    public override CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller) =>
        SetupAnimationState(controller, idleName: "Idle", deadName: "Die", attackName: "Attack");

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var summon = new MoveState("SUMMON", SummonMove, new SummonIntent());
        var strengthen = new MoveState("STRENGTHEN_FAMILY", StrengthenFamilyMove, new BuffIntent());
        var defend = new MoveState("DEFEND_FAMILY", DefendFamilyMove, new DefendIntent(), new BuffIntent());

        summon.FollowUpState = strengthen;
        strengthen.FollowUpState = defend;
        defend.FollowUpState = strengthen;

        return new MonsterMoveStateMachine(new[] { summon, strengthen, defend }, summon);
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();

        TalkCmd.Play(
            new LocString("monsters", "PENANCEMOD-MAFIA_GODFATHER.dialogs.START"),
            Creature,
            VfxColor.White
        );
    }

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature == Creature)
        {
            foreach (Creature cleaner in LivingCleaners().ToList())
            {
                await CreatureCmd.Damage(
                    new ThrowingPlayerChoiceContext(),
                    cleaner,
                    cleaner.CurrentHp,
                    ValueProp.Unblockable,
                    Creature,
                    null
                );
            }
        }
    }

    private async Task SummonMove(IReadOnlyList<Creature> targets)
    {
        if (LivingCleaners().Any())
        {
            await StrengthenFamilyMove(targets);
            return;
        }

        await SummonCleaner("cleaner1");
        await SummonCleaner("cleaner2");
    }

    private async Task StrengthenFamilyMove(IReadOnlyList<Creature> targets)
    {
        if (!LivingCleaners().Any())
        {
            await SummonMove(targets);
            return;
        }

        foreach (Creature creature in LivingFamilyMembers())
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature, StrengthAmount, Creature, null);
        }
    }

    private async Task DefendFamilyMove(IReadOnlyList<Creature> targets)
    {
        if (!LivingCleaners().Any())
        {
            await SummonMove(targets);
            return;
        }

        foreach (Creature creature in LivingFamilyMembers())
        {
            await CreatureCmd.GainBlock(creature, BlockAmount, ValueProp.Move, null);
        }
    }

    private async Task SummonCleaner(string slot)
    {
        var cleaner = CombatState.CreateCreature(
            ModelDb.Monster<FamilyCleaner>().ToMutable(),
            Creature.Side,
            slot
        );

        await CreatureCmd.Add(cleaner);
    }

    private IEnumerable<Creature> LivingFamilyMembers() =>
        CombatState.Enemies.Where(c => !c.IsDead && (c == Creature || c.Monster is FamilyCleaner));

    private IEnumerable<Creature> LivingCleaners() =>
        CombatState.Enemies.Where(c => !c.IsDead && c.Monster is FamilyCleaner);
}
