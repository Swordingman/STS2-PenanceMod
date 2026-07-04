using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.Scripts.Utils; 

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class ChapterOfPenance : CustomRelicModel
{
    // 设置为特殊稀有度，这样它只会在开局被我们强塞，正常打怪或开宝箱拿不到
    public override RelicRarity Rarity => RelicRarity.Starter; 

    public override string PackedIconPath => $"res://PenanceMod/images/relics/large/{nameof(ChapterOfPenance)}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/large/{nameof(ChapterOfPenance)}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{nameof(ChapterOfPenance)}.png";
}