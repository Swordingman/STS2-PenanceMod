using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.GameInfo.Objects;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class WandAntigravity : PenanceBaseCard
{
    public WandAntigravity() : base(1, CardType.Curse, CardRarity.Curse, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves),
        ..WolfCurseHelper.GetWolfCurseHoverTips(IsUpgraded)
    ];

    private bool _autoPlaying;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this)
            return;

        if (_autoPlaying)
            return;

        _autoPlaying = true;

        try
        {
            await TriggerWolfAutoplay(choiceContext, card);
        }
        finally
        {
            _autoPlaying = false;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;

        // 1. 消耗所有不在消耗堆中的诅咒（手牌、抽牌堆、弃牌堆）
        var otherPiles = new[] { PileType.Hand, PileType.Draw, PileType.Discard };
        foreach (var pileType in otherPiles)
        {
            var pile = pileType.GetPile(player);
            // 找出所有诅咒（排除掉正在打出的这张秘杖本身，防止自杀式报错）
            var curses = pile.Cards.Where(c => c.Type == CardType.Curse && c != this).ToList();
            foreach (var curse in curses)
            {
                await CardCmd.Exhaust(choiceContext, curse, causedByEthereal: false);
            }
        }

        // 2. 统计消耗堆中的目标数量
        var exhaustPile = PileType.Exhaust.GetPile(player);
        int targetCount = 0;

        if (IsUpgraded)
        {
            // 升级后：每有一张牌（不限类型）
            targetCount = exhaustPile.Cards.Count;
        }
        else
        {
            // 升级前：每有一张诅咒
            targetCount = exhaustPile.Cards.Count(c => c.Type == CardType.Curse);
        }

        // 3. 根据数量洗入随机狼群诅咒
        if (targetCount > 0)
        {
            for (int i = 0; i < targetCount; i++)
            {
                // 使用你提供的模板生成随机狼群诅咒
                var randomCurse = WolfCurseHelper.GetRandomWolfCurse(player, CombatState, IsUpgraded);
                
                // 将生成的牌加入弃牌堆，并添加一个预览动画
                var cardNode = await CardPileCmd.AddGeneratedCardToCombat(randomCurse, PileType.Discard, Owner);
                CardCmd.PreviewCardPileAdd(cardNode, 1.5f); 
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级逻辑已经在 OnPlay 中通过 IsUpgraded 实现
    }
}