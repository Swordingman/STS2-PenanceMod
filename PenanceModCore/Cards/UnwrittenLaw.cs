using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Patches.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class UnwrittenLaw : PenanceBaseCard
{
    // 耗能 1，类型 Skill，稀有度 Uncommon，目标 Self
    public UnwrittenLaw() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;

        // 1. 获取抽牌堆
        //
        // 用法说明：
        // STS2 里不要用 p.drawPile.group。
        // 牌堆统一通过 PileType.X.GetPile(player) 获取。
        //
        // 如果你的 IDE 里没有 PileType.Draw，
        // 就看提示是否叫 PileType.DrawPile，然后把这里改成对应枚举。
        var drawPile = PileType.Draw.GetPile(player);

        // 2. 找出抽牌堆里的所有能力牌
        var powerCards = drawPile.Cards
            .Where(card => card.Type == CardType.Power)
            .ToList();

        if (powerCards.Count == 0)
            return;

        // 3. 随机选择一张能力牌
        //
        // 用法说明：
        // 用 RunState.Rng.Shuffle，而不是 System.Random。
        // 这样更适合回放、多人同步和固定种子。
        int randomIndex = player.RunState.Rng.Shuffle.NextInt(powerCards.Count);
        CardModel selectedCard = powerCards[randomIndex];

        // 4. 判断手牌是否已满
        //
        // 用法说明：
        // MaxHandSizePatch.GetMaxHandSize(player) 会读取当前实际手牌上限，
        // 比写死 10 更稳。
        var hand = PileType.Hand.GetPile(player);
        int maxHandSize = MaxHandSizePatch.GetMaxHandSize(player);

        bool canMoveToHand = hand.Cards.Count < maxHandSize;

        // 5. 把选中的能力牌从抽牌堆移动到手牌或弃牌堆
        //
        // 原版逻辑：
        // 手牌未满 -> 进手牌
        // 手牌已满 -> 进弃牌堆
        //
        // CardPileCmd.Add(card, pileType) 可以把一张已经存在于战斗牌堆中的牌
        // 移动到指定牌堆。
        if (canMoveToHand)
        {
            await CardPileCmd.Add(selectedCard, PileType.Hand);
        }
        else
        {
            await CardPileCmd.Add(selectedCard, PileType.Discard);

            // 如果你后面找到了“手牌已满提示”的 Cmd，可以放这里。
            // 原版 p.createHandIsFullDialog() 对应的是 UI 提示，不影响核心逻辑。
        }

        // 6. 给玩家施加“不成文的法律”临时 Power
        //
        // 这个 Power 会：
        // - 记录 selectedCard；
        // - 临时让 selectedCard.BaseReplayCount +1；
        // - selectedCard 本回合如果被打出，就额外执行一次；
        // - 打出后自动移除；
        // - 如果本回合没打出，回合结束时也会随 Duration 移除。
        var power = ModelDb.Power<UnwrittenLawPower>().ToMutable() as UnwrittenLawPower;
        if (power == null)
            return;

        power.MarkedCard = selectedCard;

        await PowerCmd.Apply(
            power,
            creature,
            1,
            creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        // 升级减费：1 -> 0
        //
        // 如果你的 BaseLib / CustomCardModel 里 EnergyCost.UpgradeBy(-1) 可用，
        // 这句就保留。
        //
        // 如果这里报错，就换成你项目中其它卡已经在用的减费接口。
        EnergyCost.UpgradeBy(-1);
    }
}