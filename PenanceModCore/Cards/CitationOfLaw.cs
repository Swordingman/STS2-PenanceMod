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
public class CitationOfLaw : PenanceBaseCard
{
    // 耗能 1，技能牌，罕见，目标自身
    public CitationOfLaw() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 绑定消耗词条
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    // 🌟 按顺序注册：索引 0 是基础屏障 (5)，索引 1 是百分比 (50)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Citation-Base", 5m),
        new DynamicVar("Citation-Percent", 50m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        var vars = DynamicVars.Values.ToList();
        int baseBarrier = vars[0].IntValue;
        int percent = vars[1].IntValue;

        // 1. 获得基础屏障 (await 确保此处结算完毕)
        await PowerCmd.Apply<BarrierPower>(creature, baseBarrier, creature, this);

        // 2. 获得当前屏障百分比的屏障
        // 此时获取的 Amount 已经是加上基础值之后的结果
        var barrierPower = creature.GetPower<BarrierPower>();
        if (barrierPower != null)
        {
            int currentAmount = barrierPower.Amount;
            int bonusGain = (int)(currentAmount * (percent / 100f));
            
            if (bonusGain > 0)
            {
                await PowerCmd.Apply<BarrierPower>(creature, bonusGain, creature, this);
            }
        }
    }

    // 🌟 动态描述：实时计算打出此牌能获得的总屏障（基础 + 百分比部分）
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        if (IsInCombat)
        {
            var vars = DynamicVars.Values.ToList();
            int baseB = vars[0].IntValue;
            int pct = vars[1].IntValue;

            var barrierPower = Owner.Creature.GetPower<BarrierPower>();
            int current = barrierPower?.Amount ?? 0;
            
            // 计算逻辑还原：(当前 + 基础) * 百分比 + 基础
            int bonus = (int)((current + baseB) * (pct / 100f));
            int total = baseB + bonus;

            LocString extendedDesc = new LocString("cards", "PENANCEMOD-CITATION_OF_LAW.extended_description");
            extendedDesc.Add("amount", total);
            
            description.Add("DynamicTotal", extendedDesc.GetFormattedText());
        }
        else
        {
            description.Add("DynamicTotal", "");
        }
    }

    protected override void OnUpgrade()
    {
        // 升级提升百分比 (50% -> 70%)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 1)
        {
            vars[1].UpgradeValueBy(20);
        }
    }
}