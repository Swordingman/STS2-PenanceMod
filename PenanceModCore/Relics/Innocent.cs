using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class Innocent : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{Id.Entry.ToLowerInvariant()}.png";

    // 注册变量：20%
    // 供你的“裁决”伤害计算公式外部读取：
    // int extraDmg = player.HasRelic<Innocent>() ? player.GetRelic<Innocent>().DynamicVars["Innocent-DmgUp"].IntValue : 0;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Innocent-DmgUp", 20m)
    ];
}