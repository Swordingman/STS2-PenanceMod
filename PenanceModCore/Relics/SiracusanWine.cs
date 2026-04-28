using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures; // 根据实际掉血钩子的参数可能需要
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class SiracusanWine : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{Id.Entry.ToLowerInvariant()}.png";

    // 注册变量：触发阈值(8)，最大生命值增加(3)，回复量(4)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Wine-TriggerDmg", 8m),
        new DynamicVar("Wine-MaxHp", 3m),
        new DynamicVar("Wine-Heal", 4m)
    ];

    // 存档属性，确保单场战斗中 S/L 也只触发一次
    [SavedProperty]
    public bool TriggeredThisCombat { get; set; }

    public override Task BeforeCombatStart()
    {
        // 战斗开始时重置状态和 UI（取消变灰）
        TriggeredThisCombat = false;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

    // 🌟 监听掉血的钩子 (具体名称以底层基类自动补全为准)
    public virtual async Task AfterLoseHp(int damageAmount)
    {
        int triggerDmg = DynamicVars["Wine-TriggerDmg"].IntValue;

        if (!TriggeredThisCombat && damageAmount >= triggerDmg)
        {
            TriggeredThisCombat = true;
            
            // 🌟 触发后直接设为 Disabled，引擎会自动将其变灰
            Status = RelicStatus.Disabled;
            Flash();

            var creature = Owner?.Creature;
            if (creature != null)
            {
                int maxHpGain = DynamicVars["Wine-MaxHp"].IntValue;
                int healAmt = DynamicVars["Wine-Heal"].IntValue;

                // 1. 增加最大生命值
                await CreatureCmd.GainMaxHp(creature, maxHpGain);

                // 2. 直接调用回复（得益于你之前的 Patch 设计，这里不会被初始遗物误拦截）
                await CreatureCmd.Heal(creature, healAmt);
            }
        }
    }
}