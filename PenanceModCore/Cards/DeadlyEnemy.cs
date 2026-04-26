using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Players;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class DeadlyEnemy : PenanceBaseCard
{
    // 耗能 1，诅咒，特殊稀有度，目标 None (因为不需要指向打出)
    public DeadlyEnemy() : base(1, CardType.Curse, CardRarity.Curse, TargetType.None, true)
    {
    }

    // 写全路径绑定“消耗”和“保留”词条
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => [
        MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Exhaust,
        MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Retain
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 诅咒打出不生效，直接返回
        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 特殊诅咒通常无法升级
    }
    /// <summary>
    /// 这就是你的 DeadlyEnemyPatch 的二代完美平替。
    /// 只需要在你的 Mod 初始化代码 (Initialize) 中注册这个钩子即可：
    /// Hook.ModifyDamageReceived += DeadlyEnemy.OnModifyDamageReceived;
    /// </summary>
    public static decimal OnModifyDamageReceived(CombatState combatState, Player player, decimal incomingDamage, ValueProp props, AbstractModel damageSource)
    {

        if (incomingDamage > 0)
        {
            int deadlyEnemyCount = PileType.Hand.GetPile(player).Cards.Count(c => c is DeadlyEnemy);

            if (deadlyEnemyCount > 0)
            {
                incomingDamage += deadlyEnemyCount;
            }
        }

        return incomingDamage;
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal damage, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != null && target.IsPlayer && damage > 0)
        {
            if (this.Pile != null && this.Pile.Type == PileType.Hand)
            {
                return 1m; 
            }
        }
        return 0m;
    }
}