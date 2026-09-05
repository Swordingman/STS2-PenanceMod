using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves),
        HoverTipFactory.FromKeyword(PenanceKeywords.Judgement),
        HoverTipFactory.FromKeyword(PenanceKeywords.ThornAura),
        HoverTipFactory.FromKeyword(PenanceKeywords.Barrier),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Fox-Magic", 3m),
        new DynamicVar("Fox-Barrier", 25m)
    ];

    private bool _autoPlaying;
    private bool _pendingEndTurn;
    private bool _retainEnergyOnce;

    // 确保卡牌进入消耗堆后仍能正确接收 Hook 事件
    public override bool ShouldReceiveCombatHooks => base.ShouldReceiveCombatHooks || _retainEnergyOnce || _pendingEndTurn;

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

        if (_autoPlaying)
        {
            _pendingEndTurn = true;
        }
        else
        {
            PlayerCmd.EndTurn(Owner, false);
        }
    }

    // 场景 1：开局自动打出阶段，等待所有回合开始弹窗及预打出结算完毕后安全结束回合
    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || !_pendingEndTurn)
            return;

        _pendingEndTurn = false;
        PlayerCmd.EndTurn(player, false);
    }

    // 场景 2：常规出牌阶段（Play 阶段）通过过牌卡抽到并自动打出时，在打出完成时结束回合
    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this || !_pendingEndTurn)
            return;

        if (Owner.PlayerCombatState?.Phase == PlayerTurnPhase.Play)
        {
            _pendingEndTurn = false;
            PlayerCmd.EndTurn(Owner, false);
        }
    }

    protected override void OnUpgrade()
    {
    }
}