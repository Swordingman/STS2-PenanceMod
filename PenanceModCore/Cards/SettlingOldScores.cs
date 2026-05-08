using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class SettlingOldScores : PenanceBaseCard
{
    public SettlingOldScores() : base(1, CardType.Curse, CardRarity.Curse, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Settling-Draw", 2m)
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
        int drawBatchSize = DynamicVars["Settling-Draw"].IntValue;
        int nonAttackCount = 0;

        // 🌟 核心逻辑：重复抽牌，直到累计抽出两张非攻击牌
        while (nonAttackCount < 2)
        {
            var drawnCards = await CardPileCmd.Draw(choiceContext, drawBatchSize, Owner);
            
            // 容错：如果牌库和弃牌堆彻底抽干了，强行中止
            if (drawnCards == null || !drawnCards.Any()) break;

            foreach (var card in drawnCards)
            {
                if (card.Type == CardType.Attack)
                {
                    // 🌟 使用你提供的官方 API 进行自动打出
                    // target 传 null，让 AutoPlay 根据卡牌自身的 TargetType 自动找怪
                    await CardCmd.AutoPlay(choiceContext, card, null);
                    
                    // 打完之后立刻消耗
                    await CardCmd.Exhaust(choiceContext, card, causedByEthereal: false);
                    
                    // 连发间隔，视觉上更清晰
                    await Cmd.Wait(0.5f);
                }
                else
                {
                    nonAttackCount++;
                    // 如果在当前批次中已经凑够两张非攻击牌，提前收工
                    if (nonAttackCount >= 2) break;
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级从 2 张变为 3 张
        DynamicVars["Settling-Draw"].UpgradeValueBy(1);
    }
}