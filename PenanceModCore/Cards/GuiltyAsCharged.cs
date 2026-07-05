using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class GuiltyAsCharged : PenanceBaseCard
{
    public GuiltyAsCharged() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 完美挂载官方动态伤害三件套
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(15m), 
        new ExtraDamageVar(2m),      
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalculateCurseCountMultiplier)
    ];

    // 🌟 静态诅咒计数器
    private static decimal CalculateCurseCountMultiplier(CardModel card, Creature? target)
    {
        var player = card.Owner;
        if (player == null) return 0m;

        var pilesToSearch = new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust };
        int count = 0;
        
        foreach (var pileType in pilesToSearch)
        {
            var pile = pileType.GetPile(player);
            if (pile != null)
            {
                count += pile.Cards.Count(c => c.Type == CardType.Curse);
            }
        }
        
        return count; 
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        if (target.Block > 0)
        {
            await CreatureCmd.LoseBlock(target, target.Block);
            await Cmd.Wait(0.1f); 
        }

        // 🌟 抓取引擎算好的最终伤害
        int finalDamage = DynamicVars["CalculatedDamage"].IntValue;

        await DamageCmd.Attack(finalDamage)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx(VfxCmd.heavyBluntPath)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 升级基础伤害 (15 -> 20) 和 单张诅咒加成 (2 -> 3)
        DynamicVars.CalculationBase.UpgradeValueBy(5);
        DynamicVars.ExtraDamage.UpgradeValueBy(1);
    }
}