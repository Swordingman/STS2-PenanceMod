using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class LittleGavel : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{Id.Entry.ToLowerInvariant()}.png";

    // 🌟 供你的 Patch 调用的异步方法（格挡被击碎时）
    public async Task TriggerOnBlockBroken(int blockAmountLost)
    {
        if (blockAmountLost <= 0) 
            return;

        var creature = Owner?.Creature;
        if (creature == null) 
            return;

        Flash();

        // 获得等同于被击碎前格挡值的屏障
        await PowerCmd.Apply<BarrierPower>(creature, blockAmountLost, creature, null);
    }
}