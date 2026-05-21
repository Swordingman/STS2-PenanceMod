using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories; // 🌟 引入官方卡牌工厂
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;

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

        // 2. 完美复刻官方《发现》：使用 CardFactory 实例�?3 张不重复的随机卡
        var cardsToChoose = CardFactory.GetDistinctForCombat(
            player,
            allCurses,
            3,
            player.RunState.Rng.CombatCardGeneration
        ).ToList();

        // 3. 呼出三选一界面
        var chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, cardsToChoose, player, canSkip: false);

        // 4. 将选中的卡洗入抽牌�?
        if (chosenCard != null)
        {
            await CardPileCmd.AddGeneratedCardToCombat(chosenCard, PileType.Draw, player);
        }

        // 5. 获得巨额防御
        await ApplyBarrier(player.Creature, DynamicVars["Jury-Barrier"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Jury-Barrier"].UpgradeValueBy(3);
    }
}
