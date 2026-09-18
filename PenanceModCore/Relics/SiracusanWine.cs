using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class SiracusanWine : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override string PackedIconPath =>
        $"res://PenanceMod/images/relics/large/{nameof(SiracusanWine)}.png";

    protected override string PackedIconOutlinePath =>
        $"res://PenanceMod/images/relics/large/{nameof(SiracusanWine)}.png";

    protected override string BigIconPath =>
        $"res://PenanceMod/images/relics/large/{nameof(SiracusanWine)}.png";

    // 每场战斗累计失去 8 点生命后：
    // 最大生命值 +3，本次效果总共回复 4 点生命。
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Wine-TriggerDmg", 8m),
        new DynamicVar("Wine-MaxHp", 3m),
        new DynamicVar("Wine-Heal", 4m)
    ];

    // 纯战斗内状态，不参与存档。
    private int _damageTakenThisCombat;
    private bool _triggeredThisCombat;

    public override Task BeforeCombatStart()
    {
        _damageTakenThisCombat = 0;
        _triggeredThisCombat = false;
        Status = RelicStatus.Normal;

        return Task.CompletedTask;
    }

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        // 只计算遗物持有者自己的 HP 变化。
        if (creature != Owner?.Creature)
            return;

        // 本场已经触发过。
        if (_triggeredThisCombat)
            return;

        // delta < 0 才表示实际失去生命。
        // 回血、增加生命等正向变化全部忽略。
        if (delta >= 0m)
            return;

        int hpLost = (int)(-delta);
        _damageTakenThisCombat += hpLost;

        int triggerDmg = DynamicVars["Wine-TriggerDmg"].IntValue;

        // 尚未累计达到触发阈值。
        if (_damageTakenThisCombat < triggerDmg)
            return;

        // 先锁定触发状态。
        // 后面的 GainMaxHp / Heal 会再次产生 HP Changed Hook，
        // 因此必须在它们之前设置。
        _triggeredThisCombat = true;
        Status = RelicStatus.Disabled;
        Flash();

        int maxHpGain = DynamicVars["Wine-MaxHp"].IntValue;
        int totalHeal = DynamicVars["Wine-Heal"].IntValue;

        // GainMaxHp(+3) 本身同时会令当前生命 +3。
        if (maxHpGain > 0)
            await CreatureCmd.GainMaxHp(creature, maxHpGain);

        // Wine-Heal 表示整个遗物效果最终回复的总量。
        // +3 MaxHP 已经附带回复了 3，所以这里只补剩余的 1。
        int additionalHeal = totalHeal - maxHpGain;

        if (additionalHeal > 0)
            await CreatureCmd.Heal(creature, additionalHeal);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _damageTakenThisCombat = 0;
        _triggeredThisCombat = false;
        Status = RelicStatus.Normal;

        return Task.CompletedTask;
    }
}