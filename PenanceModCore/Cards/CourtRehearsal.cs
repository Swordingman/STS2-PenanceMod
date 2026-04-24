using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class CourtRehearsal : PenanceBaseCard
{
    // 耗能 0，类型 Skill，稀有度 Uncommon
    // ⚠️ 因为几乎打不出/打出没效果，使用我们刚学到的 TargetType.None
    public CourtRehearsal() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    // 绑定 保留(Retain) 和 消耗(Exhaust)
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => [
        MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Retain,
        MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Exhaust
    ];

    // ==========================================
    // 抽到时触发变身
    // ==========================================
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this)
        {
            // 给引擎一点点时间完成抽牌动画，然后再变，视觉效果更好
            await Cmd.Wait(0.15f);
            await TransformIntoRandomCard();
        }
    }

    // ==========================================
    // 变身核心逻辑
    // ==========================================
    private async Task TransformIntoRandomCard()
    {
        var owner = Owner;
        if (owner == null) return;

        // 1. 获取随机卡牌 (过滤掉诅咒、状态牌，以及这张牌本身防止套娃)
        var allCards = ModelDb.AllCardPools
            .SelectMany(p => p.AllCardIds)
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(c => c.Type != CardType.Curse && c.Type != CardType.Status && c.Id.Entry != this.Id.Entry)
            .ToList();

        if (allCards.Count == 0) return;

        int randomIndex = owner.RunState.Rng.Shuffle.NextInt(allCards.Count);
        var randomCard = allCards[randomIndex].ToMutable();

        // 如果“庭审预演”已升级，则生成的牌也强制升级
        if (IsUpgraded && randomCard.IsUpgradable)
        {
            randomCard.UpgradeInternal();
            randomCard.FinalizeUpgradeInternal();
        }

        // 💡 [高阶提示]：为了平替你在 StS1 写的 Patch，二代你应该在这里给随机牌加上修饰器
        // 比如： randomCard.AddModifier(new RehearsalModifier());
        // 修饰器内部可以监听 OnTurnStartInHand 事件，再次调用类似这里的变身逻辑。

        // 2. 将“庭审预演”本体从手牌/战斗中直接抹除 (skipVisuals: true 让它瞬间消失)
        await CardPileCmd.RemoveFromCombat(this, skipVisuals: true);

        // 3. 将新牌加入战斗，并送入手牌 (触发原生获取卡牌动画)
        await CardPileCmd.AddGeneratedCardToCombat(randomCard, PileType.Hand, addedByPlayer: false);
    }

    // ==========================================
    // 兜底逻辑
    // ==========================================
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 就像你注释里说的，基本不会被直接打出。
        // 为了满足 async Task 签名，这里直接返回。
        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
    }
}