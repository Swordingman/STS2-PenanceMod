using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.Scripts;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class CarnivalMoment : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{Id.Entry.ToLowerInvariant()}.png";

    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        // 这里的 Owner.Deck 就是获取 PileType.Deck 对应的 CardPile 实例
        var deck = Owner?.Deck; 
        if (deck == null) return;

         // 1. 升级牌库里已有的狼群诅咒
        var cursesToUpgrade = deck.Cards.Where(c => 
            c.Tags.Contains(PenanceCardTags.CurseOfWolves) && 
            c.IsUpgradable // 源码中判断能否升级的标准属性是 IsUpgradable
        ).ToList();

        foreach (var card in cursesToUpgrade)
        {
            // 🌟 完全遵循 STS2 底层的二段式升级规范
            card.UpgradeInternal();
            card.FinalizeUpgradeInternal();
        }

        // 2. 🌟 核心修正：使用 C# 事件委托，订阅卡牌加入牌库的事件
        deck.CardAdded += OnCardAddedToDeck;
    }

    public override async Task AfterRemoved()
    {
        await base.AfterRemoved();

        var deck = Owner?.Deck;
        if (deck != null)
        {
            // 🌟 必须注销事件，防止玩家失去遗物后依然在后台触发，导致内存泄漏！
            deck.CardAdded -= OnCardAddedToDeck;
        }
    }

    // 真正的事件回调方法
    private void OnCardAddedToDeck(CardModel card)
    {
        if (card.Tags.Contains(PenanceCardTags.CurseOfWolves) && !card.IsUpgraded)
        {
            // 🌟 完全遵循 STS2 底层的二段式升级规范
            card.UpgradeInternal();
            card.FinalizeUpgradeInternal();
            Flash(); // 闪烁提示玩家遗物生效
        }
    }
}