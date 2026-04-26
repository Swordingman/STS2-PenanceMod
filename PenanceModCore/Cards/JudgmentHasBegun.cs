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
public class JudgmentHasBegun : PenanceBaseCard
{
    // 耗能 1，类型 Skill，稀有度 Rare，目标 Self
    public JudgmentHasBegun() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    // 🌟 注册基础关键词：消耗
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded 
        ? [CardKeyword.Exhaust, CardKeyword.Retain] // 升级后增加保留 (Retain)
        : [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 挂载核心 Power
        await PowerCmd.Apply<JudgmentHasBegunPower>(Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
    }
}