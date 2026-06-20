using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.GameInfo.Objects;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class WandAntigravity : PenanceBaseCard
{
    public WandAntigravity() : base(1, CardType.Curse, CardRarity.Curse, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves),
        ..WolfCurseHelper.GetWolfCurseHoverTips(IsUpgraded)
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
        var player = Owner;

        if (player == null || CombatState == null)
            return;

        // 1. 收集所有需要消耗的诅咒
        var allCursesToExhaust = new List<CardModel>();
        var otherPiles = new[] { PileType.Hand, PileType.Draw, PileType.Discard };
        
        foreach (var pileType in otherPiles)
        {
            var pile = pileType.GetPile(player);
            // 找出所有诅咒并直接加入列表（排除自身）
            allCursesToExhaust.AddRange(pile.Cards.Where(c => c.Type == CardType.Curse && c != this));
        }

        // 🌟 核心修改：错峰并发消耗（拉链式消耗）
        if (allCursesToExhaust.Any())
        {
            var exhaustTasks = new List<Task>();
            
            foreach (var curse in allCursesToExhaust)
            {
                // 启动当前这张牌的消耗任务，并存入列表
                exhaustTasks.Add(CardCmd.Exhaust(choiceContext, curse, causedByEthereal: false));
                
                // 极短的错峰延迟 (0.05 秒)
                // 打破同帧叠加，将“一声巨响”变成“机关枪连射”
                await Cmd.Wait(0.05f); 
            }

            // 确保所有的消耗动作都彻底跑完，再往下进行计数和生成
            await Task.WhenAll(exhaustTasks);
        }

        // 2. 统计消耗堆中的目标数量
        var exhaustPile = PileType.Exhaust.GetPile(player);
        int targetCount = 0;

        if (IsUpgraded)
        {
            // 升级后：每有一张牌（不限类型）
            targetCount = exhaustPile.Cards.Count;
        }
        else
        {
            // 升级前：每有一张诅咒
            targetCount = exhaustPile.Cards.Count(c => c.Type == CardType.Curse);
        }

        // 3. 根据数量洗入随机狼群诅咒
        if (targetCount > 0)
        {
            for (int i = 0; i < targetCount; i++)
            {
                var randomCurse = WolfCurseHelper.GetRandomWolfCurse(player, CombatState, IsUpgraded);
                
                var cardNode = await CardPileCmd.AddGeneratedCardToCombat(randomCurse, PileType.Discard, Owner);
                CardCmd.PreviewCardPileAdd(cardNode, 0.5f); 
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级逻辑已经在 OnPlay 中通过 IsUpgraded 实现
    }
}