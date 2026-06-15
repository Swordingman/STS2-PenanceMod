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
using System.Numerics;
using System.Drawing;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

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

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        await base.AfterCreatureAddedToCombat(creature);

        // 必须判断触发这个钩子的是不是平民自己（因为场上任何单位加入都会触发这个钩子）
        if (creature == this.Creature)
        {
            // 适当等待，确保生成动画和视觉节点准备完毕
            await Cmd.Wait(1.0f); 

            // 1. 先获取本地化翻译好的文本
            string startText = new LocString("monsters", "PENANCEMOD-VOLSINII_CIVILIAN.dialogs.START").GetFormattedText();

            // 2. 🌟 直接调用底层的 Create 方法，在这里传入你想要的任意秒数（比如 10.0 秒）
            var bubble = NSpeechBubbleVfx.Create(startText, creature, 10.0, VfxColor.White);

            // 3. 手动把它挂载到平民头上的特效节点里
            if (bubble != null)
            {
                creature.GetVfxContainer()?.AddChildSafely(bubble);
            }
        }
    }

    // 战斗即将宣布胜利时触发
    public override async Task AfterCombatVictoryEarly(CombatRoom room)
    {
        await base.AfterCombatVictoryEarly(room);

        // 判断平民是否存活
        if (Creature.IsAlive)
        {
            // 1. 说话并等待
            TalkCmd.Play(new LocString("monsters", "PENANCEMOD-VOLSINII_CIVILIAN.dialogs.ESCAPE"), Creature, VfxColor.White, VfxDuration.Standard);
            await Cmd.Wait(1.5f);

            // 2. 播放原地跑步的动画 
            await CreatureCmd.TriggerAnim(Creature, "Move", 0f);

            // 3. 获取平民的视觉节点 (NCreature)
            var creatureNode = Creature.GetCreatureNode();
            if (creatureNode != null)
            {
                creatureNode.ToggleIsInteractable(false);

                // 🌟 关键：获取不受布局限制的视觉子节点
                var visualsNode = creatureNode.GetNodeOrNull<Node2D>("Visuals");
                
                if (visualsNode != null)
                {
                    var tween = visualsNode.CreateTween();
                    tween.SetParallel(true); 

                    // 操作 visualsNode 的位置和透明度
                    var targetPosition = new Godot.Vector2(visualsNode.Position.X + 800f, visualsNode.Position.Y);
                    tween.TweenProperty(visualsNode, "position", targetPosition, 1.0f);

                    var targetColor = new Godot.Color(1f, 1f, 1f, 0f);
                    tween.TweenProperty(visualsNode, "modulate", targetColor, 1.0f);

                    // 等待视觉节点的动画完成
                    await TweenHelper.AwaitFinished(tween, visualsNode);
                }
                else
                {
                    await Cmd.Wait(1.0f); // 兜底
                }
            }
            else
            {
                // 兜底逻辑：如果因为某种原因没拿到视觉节点，干等1秒
                await Cmd.Wait(1.0f);
            }

            // 4. 调用原生系统的逃跑逻辑，彻底从战斗底层清理掉该实体
            await CreatureCmd.Escape(Creature);
        }
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