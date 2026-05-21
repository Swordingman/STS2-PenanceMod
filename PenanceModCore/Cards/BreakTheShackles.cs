using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization; // LocString 命名空间
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System.Linq;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class BreakTheShackles : PenanceBaseCard
{
    private const string PercentKey = "Shackles-Percent";
    private const int BaseThorns = 3;

    public BreakTheShackles() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(PercentKey, 20m).WithTooltip("PENANCEMOD-THORN_AURA") // 初始 20%
    ];

    // ==========================================
    // 核心计算逻辑
    // ==========================================
    private int CalculateTotalAmount()
    {
        int total = BaseThorns;

        // 安全检查：只有在战斗中且玩家存在时，才计算额外层数
        if (Owner != null && Owner.Creature != null && CombatState != null)
        {
            var judgement = Owner.Creature.GetPower<JudgementPower>();
            if (judgement != null && judgement.Amount > 0)
            {
                var percentVar = DynamicVars.Values.First();
                int percent = percentVar.IntValue;
                
                total += (int)(judgement.Amount * (percent / 100f));
            }
        }
        return total;
    }

    // ==========================================
    // 官方神技：动态注入本地化文本参数
    // ==========================================
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        if (IsInCombat)
        {
            int total = CalculateTotalAmount();
            // �?使用 LocString 引用 JSON 中的 extended_description
            LocString extendedDesc = new LocString("cards", "PENANCEMOD-BREAK_THE_SHACKLES.extended_description");
            extendedDesc.Add("amount", total);
            
            description.Add("DynamicTotalText", extendedDesc.GetFormattedText());
        }
        else
        {
            description.Add("DynamicTotalText", "");
        }
    }

    // ==========================================
    // 打出与升�?
    // ==========================================
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int total = CalculateTotalAmount();
        if (total > 0)
        {
            await PowerCmd.Apply<ThornAuraPower>(choiceContext,Owner.Creature, total, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 百分比提�?(20% -> 30%)
        var percentVar = DynamicVars.Values.First();
        percentVar.UpgradeValueBy(10);
    }
}
