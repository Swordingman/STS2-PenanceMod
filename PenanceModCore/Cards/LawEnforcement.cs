using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class LawEnforcement : PenanceBaseCard
{
    // 耗能 2，类型 Attack，稀有度 Uncommon，目标 AnyEnemy
    public LawEnforcement() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册变量：百分比数值 (初始 50)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Law-Percent", 50m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var creature = Owner.Creature;

        // 1. 获取当前屏障与百分比变量
        int barrierAmount = creature.GetPower<BarrierPower>()?.Amount ?? 0;
        
        var vars = DynamicVars.Values.ToList();
        int percent = vars.Count > 0 ? vars[0].IntValue : 50;

        // 2. 计算动态基础伤害 (屏障 * 百分比)
        int baseDamage = (int)(barrierAmount * (percent / 100f));

        // 3. 造成伤害！(这个 baseDamage 传给底层后，引擎会自动让它吃力量、易伤等修饰)
        await DamageCmd.Attack(baseDamage)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 4. 阈值判定：如果基础伤害不足 10，获得其两倍的屏障
        if (baseDamage < 10)
        {
            int barrierGain = baseDamage * 2;
            if (barrierGain > 0)
            {
                // 等待伤害动画演完
                await Cmd.Wait(0.2f);
                await PowerCmd.Apply<BarrierPower>(creature, barrierGain, creature, this);
            }
        }
    }

    // ==========================================
    // 📝 动态 UI：实时显示算出来的面板伤害
    // ==========================================
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        if (IsInCombat)
        {
            int barrierAmount = Owner.Creature.GetPower<BarrierPower>()?.Amount ?? 0;
            var vars = DynamicVars.Values.ToList();
            int percent = vars.Count > 0 ? vars[0].IntValue : 50;

            int currentBaseDamage = (int)(barrierAmount * (percent / 100f));

            // 动态提取语言包中的额外文本并传入参数
            LocString extendedDesc = new LocString("cards", "PENANCEMOD-LAW_ENFORCEMENT.extended_description");
            extendedDesc.Add("amount", currentBaseDamage);
            description.Add("DynamicDamageText", extendedDesc.GetFormattedText());
        }
        else
        {
            description.Add("DynamicDamageText", "");
        }
    }

    protected override void OnUpgrade()
    {
        // 百分比提升 25 (50 -> 75)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(25);
        }
    }
}