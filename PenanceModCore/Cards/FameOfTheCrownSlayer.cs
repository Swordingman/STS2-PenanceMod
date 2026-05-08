using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class FameOfTheCrownSlayer : PenanceBaseCard
{
    // 耗能 1，诅咒，特殊稀有度，目标 None (因为自动对全场判定)
    public FameOfTheCrownSlayer() : base(1, CardType.Curse, CardRarity.Curse, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves)
    ];
    
    // 🌟 注册变量：索引 0 = 抽牌数(2), 索引 1 = 临时力量(3)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Fame-Draw", 2m),
        new DynamicVar("Fame-TempStr", 3m)
    ];

    private bool _autoPlaying;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this)
            return;

        if (_autoPlaying)
            return;

        _autoPlaying = true;

        try
        {
            await TriggerWolfAutoplay(choiceContext, card);
        }
        finally
        {
            _autoPlaying = false;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var vars = DynamicVars.Values.ToList();
        int drawAmt = vars.Count > 0 ? vars[0].IntValue : 2;
        int tempStr = vars.Count > 1 ? vars[1].IntValue : 3;

        // 1. 遍历所有存活敌人，直接无脑发临时力量
        var aliveEnemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        foreach (var enemy in aliveEnemies)
        {
            // 加力量
            await PowerCmd.Apply<FlexPotionPower>(choiceContext,enemy, tempStr, Owner.Creature, this);
        }

        // 2. 抽牌
        await CardPileCmd.Draw(choiceContext, drawAmt, Owner);
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