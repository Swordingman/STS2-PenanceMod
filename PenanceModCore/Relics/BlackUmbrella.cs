using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class BlackUmbrella : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    // 沿用标准的 Id.Entry.ToLowerInvariant() 图片加载逻辑
    public override string PackedIconPath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{Id.Entry.ToLowerInvariant()}.png";

    // 注册变量：[0] 易伤层数 (1)，[1] 力量层数 (1)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Umbrella-Vuln", 1m),
        new DynamicVar("Umbrella-Str", 1m)
    ];

    // 🌟 核心：使用 SavedProperty 确保战斗中 S/L 状态不会被重置
    [SavedProperty]
    public bool TriggeredThisCombat { get; set; }

    public override Task BeforeCombatStart()
    {
        // 每次战斗开始时重置触发状态和 UI 状态
        TriggeredThisCombat = false;
        Status = RelicStatus.Normal; 
        return Task.CompletedTask;
    }

    public async Task TriggerOnBlockBroken(Creature attacker)
    {
        if (TriggeredThisCombat) 
            return;

        TriggeredThisCombat = true;
        
        Status = RelicStatus.Disabled; 
        
        Flash();

        var creature = Owner.Creature;
        int vulnAmt = DynamicVars["Umbrella-Vuln"].IntValue;
        int strAmt = DynamicVars["Umbrella-Str"].IntValue;

        if (attacker != null)
            await PowerCmd.Apply<VulnerablePower>(attacker, vulnAmt, creature, null);

        await PowerCmd.Apply<StrengthPower>(creature, strAmt, creature, null);
    }
}