using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class ConsultTheDossier : PenanceBaseCard
{
    public ConsultTheDossier() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：抽牌数 (2)，每次给的裁决层�?(改为 1)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Consult-Draw", 2m),
        new DynamicVar("Consult-Judge", 1m).WithTooltip("PENANCEMOD-JUDGEMENT")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 字典取值更稳妥和直�?
        int drawAmount = DynamicVars["Consult-Draw"].IntValue;
        int judgeAmountPerCard = DynamicVars["Consult-Judge"].IntValue;

        // 1. 抽牌
        var drawnCards = await CardPileCmd.Draw(choiceContext, drawAmount, Owner);

        if (drawnCards != null)
        {
            // 2. 统计抽到的技能牌数量 (利用 LINQ �?Count)
            int skillCardCount = drawnCards.Count(c => c.Type == CardType.Skill);

            if (skillCardCount > 0)
            {
                // 3. 计算总获得量：技能牌数量 * 1
                int totalJudgement = skillCardCount * judgeAmountPerCard;
                
                await PowerCmd.Apply<JudgementPower>(choiceContext, Owner.Creature, totalJudgement, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级抽牌�?(2 -> 3)
        DynamicVars["Consult-Draw"].UpgradeValueBy(1);
    }
}
