using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class BlackUmbrella : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    // 沿用标准的 Id.Entry.ToLowerInvariant() 图片加载逻辑
    public override string PackedIconPath => $"res://PenanceMod/images/relics/large/{nameof(BlackUmbrella)}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/large/{nameof(BlackUmbrella)}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{nameof(BlackUmbrella)}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Umbrella-Vuln", 2m),
        new DynamicVar("Umbrella-Str", 1m)
    ];

    // 🌟 核心：使用 SavedProperty 确保战斗中 S/L 状态不会被重置
    [SavedProperty]
    public bool PenanceMod_TriggeredThisCombat { get; set; }

    public override Task BeforeCombatStart()
    {
        // 每次战斗开始时重置触发状态和 UI 状态
        PenanceMod_TriggeredThisCombat = false;
        Status = RelicStatus.Normal; 
        return Task.CompletedTask;
    }

    private Creature? _pendingAttacker;
    private PlayerChoiceContext? _pendingChoiceContext;

    public override Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target.Player == Owner && target.Block > 0)
        {
            _pendingAttacker = dealer;
            _pendingChoiceContext = choiceContext;
        }

        return Task.CompletedTask;
    }

    #if STS2_BETA
    public override async Task AfterBlockBroken(PlayerChoiceContext choiceContext, Creature target, Creature? breaker)
    {
        if (PenanceMod_TriggeredThisCombat || target.Player != Owner)
            return;

        PenanceMod_TriggeredThisCombat = true;
        Status = RelicStatus.Disabled;
        Flash();

        int vulnAmt = DynamicVars["Umbrella-Vuln"].IntValue;
        int strAmt = DynamicVars["Umbrella-Str"].IntValue;

        if (breaker != null)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, breaker, vulnAmt, target, null);

        await PowerCmd.Apply<StrengthPower>(choiceContext, target, strAmt, target, null);
    }
    #else
    public override async Task AfterBlockBroken(Creature creature)
    {
        if (PenanceMod_TriggeredThisCombat)
            return;

        if (creature.Player != Owner)
            return;

        PenanceMod_TriggeredThisCombat = true;
        Status = RelicStatus.Disabled;
        Flash();

        var choiceContext = _pendingChoiceContext ?? new ThrowingPlayerChoiceContext();
        var attacker = _pendingAttacker;

        int vulnAmt = DynamicVars["Umbrella-Vuln"].IntValue;
        int strAmt = DynamicVars["Umbrella-Str"].IntValue;

        if (attacker != null)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, attacker, vulnAmt, creature, null);

        await PowerCmd.Apply<StrengthPower>(choiceContext, creature, strAmt, creature, null);

        _pendingAttacker = null;
        _pendingChoiceContext = null;
    }
    #endif
}