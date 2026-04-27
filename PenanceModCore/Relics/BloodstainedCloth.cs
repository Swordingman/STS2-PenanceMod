using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class BloodstainedCloth : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{Id.Entry.ToLowerInvariant()}.png";

    // 注册变量：[0] 力量 (-2)，[1] 荆棘 (8)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Cloth-Str", -2m),
        new DynamicVar("Cloth-Thorns", 8m)
    ];

    public override async Task BeforeCombatStart()
    {
        var creature = Owner?.Creature;
        if (creature == null) return;

        Flash();

        int strAmt = DynamicVars["Cloth-Str"].IntValue;
        int thornsAmt = DynamicVars["Cloth-Thorns"].IntValue;

        await PowerCmd.Apply<StrengthPower>(creature, strAmt, creature, null);
        await PowerCmd.Apply<ThornAuraPower>(creature, thornsAmt, creature, null);
    }
}