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
using MegaCrit.Sts2.Core.HoverTips;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class TestTheLaw : PenanceBaseCard
{
    public TestTheLaw() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.Barrier),
        HoverTipFactory.FromKeyword(PenanceKeywords.Judgement),
        HoverTipFactory.FromKeyword(PenanceKeywords.ThornAura)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;

        await CreatureCmd.TriggerAnim(creature, "Cast", 0.2f);

        // 1. 生命值结算
        int damage = Math.Min(creature.CurrentHp - 10, creature.CurrentHp - 1);

        if (damage > 0)
        {
        #if STS2_BETA
            await CreatureCmd.Damage(choiceContext, creature, damage, ValueProp.Unblockable | ValueProp.Unpowered, this, cardPlay);
        #else
            await CreatureCmd.Damage(choiceContext, creature, damage, ValueProp.Unblockable | ValueProp.Unpowered, this);
        #endif

            await ApplyBarrier(creature, damage * 3);
        }
        else if (damage < 0)
        {
            // 不足 10，直接治疗
            await CreatureCmd.Heal(creature, -damage);
        }

        // 2. 挂载终极能力 (放在最后挂载，防止被这波扣血提前触发)
        await PowerCmd.Apply<TestTheLawPower>(choiceContext, creature, 1, creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}