using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Extensions;
using PenanceMod.PenanceModCode.Powers;
using PenanceMod.Scripts.Utils;
using System.Linq;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class ArtOfTheHidingFox : PenanceBaseCard
{
    public ArtOfTheHidingFox() : base(1, CardType.Curse, CardRarity.Curse, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves)
    ];
    
    // �?顺序注册动态变量：索引0是Buff量，索引1是屏障量
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Fox-Magic", 3m)
            .WithTooltip("PENANCEMOD-JUDGEMENT")
            .WithTooltip("PENANCEMOD-THORN_AURA"),
        new DynamicVar("Fox-Barrier", 25m).WithTooltip("PENANCEMOD-BARRIER")
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
        var creature = Owner.Creature;

        string audioPath = PenanceConfig.CharacterVoice switch
        {
            VoiceLanguage.EN => "res://PenanceMod/scenes/audio/artofthehidingfox_en.wav",
            VoiceLanguage.JP => "res://PenanceMod/scenes/audio/artofthehidingfox_jp.wav",
            VoiceLanguage.KR => "res://PenanceMod/scenes/audio/artofthehidingfox_kr.wav",
            VoiceLanguage.IT => "res://PenanceMod/scenes/audio/artofthehidingfox_it.wav",
            _ => "res://PenanceMod/scenes/audio/artofthehidingfox_cn.wav",
        };
        await AudioManager.PlayCustomSfx(WolfCurseSfx);
        await AudioManager.PlayCustomSfx(audioPath);
        
        // �?安全取值：按注册顺序提�?
        var vars = DynamicVars.Values.ToList();
        int buffAmount = vars.Count > 0 ? vars[0].IntValue : 3;
        int barrierAmount = vars.Count > 1 ? vars[1].IntValue : 25;

        // 1. 获得力量 (假设二代原版依然�?StrengthPower)
        await PowerCmd.Apply<StrengthPower>(choiceContext,creature, buffAmount, creature, this);
        
        // 2. 获得裁决 (直接调用基类方法)
        await ApplyJudgement(creature, buffAmount);
        
        // 3. 获得荆棘环身 (这里假设你已经写�?ThornAuraPower)
        await PowerCmd.Apply<ThornAuraPower>(choiceContext,creature, buffAmount, creature, this);
        
        // 4. 获得屏障 (直接调用基类方法)
        await ApplyBarrier(creature, barrierAmount);

        // 5. 升级后保留手�?
        if (IsUpgraded)
        {
            await PowerCmd.Apply<RetainHandPower>(choiceContext,creature, 1, creature, this);
        }

        // 6. 强制结束回合
        PlayerCmd.EndTurn(Owner, false);
    }

    protected override void OnUpgrade()
    {
    }
}
