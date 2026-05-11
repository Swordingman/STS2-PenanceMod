using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class GuiltyAsCharged : PenanceBaseCard
{
    public GuiltyAsCharged() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册双变量：基础伤害 15，诅咒额外加成 2
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(15, ValueProp.Move),
        new DynamicVar("Guilty-CurseDmg", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var player = Owner;

        // 1. 击碎护甲 (移除所有格挡)
        if (target.Block > 0)
        {
            await CreatureCmd.LoseBlock(target, target.Block);
            await Cmd.Wait(0.1f); // 碎甲稍作停顿，增加打击感
        }

        // 2. 统计所有位置的诅咒牌数量（手牌、抽牌堆、弃牌堆、消耗堆）
        var pilesToSearch = new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust };
        int curseCount = 0;
        
        foreach (var pileType in pilesToSearch)
        {
            var pile = pileType.GetPile(player);
            if (pile != null)
            {
                curseCount += pile.Cards.Count(c => c.Type == CardType.Curse);
            }
        }

        // 3. 计算最终的基础总伤：原面板伤害 + (诅咒数量 * 单张额外伤害)
        int extraDmgPerCurse = DynamicVars["Guilty-CurseDmg"].IntValue;
        int totalBaseDamage = (int)(DynamicVars.Damage.BaseValue + (curseCount * extraDmgPerCurse));

        // 4. 造成伤害 (引擎会拿着这个计算好的 totalBaseDamage 去自动结算力量等增伤 Buff)
        await DamageCmd.Attack(totalBaseDamage)
            .FromCard(this)
            .Targeting(target)
            .WithHitFx(VfxCmd.heavyBluntPath)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 基础伤害提升 5 (15 -> 20)
        DynamicVars.Damage.UpgradeValueBy(5);
        // 诅咒带来的额外伤害提升 1 (2 -> 3)
        DynamicVars["Guilty-CurseDmg"].UpgradeValueBy(1);
    }
}