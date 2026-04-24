using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
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
    // 耗能 1，类型 Skill，稀有度 Uncommon，目标 Self
    public ConsultTheDossier() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：索引 0 为抽牌数 (2)，索引 1 为裁决数值 (3)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Consult-Draw", 2m),
        new DynamicVar("Consult-Judge", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int drawAmount = vars.Count > 0 ? vars[0].IntValue : 2;
        int judgeAmount = vars.Count > 1 ? vars[1].IntValue : 3;

        // 1. 抽牌，并直接拿到实际抽上来的这些牌
        var drawnCards = await CardPileCmd.Draw(choiceContext, drawAmount, Owner);

        // 2. 一行代码完成判断：只要抽上来的牌里有任何一张是能力牌
        if (drawnCards != null && drawnCards.Any(c => c.Type == CardType.Power))
        {
            // 3. 获得裁决
            await PowerCmd.Apply<JudgementPower>(Owner.Creature, judgeAmount, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级抽牌数 (2 -> 3)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(1);
        }
    }
}