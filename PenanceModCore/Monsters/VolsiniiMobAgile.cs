using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Monsters;

public sealed class VolsiniiMobAgile : CustomMonsterModel
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 45, 42);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 50, 47);

    public int NormalAttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 10);

    public int MultiAttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

    public override bool HasDeathSfx => false;

    public override NCreatureVisuals? CreateCustomVisuals() => 
        NodeFactory<NCreatureVisuals>.CreateFromScene("res://PenanceMod/scenes/VolsiniiMobAgile.tscn");

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
            deadName: "Die", 
            attackName: "Attack"
        );
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var multiAttack = new MoveState("MULTI_ATTACK", MultiAttackMove, new MultiAttackIntent(MultiAttackDamage, 3));
        var blockMove = new MoveState("BLOCK_MOVE", BlockMoveAction, new DefendIntent());
        var normalAttack = new MoveState("NORMAL_ATTACK", NormalAttackMove, new SingleAttackIntent(NormalAttackDamage));
        var buffMove = new MoveState("BUFF_MOVE", BuffMoveAction, new BuffIntent());

        multiAttack.FollowUpState = blockMove;
        blockMove.FollowUpState = normalAttack;
        normalAttack.FollowUpState = buffMove;
        buffMove.FollowUpState = multiAttack;

        return new MonsterMoveStateMachine(new[] { multiAttack, blockMove, normalAttack, buffMove }, multiAttack);
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<BackAttackLeftPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
    }

    private async Task MultiAttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(MultiAttackDamage).WithHitCount(3).FromMonster(this)
        .WithAttackerAnim("Attack", 0.5f)
        .WithHitFx(VfxCmd.slashPath)
        .Execute(null);
    }

    private async Task BlockMoveAction(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, 8m, ValueProp.Move, null);
    }

    private async Task NormalAttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(NormalAttackDamage).FromMonster(this)
            .WithAttackerAnim("Attack", 0.5f)
            .WithHitFx(VfxCmd.bluntPath)
            .Execute(null);
    }

    private async Task BuffMoveAction(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
    }
}