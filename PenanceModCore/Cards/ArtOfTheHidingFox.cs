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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class ArtOfTheHidingFox : PenanceBaseCard
{
    public ArtOfTheHidingFox() : base(1, CardType.Curse, CardRarity.Curse, TargetType.Self, true) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];

    protected override HashSet<CardTag> CanonicalTags =>
        [PenanceCardTags.CurseOfWolves];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves),
        HoverTipFactory.FromKeyword(PenanceKeywords.Judgement),
        HoverTipFactory.FromKeyword(PenanceKeywords.ThornAura),
        HoverTipFactory.FromKeyword(PenanceKeywords.Barrier),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Fox-Magic", 3m), new DynamicVar("Fox-Barrier", 25m)];

    private bool _retainEnergyOnce;

    // 升级后的狐狸进入消耗堆后，仍需要接收下一次能量重置 Hook。
    public override bool ShouldReceiveCombatHooks => base.ShouldReceiveCombatHooks || _retainEnergyOnce;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;

        // 狐狸不再使用普通 TriggerWolfAutoplay。
        //
        // TriggerForFox 最终不会直接 CardCmd.AutoPlay，
        // 而是 enqueue 一个真正的 PlayCardAction。
        //
        // 因此这里返回以后，当前抽牌 Hook 可以正常结束，
        // 狐狸随后作为独立的出牌 Action 进行结算。
        await TriggerForFox(choiceContext, card);
    }

    public override bool ShouldPlayerResetEnergy(Player player)
    {
        if (!_retainEnergyOnce) return true;
        if (player != Owner) return true;

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
            _retainEnergyOnce = true;
            await PowerCmd.Apply<RetainHandPower>(choiceContext, creature, 1, creature, this);
        }

        // 无论手动还是 TriggerForFox 自动触发，现在都会在真正的 PlayCardAction 中来到这里。
        // 因此统一按照原本已经验证正常的手动逻辑强制结束回合。
        PlayerCmd.EndTurn(Owner, false);
    }

    protected override void OnUpgrade() {}
}