using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Patches.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
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

        // 获取抽牌堆。
        //
        // 用法说明：
        // PileType.Draw.GetPile(player) 代表玩家当前战斗中的抽牌堆。
        // 如果你的 IDE 里没有 PileType.Draw，看看枚举是不是叫 PileType.DrawPile。
        var drawPile = PileType.Draw.GetPile(player);

        // 从抽牌堆中筛选所有能力牌。
        //
        // 注意：
        // 这里必须使用抽牌堆里的原始 CardModel 实例。
        // 不要 ToMutable()，不要 new。
        // 因为后面的 UnwrittenLawPower 要记录的就是这张牌的引用。
        var powersInDrawPile = drawPile.Cards
            .Where(card => card.Type == CardType.Power)
            .ToList();

        if (powersInDrawPile.Count == 0)
            return;

        // 随机选一张能力牌。
        //
        // 用法说明：
        // 使用 player.RunState.Rng.Shuffle，而不是 System.Random。
        // 这样更适合游戏的种子、回放和同步逻辑。
        int index = player.RunState.Rng.Shuffle.NextInt(powersInDrawPile.Count);
        CardModel selectedPowerCard = powersInDrawPile[index];

        // 判断手牌是否已满。
        //
        // 用法说明：
        // MaxHandSizePatch.GetMaxHandSize(player) 会读取当前真实手牌上限，
        // 包括其他 mod 或效果对手牌上限的修改。
        var hand = PileType.Hand.GetPile(player);
        int maxHandSize = MaxHandSizePatch.GetMaxHandSize(player);

        if (hand.Cards.Count < maxHandSize)
        {
            // 手牌没满：把这张能力牌从抽牌堆移动到手牌。
            //
            // CardPileCmd.Add(card, PileType.Hand)：
            // 如果 card 已经在某个战斗牌堆中，它会被移动到目标牌堆。
            await CardPileCmd.Add(selectedPowerCard, PileType.Hand);
        }
        else
        {
            // 手牌已满：把这张能力牌从抽牌堆移动到弃牌堆。
            //
            // 一代逻辑里手牌满时会进弃牌堆，
            // 这里保持相同逻辑。
            await CardPileCmd.Add(selectedPowerCard, PileType.Discard);
        }

        // 给玩家施加 UnwrittenLawPower，并把目标卡传进去。
        //
        // 注意：
        // 不能直接用 PowerCmd.Apply<UnwrittenLawPower>(...)
        // 因为那样没有机会在 Apply 前调用 Initialize(selectedPowerCard)。
        //
        // 正确做法：
        // 1. 从 ModelDb 取 Power 原型；
        // 2. ToMutable() 得到可变实例；
        // 3. Initialize 记录目标卡；
        // 4. PowerCmd.Apply(power, ...) 应用这个已经初始化过的实例。
        var existingPower = creature.GetPower<UnwrittenLawPower>();

        if (existingPower != null)
        {
            // 如果本回合已经有这个 Buff，就追加一个目标。
            // 这样可以支持一回合内多次打出“不成文的法律”。
            existingPower.AddTarget(selectedPowerCard);
        }
        else
        {
            var power = (UnwrittenLawPower)ModelDb.Power<UnwrittenLawPower>().ToMutable();

            power.Initialize(selectedPowerCard);

            await PowerCmd.Apply(
                power,
                creature,
                1,
                creature,
                this
            );
        }
    }

    protected override void OnUpgrade()
    {
        // 升级减费：1 -> 0
        //
        // 如果你的项目里 EnergyCost.UpgradeBy(-1) 可用，就这样写。
        // 如果报错，换成你 PenanceBaseCard / BaseLib 里封装的升级费用方法。
        EnergyCost.UpgradeBy(-1);
    }
}