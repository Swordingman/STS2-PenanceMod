using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Powers;
using System.Threading.Tasks;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class TestTheLaw : PenanceBaseCard
{
    public TestTheLaw() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        int currentHp = creature.CurrentHp;
        int diff = currentHp - 10;

        // 1. 生命值结算
        if (diff > 0)
        {
            // 超过 10，扣血并获得 3 倍屏障
            await CreatureCmd.Damage(choiceContext, creature, diff, ValueProp.Unblockable, this);
            await ApplyBarrier(creature, diff * 3);
        }
        else if (diff < 0)
        {
            // 不足 10，直接治疗
            await CreatureCmd.Heal(creature, -diff);
        }

        // 2. 挂载终极能力 (放在最后挂载，防止被这波扣血提前触发)
        await PowerCmd.Apply<TestTheLawPower>(choiceContext, creature, 1, creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}