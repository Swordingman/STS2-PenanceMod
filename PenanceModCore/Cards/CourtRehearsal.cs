using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Factories;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class CourtRehearsal : PenanceBaseCard
{
    public CourtRehearsal() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    // 1. 把变身逻辑提取成一个独立的安全检查方法
    private async Task CheckAndTransform()
    {
        // 如果它现在不在手牌里，直接退出，什么都不做
        if (this.Pile == null || this.Pile.Type != PileType.Hand) return;

        var player = Owner;
        var combatState = CombatState ?? player?.Creature.CombatState;
        if (player == null || combatState == null) return;

        // 获取随机牌
        var candidates = ModelDb.AllCardPools
            .OfType<PenanceModCardPool>()
            .SelectMany(pool => pool.AllCardIds)
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(c => c != null && c.Type != CardType.Curse && c.Type != CardType.Status && c.Id.Entry != this.Id.Entry)
            .ToList();

        var randomCard = CardFactory.GetDistinctForCombat(
            player, candidates, 1, player.RunState.Rng.CombatCardGeneration
        ).FirstOrDefault();

        if (randomCard != null)
        {
            if (this.IsUpgraded && randomCard.IsUpgradable && !randomCard.IsUpgraded)
            {
                randomCard.UpgradeInternal();
                randomCard.FinalizeUpgradeInternal();
            }

            randomCard.AddKeyword(CardKeyword.Retain);
            randomCard.AddKeyword(CardKeyword.Exhaust);

            // 变身！
            await Cmd.Wait(0.1f);
            await CardCmd.Transform(this, randomCard);

            ulong? netId = LocalContext.NetId;
            if (!netId.HasValue) return;

            PlayerChoiceContext syntheticContext = new HookPlayerChoiceContext(this, netId.Value, combatState, GameActionType.Combat);
            await PowerCmd.Apply<CourtRehearsalTrackerPower>(syntheticContext, player.Creature, 1, player.Creature, null);

            var tracker = player.Creature.Powers.OfType<CourtRehearsalTrackerPower>().LastOrDefault(p => p.PenanceMod_TrackedCard == null);
            if (tracker != null)
            {
                tracker.PenanceMod_TrackedCard = randomCard;
                tracker.PenanceMod_IsUpgradedMode = this.IsUpgraded;
            }
        }
    }

    // 2. 占领所有可能的入口钩子，全部指向这个检查方法！

    // 入口 A：正常的卡牌堆移动 (抽牌、从弃牌堆捞回等)
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card == this) await CheckAndTransform();
    }

    // 入口 B：凭空印卡专门的钩子！(针对你说的“凭空刷出来”的情况)
    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card == this) await CheckAndTransform();
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;
    protected override void OnUpgrade() {}
}