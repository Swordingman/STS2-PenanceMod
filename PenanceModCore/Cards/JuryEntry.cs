using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using PenanceMod.PenanceModCode.Relics;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class JuryEntry : PenanceBaseCard
{
    public JuryEntry() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Jury-Barrier", 14m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player.Creature.CombatState == null) return;

        // 1. 获取所有狼群诅咒的原型列表
        var allCurses = WolfCurseHelper.GetAllWolfCurses();

        // 2. 实例化 3 张不重复的随机卡
        var cardsToChoose = CardFactory.GetDistinctForCombat(
            player,
            allCurses,
            3,
            player.RunState.Rng.CombatCardGeneration
        ).ToList();

        // 3. 检查遗物升级逻辑 (复用我们之前的修正)
        bool shouldUpgrade = false;
        var relic = player.GetRelic<CarnivalMoment>();
        if (relic != null)
        {
            shouldUpgrade = true;
            relic.Flash(); 
        }

        if (shouldUpgrade)
        {
            foreach (var card in cardsToChoose)
            {
                if (card.IsUpgradable && !card.IsUpgraded)
                {
                    card.UpgradeInternal();
                    card.FinalizeUpgradeInternal();
                }
            }
        }

        // 4. 呼出三选一界面
        var chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, cardsToChoose, player, canSkip: false);

        // 5. 🌟 核心修正：洗入抽牌堆并播放官方的预览特效
        if (chosenCard != null)
        {
            // 将生成的卡洗入抽牌堆，并接收返回的添加结果 (CardPileAddResult)
            // 建议用 CardPilePosition.Random 让它真正“洗”进抽牌堆的随机位置
            var addResult = await CardPileCmd.AddGeneratedCardToCombat(
                chosenCard, 
                PileType.Draw, 
                player,
                CardPilePosition.Random
            );

            // 完美复刻官方的视觉展现逻辑
            if (LocalContext.IsMe(player)) // 确保是本地玩家才播放 UI 动画
            {
                // 将单个结果包装成数组
                CardPileAddResult[] statusCards = [addResult];
                
                // 调用官方大屏预览指令
                CardCmd.PreviewCardPileAdd(statusCards, 1.2f, CardPreviewStyle.HorizontalLayout);
                
                // 等待动画播放完毕，防止逻辑跑得比动画快
                await Cmd.Wait(1f); 
            }
        }

        // 6. 获得巨额防御
        await ApplyBarrier(player.Creature, DynamicVars["Jury-Barrier"].IntValue);
    }
    protected override void OnUpgrade()
    {
        DynamicVars["Jury-Barrier"].UpgradeValueBy(3);
    }
}
