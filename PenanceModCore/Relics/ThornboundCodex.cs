using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Threading.Tasks;
using PenanceMod.Scripts;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class ThornboundCodex : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{Id.Entry.ToLowerInvariant()}.png";

    // 🌟 核心细节：让遗物在悬停时能预览所有可能的狼群诅咒
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        WolfCurseHelper.GetWolfCurseHoverTips(false); 

    // 获得遗物时：增加基础能量上限
    public override async Task AfterObtained()
    {
        await base.AfterObtained();
        var player = Owner;
        if (player != null)
        {
            // 注：如果 STS2 玩家基础能量属性不叫 BaseEnergy，请点出自动补全找类似 MaxEnergy 的属性
            player.MaxEnergy += 1; 
        }
    }

    // 失去遗物时：扣除基础能量上限
    public override async Task AfterRemoved()
    {
        await base.AfterRemoved();
        var player = Owner;
        if (player != null)
        {
            player.MaxEnergy -= 1;
        }
    }

    // 🌟 回合结束时：洗入一张随机狼群诅咒
    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        var player = Owner;
        // 判定：仅在己方回合结束时触发
        if (player == null || side != player.Creature.Side) 
            return;

        Flash();

        var curse = WolfCurseHelper.GetRandomWolfCurse(player);
        if (curse != null)
        {
            curse.Owner = player;
            // 塞入抽牌堆，位置设为随机 (CardPilePosition.Random)
            await CardPileCmd.Add(curse, PileType.Draw, CardPilePosition.Random);
            
            // 稍微停顿，让洗牌动画表现清楚
            await Cmd.Wait(0.1f);
        }
    }
}