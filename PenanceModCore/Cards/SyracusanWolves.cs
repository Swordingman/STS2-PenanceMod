using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using PenanceMod.Scripts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class SyracusanWolves : PenanceBaseCard
{
    // 耗能 1，类型 Skill，稀有度 Rare，目标 Self
    public SyracusanWolves() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 消耗。
    //
    // 用法说明：
    // CardKeyword.Exhaust 会让这张牌打出后自动进入消耗堆。
    // 不需要在 OnPlay 里手动消耗自己。
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];

    // 预览所有“狼群诅咒”。
    //
    // 用法说明：
    // ExtraHoverTips 是 STS2 的卡牌额外悬浮预览接口。
    // WolfCurseHelper.GetWolfCurseHoverTips(IsUpgraded) 会把所有带 CurseOfWolves Tag 的牌转成 hover tip。
    //
    // 注意：
    // 这里依赖 HoverTipFactory.FromCard(CardModel) 这个重载。
    // 如果你的版本只有 HoverTipFactory.FromCard<T>()，看下面 Helper 里的替代说明。
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves),
        ..WolfCurseHelper.GetWolfCurseHoverTips(IsUpgraded)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;

        // 原版逻辑：随机 5 张狼群诅咒洗入抽牌堆。
        const int amount = 5;

        for (int i = 0; i < amount; i++)
        {
            var randomCurse = WolfCurseHelper.GetRandomWolfCurse(player, CombatState, IsUpgraded);

            // 加入抽牌堆。
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(randomCurse, PileType.Draw, Owner), 2.2f);
        }
    }

    protected override void OnUpgrade()
    {
        // 原版升级效果不是减费，而是：
        // “生成的狼群诅咒变为升级后版本”。
        //
        // 在 STS2 中，CardModel.UpgradeInternal() 会先把 CurrentUpgradeLevel +1，
        // 然后调用 OnUpgrade()。
        //
        // 所以这里可以留空。
        // 具体生成升级诅咒的逻辑在 OnPlay 里通过 IsUpgraded 传给 Helper。
    }
}