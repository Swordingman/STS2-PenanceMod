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
using PenanceMod.PenanceModCode.Extensions;
using PenanceMod.Scripts.Utils;
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

        if (player == null || CombatState == null)
            return;

        await AudioManager.PlayCustomSfx(WolfCurseSfx);

        if (PenanceConfig.EnableWolfCurseSpeak)
        {
            string audioPath = PenanceConfig.CharacterVoice switch
            {
                VoiceLanguage.EN => "res://PenanceMod/scenes/audio/wandantigravity_en.wav",
                VoiceLanguage.JP => "res://PenanceMod/scenes/audio/wandantigravity_jp.wav",
                VoiceLanguage.KR => "res://PenanceMod/scenes/audio/wandantigravity_kr.wav",
                VoiceLanguage.IT => "res://PenanceMod/scenes/audio/wandantigravity_cn.wav",
                _ => "res://PenanceMod/scenes/audio/wandantigravity_cn.wav",
            };

            await AudioManager.PlayCustomSfx(audioPath);
        }


        // 收集需要消耗的诅咒
        var cursesToExhaust = new List<(CardModel card, bool skipVisuals)>();


        var handPile = PileType.Hand.GetPile(player);
        var drawPile = PileType.Draw.GetPile(player);
        var discardPile = PileType.Discard.GetPile(player);


        foreach (var card in handPile.Cards.Where(c => c.Type == CardType.Curse && c != this))
        {
            cursesToExhaust.Add((card, false));
        }


        foreach (var card in drawPile.Cards.Where(c => c.Type == CardType.Curse && c != this))
        {
            cursesToExhaust.Add((card, true));
        }


        foreach (var card in discardPile.Cards.Where(c => c.Type == CardType.Curse && c != this))
        {
            cursesToExhaust.Add((card, true));
        }


        // 串行 Exhaust，避免多人同步状态分裂
        foreach (var item in cursesToExhaust)
        {
            await CardCmd.Exhaust(
                choiceContext,
                item.card,
                causedByEthereal: false,
                skipVisuals: item.skipVisuals
            );
        }


        // 按原设计统计 Exhaust 堆数量
        var exhaustPile = PileType.Exhaust.GetPile(player);
        int targetCount = 0;

        if (IsUpgraded)
        {
            // 升级：每张消耗牌
            targetCount = exhaustPile.Cards.Count;
        }
        else
        {
            // 未升级：每张消耗的诅咒
            targetCount = exhaustPile.Cards.Count(c => c.Type == CardType.Curse);
        }


        // 生成随机狼群诅咒
        for (int i = 0; i < targetCount; i++)
        {
            var randomCurse = WolfCurseHelper.GetRandomWolfCurse(
                player,
                CombatState,
                IsUpgraded,
                typeof(WandAntigravity)
            );


            var cardNode = await CardPileCmd.AddGeneratedCardToCombat(
                randomCurse,
                PileType.Discard,
                Owner
            );

            CardCmd.PreviewCardPileAdd(cardNode, 0.5f);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级逻辑在 OnPlay 中处理
    }
}