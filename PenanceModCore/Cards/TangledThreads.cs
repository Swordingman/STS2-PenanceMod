using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Extensions;
using PenanceMod.Scripts.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class TangledThreads : PenanceBaseCard
{
    public TangledThreads() : base(1, CardType.Curse, CardRarity.Curse, TargetType.AllEnemies, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves)
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
        var creature = player.Creature;
        var combatState = creature.CombatState;
        var playerCombatState = player.PlayerCombatState;

        if (combatState == null || playerCombatState == null)
            return;

        await AudioManager.PlayCustomSfx(WolfCurseSfx);
        if (PenanceConfig.EnableWolfCurseSpeak)
        {
            string audioPath = PenanceConfig.CharacterVoice switch
            {
                VoiceLanguage.EN => "res://PenanceMod/scenes/audio/tangledthreads_en.wav",
                VoiceLanguage.JP => "res://PenanceMod/scenes/audio/tangledthreads_jp.wav",
                VoiceLanguage.KR => "res://PenanceMod/scenes/audio/tangledthreads_kr.wav",
                VoiceLanguage.IT => "res://PenanceMod/scenes/audio/tangledthreads_it.wav",
                _ => "res://PenanceMod/scenes/audio/tangledthreads_cn.wav",
            };

            await AudioManager.PlayCustomSfx(audioPath);
        }

        int energyLost = System.Math.Max(0, (int)playerCombatState.Energy);

        if (energyLost <= 0)
            return;

        await PlayerCmd.LoseEnergy(energyLost, player);

        if (IsUpgraded)
        {
            await PlayerCmd.GainEnergy(1, player);
        }

        var hand = PileType.Hand.GetPile(player);

        var candidates = hand.Cards
            .Where(card => !ReferenceEquals(card, this))
            .Where(CanBeMadeFreeThisCombat)
            .ToList();

        int actualCount = System.Math.Min(energyLost, candidates.Count);
        var cardsToMakeFree = PickRandomCards(candidates, actualCount);

        foreach (CardModel card in cardsToMakeFree)
        {
            card.SetToFreeThisCombat();
        }
    }

    private static bool CanBeMadeFreeThisCombat(CardModel card)
    {
        if (card.EnergyCost.CostsX)
            return false;

        return card.EnergyCost.GetWithModifiers(CostModifiers.All) > 0;
    }

    private List<CardModel> PickRandomCards(List<CardModel> source, int count)
    {
        var pool = source.ToList();
        var result = new List<CardModel>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Owner.RunState.Rng.Shuffle.NextInt(pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    protected override void OnUpgrade()
    {
    }
}