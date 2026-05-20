using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Monsters;

public sealed class FamilyCleaner : CustomMonsterModel
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 34, 30);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 40, 36);

    private int SingleAttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 12);

    private int MultiAttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 8);

    private const int MultiAttackHits = 2;

    public override bool HasDeathSfx => false;

    public override NCreatureVisuals? CreateCustomVisuals() =>
        NodeFactory<NCreatureVisuals>.CreateFromScene("res://PenanceMod/scenes/FamilyCleaner.tscn");

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
        var singleAttack = new MoveState("SINGLE_ATTACK", SingleAttackMove, new SingleAttackIntent(SingleAttackDamage));
        var multiAttack = new MoveState("MULTI_ATTACK", MultiAttackMove, new MultiAttackIntent(MultiAttackDamage, MultiAttackHits));

        singleAttack.FollowUpState = multiAttack;
        multiAttack.FollowUpState = singleAttack;

        return new MonsterMoveStateMachine(new[] { singleAttack, multiAttack }, singleAttack);
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
    }

    private async Task SingleAttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(SingleAttackDamage).FromMonster(this)
            .WithAttackerAnim("Attack", 0.5f)
            .WithHitFx(VfxCmd.slashPath)
            .Execute(null);
    }

    private async Task MultiAttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(MultiAttackDamage).WithHitCount(MultiAttackHits).FromMonster(this)
            .WithAttackerAnim("Attack", 0.5f)
            .WithHitFx(VfxCmd.slashPath)
            .Execute(null);
    }
}
