using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.CardSelection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class WolfFeast : PenanceBaseCard
{
    public WolfFeast() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        // 1. 获取所有狼群诅咒原型
        var allCurses = WolfCurseHelper.GetAllWolfCurses();
        
        // 🌟 修复点 1：传入 player 作为 owner 参数
        var candidates = allCurses.Select(c => combatState.CreateCard(c, player)).ToList();

        // 升级判定
        if (IsUpgraded)
        {
            foreach (var c in candidates.Where(c => c.IsUpgradable))
            {
                c.UpgradeInternal();
                c.FinalizeUpgradeInternal();
            }
        }

        // 2. 构造选牌配置，呼出网格界面
        CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 3);
        var selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, player, prefs);

        if (selectedCards != null && selectedCards.Any())
        {
            // 3. 核心排序逻辑：检测到隐狐之艺，强制排在最后
            var sortedCards = selectedCards
                .OrderBy(c => c.Id.ToString().Contains("ART_OF_THE_HIDING_FOX") ? 1 : 0)
                .ToList();

            // 4. 逐个免费打出
            foreach (var cardToPlay in sortedCards)
            {
                cardToPlay.SetToFreeThisTurn();
                
                // 🌟 修复点 2：使用官方自带的 AutoPlay 指令打出卡牌
                // 参数：上下文，要打的牌，目标(传 null 引擎会自动选)
                await CardCmd.AutoPlay(choiceContext, cardToPlay, null); 
                
                await Cmd.Wait(0.15f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 仅作为升级判定
    }
}