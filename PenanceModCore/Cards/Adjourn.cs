using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Combat; 
using System.Linq; 
using PenanceMod.PenanceModCode.Powers;
using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Adjourn : CustomCardModel
{
    // 倍率常量，代替 MagicVar
    private const int EffectMultiplier = 2;

    public Adjourn() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;
        
        // 查询本回合出牌数
        int cardsPlayedCount = CombatManager.Instance.History.CardPlaysStarted
            .Count(entry => 
                entry.RoundNumber == CombatState.RoundNumber && 
                entry.CardPlay.Card.Owner == player);
        
        // 计算最终数值
        int effectValue = cardsPlayedCount * EffectMultiplier;

        // 注意：这里改用了 CurrentHp。如果官方属性名叫 CurrentHealth，请替换一下
        bool isLowHealth = creature.CurrentHp < (creature.MaxHp / 2f);

        if (isLowHealth)
        {
            // 直接 await 方法本身，不需要 .Execute()
            await CreatureCmd.Heal(creature, effectValue);
        }
        else
        {
            // 直接 await 方法本身
            await PowerCmd.Apply<BarrierPower>(creature, effectValue, creature, this);
        }

        PlayerCmd.EndTurn(player, false);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1); 
    }
}