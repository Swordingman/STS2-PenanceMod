using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories; // 🌟 新增：必须引入节点工厂
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat; // 🌟 新增：NCreatureVisuals 需要这个
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Monsters;

public sealed class VolsiniiMobHeavy : CustomMonsterModel
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 70, 60);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 75, 65);

    private int NormalAttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 12);
    private int HeavyAttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 35, 30);
    private const int BlockAmount = 12;
    private const int BlockStrBlock = 8;
    private const int BlockStrStr = 1;
    private const int BuffStrAmount = 3;

    public override bool HasDeathSfx => false;

    public override NCreatureVisuals? CreateCustomVisuals() => 
        NodeFactory<NCreatureVisuals>.CreateFromScene("res://PenanceMod/scenes/VolsiniiMobHeavy.tscn");

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
        var normalAttack = new MoveState("NORMAL_ATTACK", NormalAttackMove, new SingleAttackIntent(NormalAttackDamage));
        var heavyAttack = new MoveState("HEAVY_ATTACK", HeavyAttackMove, new SingleAttackIntent(HeavyAttackDamage));
        var blockMove = new MoveState("BLOCK_MOVE", BlockMoveAction, new DefendIntent());
        var blockStrMove = new MoveState("BLOCK_STR_MOVE", BlockStrMoveAction, new DefendIntent(), new BuffIntent());
        var buffMove = new MoveState("BUFF_MOVE", BuffMoveAction, new BuffIntent());

        blockMove.FollowUpState = normalAttack;
        normalAttack.FollowUpState = blockStrMove;
        blockStrMove.FollowUpState = heavyAttack;
        heavyAttack.FollowUpState = buffMove;
        buffMove.FollowUpState = blockMove;

        return new MonsterMoveStateMachine(new[] { blockMove, normalAttack, blockStrMove, heavyAttack, buffMove }, blockMove);
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        
        await PowerCmd.Apply<BackAttackRightPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);

        var player = CombatState.GetOpponentsOf(Creature)[0];
        await PowerCmd.Apply<SurroundedPower>(new ThrowingPlayerChoiceContext(), player, 1m, Creature, null);

        _ = SummonCivilianWhenCombatStarts();
    }

    private async Task SummonCivilianWhenCombatStarts()
    {
        while (!CombatManager.Instance.IsInProgress)
        {
            await Task.Yield();
        }

        var player = CombatState.Players[0];

        if (player != null)
        {
            var civilian = CombatState.CreateCreature(
                ModelDb.Monster<VolsiniiCivilian>().ToMutable(),
                player.Creature.Side,
                "civilian"
            );

            await CreatureCmd.Add(civilian);
        }
    }

    private async Task NormalAttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(NormalAttackDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f).WithHitFx(VfxCmd.bluntPath).Execute(null);
    }

    private async Task HeavyAttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(HeavyAttackDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f).WithHitFx(VfxCmd.heavyBluntPath, null, "heavy_attack.mp3").Execute(null);
    }

    private async Task BlockMoveAction(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, BlockAmount, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move, null);
    }

    private async Task BlockStrMoveAction(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, BlockStrBlock, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, BlockStrStr, Creature, null);
    }

    private async Task BuffMoveAction(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, BuffStrAmount, Creature, null);
    }
}