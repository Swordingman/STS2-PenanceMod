using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System.Linq;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class ArtOfTheHidingFox : PenanceBaseCard
{
    public ArtOfTheHidingFox() : base(1, CardType.Curse, CardRarity.Curse, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    // ⭐ 顺序注册动态变量：索引0是Buff量，索引1是屏障量
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Fox-Magic", 3m),
        new DynamicVar("Fox-Barrier", 25m)
    ];

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this)
        {
            await Cmd.Wait(0.25f);
            await TriggerWolfAutoplay();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        
        // ⭐ 安全取值：按注册顺序提取
        var vars = DynamicVars.Values.ToList();
        int buffAmount = vars.Count > 0 ? vars[0].IntValue : 3;
        int barrierAmount = vars.Count > 1 ? vars[1].IntValue : 25;

        // 1. 获得力量 (假设二代原版依然叫 StrengthPower)
        await PowerCmd.Apply<StrengthPower>(creature, buffAmount, creature, this);
        
        // 2. 获得裁决 (直接调用基类方法)
        await ApplyJudgement(creature, buffAmount);
        
        // 3. 获得荆棘环身 (这里假设你已经写了 ThornAuraPower)
        await PowerCmd.Apply<ThornAuraPower>(creature, buffAmount, creature, this);
        
        // 4. 获得屏障 (直接调用基类方法)
        await ApplyBarrier(creature, barrierAmount);

        // 5. 升级后保留手牌
        if (IsUpgraded)
        {
            await PowerCmd.Apply<RetainHandPower>(creature, 1, creature, this);
        }

        // 6. 强制结束回合
        PlayerCmd.EndTurn(Owner, false);
    }

    protected override void OnUpgrade()
    {
    }
}