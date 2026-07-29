using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class CourtRehearsal : PenanceBaseCard
{
    private bool _isTransforming;
    private bool _hasTransformed;

    public CourtRehearsal() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    private bool IsActuallyInHand(Player player)
    {
        if (Pile?.Type != PileType.Hand) return false;

        var handPile = PileType.Hand.GetPile(player);
        return handPile.Cards.Any(card => object.ReferenceEquals(card, this));
    }

    private CardModel? GenerateRandomCard(Player player)
    {
        var candidates = ModelDb.AllCardPools
            .OfType<PenanceModCardPool>()
            .SelectMany(pool => pool.AllCardIds)
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(card => card != null
                && card is not CourtRehearsal
                && card.Type != CardType.Curse
                && card.Type != CardType.Status)
            .ToList();

        if (candidates.Count == 0) return null;

        return CardFactory.GetDistinctForCombat(player, candidates, 1, player.RunState.Rng.CombatCardGeneration).FirstOrDefault();
    }

    private async Task CheckAndTransform()
    {
        if (_isTransforming || _hasTransformed) return;

        var player = Owner;
        var combatState = CombatState ?? player?.Creature.CombatState;
        if (player == null || combatState == null) return;
        if (!IsActuallyInHand(player)) return;

        _isTransforming = true;

        try
        {
            var randomCard = GenerateRandomCard(player);
            if (randomCard == null) return;

            var upgradedMode = IsUpgraded;

            if (upgradedMode && randomCard.IsUpgradable && !randomCard.IsUpgraded)
            {
                randomCard.UpgradeInternal();
                randomCard.FinalizeUpgradeInternal();
            }

            randomCard.AddKeyword(CardKeyword.Retain);
            randomCard.AddKeyword(CardKeyword.Exhaust);

            // 防止生成随机牌期间，本体被其他效果移出手牌。
            if (!IsActuallyInHand(player)) return;

            await CardCmd.Transform(this, randomCard);
            _hasTransformed = true;

            var tracker = player.Creature.Powers.OfType<CourtRehearsalTrackerPower>().FirstOrDefault();

            if (tracker == null)
            {
                ulong? netId = LocalContext.NetId;
                if (!netId.HasValue) return;

                PlayerChoiceContext syntheticContext = new HookPlayerChoiceContext(randomCard, netId.Value, combatState, GameActionType.Combat);
                await PowerCmd.Apply<CourtRehearsalTrackerPower>(syntheticContext, player.Creature, 1, player.Creature, null);

                tracker = player.Creature.Powers.OfType<CourtRehearsalTrackerPower>().FirstOrDefault();
            }

            tracker?.Track(randomCard, upgradedMode);
        }
        finally
        {
            _isTransforming = false;
        }
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (!object.ReferenceEquals(card, this)) return;
        if (_isTransforming || _hasTransformed) return;
        if (Pile?.Type != PileType.Hand) return;

        /*
         * CardCmd.Transform 把 replacement 放回原牌堆时，
         * 会把目标牌堆类型作为 oldPileType 传进来。
         *
         * 如果在手牌中变形成庭审预演，这里收到的就是 Hand。
         * 此时外层 Transform 还没有完成手牌节点替换，不能再次 Transform。
         * 稍后交给 AfterCardGeneratedForCombat 处理。
         */
        if (oldPileType == PileType.Hand) return;

        // 正常抽牌、从弃牌堆返回、特殊效果直接塞入手牌等路径。
        await CheckAndTransform();
    }

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (!object.ReferenceEquals(card, this)) return;
        if (_isTransforming || _hasTransformed) return;
        if (Pile?.Type != PileType.Hand) return;

        /*
         * 如果这张庭审预演是由另一张牌 Transform 出来的，
         * 此时外层 Transform 已经处理完手牌节点和动画，可以安全继续。
         */
        await CheckAndTransform();
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

    protected override void OnUpgrade()
    {
    }
}