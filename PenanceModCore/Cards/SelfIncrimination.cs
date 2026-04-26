using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class SelfIncrimination : PenanceBaseCard
{
    // 耗能 1，类型 Skill，稀有度 Uncommon，目标 Self
    public SelfIncrimination() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 基础关键词：消耗
    //
    // 用法说明：
    // CardKeyword.Exhaust 会让这张牌打出后进入消耗堆。
    // 这里不要在 OnPlay 里手动消耗自己，CardModel 的结算流程会根据关键词自动处理。
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];

    // 预览两张衍生牌：ExhibitA 和 Perjury
    //
    // 用法说明：
    // ExtraHoverTips 是 STS2 用来给卡牌额外追加悬浮提示/预览的接口。
    // HoverTipFactory.FromCard<T>() 会让玩家查看本牌时，在旁边预览 T 这张卡。
    // 注意：这只负责 UI 预览，不会真正创建卡牌。
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<ExhibitA>(),
        HoverTipFactory.FromCard<Perjury>()
    ];

    // 打出限制：
    // 只有手牌中存在至少 1 张诅咒牌时，本牌才能打出。
    //
    // 用法说明：
    // IsPlayable 只负责“能不能打出”。
    // 真正打出后的效果写在 OnPlay 里。
    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            // 非战斗/预览场景下不要强行检查手牌，避免卡牌图鉴或奖励预览时报错。
            if (!IsInCombat)
                return true;

            var player = Owner;
            if (player == null)
                return true;

            // STS2 中不要用 player.Hand。
            // 手牌通过 PileType.Hand.GetPile(player) 获取。
            var hand = PileType.Hand.GetPile(player);

            return hand.Cards.Any(card => card.Type == CardType.Curse);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player == null)
            return;

        // 只允许选择手牌里的诅咒牌。
        //
        // 用法说明：
        // 这个 filter 会传给 CardSelectCmd.FromHand。
        // 选择界面只会显示满足 filter 的卡。
        bool CurseFilter(CardModel card)
        {
            return card.Type == CardType.Curse;
        }

        // 再做一次安全检查。
        // 理论上 IsPlayable 已经挡住了没诅咒的情况，
        // 但 OnPlay 里保留检查可以避免特殊自动打出/复制打出时出错。
        var cursesInHand = PileType.Hand.GetPile(player)
            .Cards
            .Where(CurseFilter)
            .ToList();

        if (cursesInHand.Count == 0)
            return;

        // 选择界面标题文本。
        //
        // 对应本地化建议：
        // "PENANCEMOD-SELF_INCRIMINATION.select_message": "选择 1 张诅咒牌消耗。"
        LocString selectMessage = new LocString(
            "cards",
            "PENANCEMOD-SELF_INCRIMINATION.select_message"
        );

        // CardSelectorPrefs 用来配置选牌界面。
        //
        // 用法说明：
        // new CardSelectorPrefs(selectMessage, 1)
        // 代表至少选择 1 张，通常也就是选择 1 张。
        //
        // RequireManualConfirmation = true
        // 代表即使只有 1 张可选牌，也尽量打开选择确认界面。
        // 如果你想让只有 1 张诅咒时自动选择，可以把这一行删掉。
        var prefs = new CardSelectorPrefs(selectMessage, 1)
        {
            RequireManualConfirmation = true
        };

        // CardSelectCmd.FromHand 是这张牌的核心选择接口。
        //
        // 参数说明：
        // choiceContext : 当前玩家选择上下文，用于本地/多人/回放同步。
        // player        : 执行选择的玩家。
        // prefs         : 选择界面的标题、数量、是否需要确认等配置。
        // filter        : 哪些手牌可以被选择；这里限制为 Curse。
        // source        : 触发这次选择的来源，一般传 this。
        //
        // 返回值：
        // IEnumerable<CardModel>，也就是玩家实际选中的牌。
        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            player,
            prefs,
            CurseFilter,
            this
        );

        var cardToExhaust = selectedCards.FirstOrDefault();
        if (cardToExhaust == null)
            return;

        // 消耗玩家选中的诅咒牌。
        //
        // 用法说明：
        // CardCmd.Exhaust 用来把指定卡牌消耗掉。
        // 这里消耗的是选中的诅咒，不是 SelfIncrimination 自己。
        await CardCmd.Exhaust(choiceContext, cardToExhaust, causedByEthereal: false);

        // 稍微停顿一下，让消耗动画先表现出来。
        await Cmd.Wait(0.15f);

        // 创建并加入两张衍生牌到手牌。
        await CreateGeneratedCardInHand<ExhibitA>(player);
        await CreateGeneratedCardInHand<Perjury>(player);
    }

    // 创建一张指定类型的卡，并加入玩家手牌。
    //
    // 用法说明：
    // T 必须是 CardModel，并且有无参构造。
    // new T() 只是在代码里创建卡牌实例；
    // card.Owner = player 是把这张卡归属给当前玩家；
    // CardPileCmd.Add(card, PileType.Hand, ...) 才是真正放入手牌。
    private static async Task<T> CreateGeneratedCardInHand<T>(Player player)
        where T : CardModel, new()
    {
        var card = new T
        {
            Owner = player
        };

        await CardPileCmd.Add(
            card,
            PileType.Hand,
            CardPilePosition.Bottom
        );

        return card;
    }

    protected override void OnUpgrade()
    {
        // 升级减费：1 -> 0
        //
        // 如果你的 BaseLib / PenanceBaseCard 里 EnergyCost.UpgradeBy(-1) 可用，
        // 这句就保留。
        //
        // 如果之后编译报 EnergyCost 没有 UpgradeBy，
        // 就需要换成你项目里封装的升级费用方法。
        EnergyCost.UpgradeBy(-1);
    }
}