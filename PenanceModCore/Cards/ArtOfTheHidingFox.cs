using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
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
    private bool _retainEnergyOnce;

    public override bool ShouldReceiveCombatHooks => base.ShouldReceiveCombatHooks || _retainEnergyOnce;

    public override bool ShouldPlayerResetEnergy(Player player)
    {
        if (!_retainEnergyOnce)
            return true;

        if (player != Owner)
            return true;

        _retainEnergyOnce = false;
        return false;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;

        await AudioManager.PlayCustomSfx(WolfCurseSfx);
        if (PenanceConfig.EnableWolfCurseSpeak)
        {
            string audioPath = PenanceConfig.CharacterVoice switch
            {
                VoiceLanguage.EN => "res://PenanceMod/scenes/audio/artofthehidingfox_en.wav",
                VoiceLanguage.JP => "res://PenanceMod/scenes/audio/artofthehidingfox_jp.wav",
                VoiceLanguage.KR => "res://PenanceMod/scenes/audio/artofthehidingfox_kr.wav",
                VoiceLanguage.IT => "res://PenanceMod/scenes/audio/artofthehidingfox_it.wav",
                _ => "res://PenanceMod/scenes/audio/artofthehidingfox_cn.wav",
            };
            await AudioManager.PlayCustomSfx(audioPath);
        }
        
        var vars = DynamicVars.Values.ToList();
        int buffAmount = vars.Count > 0 ? vars[0].IntValue : 3;
        int barrierAmount = vars.Count > 1 ? vars[1].IntValue : 25;

        await PowerCmd.Apply<StrengthPower>(choiceContext, creature, buffAmount, creature, this);
        await ApplyJudgement(creature, buffAmount);
        await PowerCmd.Apply<ThornAuraPower>(choiceContext, creature, buffAmount, creature, this);
        await ApplyBarrier(creature, barrierAmount);

        if (IsUpgraded)
        {
            if (Owner.PlayerCombatState is { } combatState)
            {
                _retainEnergyOnce = true;
            }

            await PowerCmd.Apply<RetainHandPower>(choiceContext, creature, 1, creature, this);
        }

        PlayerCmd.EndTurn(Owner, false);
    }

    protected override void OnUpgrade()
    {
    }
}
