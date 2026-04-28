using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands; // 可能需要用到相关指令
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class PlasticMole : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{Id.Entry.ToLowerInvariant()}.png";

    // 注册变量：阈值 (3)，获得能量数 (1)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Mole-Threshold", 3m),
        new DynamicVar("Mole-Energy", 1m)
    ];

    [SavedProperty]
    public int AttackCounter { get; set; }

    public override bool ShowCounter => AttackCounter > 0;
    public override int DisplayAmount => AttackCounter;

    public override Task BeforeCombatStart()
    {
        if (AttackCounter != 0)
        {
            AttackCounter = 0;
            InvokeDisplayAmountChanged(); 
        }
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 🌟 核心修正：通过 player.Creature.CombatState 获取战斗状态
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        int attackIntents = 0;

        // 🌟 核心修正：直接使用源码中的 Enemies 属性，极其优雅
        foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
        {
            // 注：如果 STS2 中判断攻击意图的字段不同，请替换为实际的属性（例如 enemy.Intent?.HasDamage）
            if (enemy.Monster?.IntendsToAttack == true)
            {
                attackIntents++;
            }
        }

        if (attackIntents > 0)
        {
            AttackCounter += attackIntents;
            Flash();

            int threshold = DynamicVars["Mole-Threshold"].IntValue;
            int energyGain = DynamicVars["Mole-Energy"].IntValue;

            if (AttackCounter >= threshold)
            {
                int timesTriggered = AttackCounter / threshold;
                AttackCounter %= threshold; // 保留余数

                // 给予玩家能量。同样，如果引擎改用 Cmd 方式给费，可以直接用 EnergyCmd.Gain(...)
                player.PlayerCombatState?.GainEnergy(timesTriggered * energyGain);
            }

            InvokeDisplayAmountChanged();
            
            await Cmd.Wait(0.1f);
        }
    }
}