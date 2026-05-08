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

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class CourtRehearsal : PenanceBaseCard
{
    public CourtRehearsal() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;

        var player = Owner;
        if (player == null) return;

        // 1. 获取随机牌
        var candidates = ModelDb.AllCardPools
            .SelectMany(pool => pool.AllCardIds)
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(c => c != null && c.Type != CardType.Curse && c.Type != CardType.Status && c.Id.Entry != this.Id.Entry)
            .ToList();

        var randomCard = CardFactory.GetDistinctForCombat(
            player, candidates, 1, player.RunState.Rng.CombatCardGeneration
        ).FirstOrDefault();

        if (randomCard != null)
        {
            // 2. 继承升级状态
            if (this.IsUpgraded && randomCard.IsUpgradable && !randomCard.IsUpgraded)
            {
                randomCard.UpgradeInternal();
                randomCard.FinalizeUpgradeInternal();
            }

            // 3. ✅ 强行注入灵魂属性
            randomCard.AddKeyword(CardKeyword.Retain);
            randomCard.AddKeyword(CardKeyword.Exhaust);

            // 4. 变身！（这句代码执行后，this 将彻底从手牌中消失）
            await CardCmd.Transform(this, randomCard);

            // 5. ✅ 召唤幕后追踪器接管
            await PowerCmd.Apply<CourtRehearsalTrackerPower>(choiceContext, player.Creature, 1, player.Creature, null);

            // 找到刚才挂上的那个追踪器，把这张新牌交给它照看
            // 因为允许实例化(Instanced)，我们找 TrackedCard 还是 null 的那个
            var tracker = player.Creature.Powers.OfType<CourtRehearsalTrackerPower>().LastOrDefault(p => p.TrackedCard == null);
            if (tracker != null)
            {
                tracker.TrackedCard = randomCard;
                tracker.IsUpgradedMode = this.IsUpgraded;
            }
        }
    }

    // 这些不需要了，因为在打出或回合开始时它早就不是它自己了
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;
    protected override void OnUpgrade() {}
}