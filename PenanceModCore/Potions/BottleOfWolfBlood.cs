using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards; 
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories; // 引入官方卡牌工厂
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using PenanceMod.PenanceModCode.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.Scripts.Potions;

[Pool(typeof(PenanceModPotionPool))]
public class BottleOfWolfBlood : CustomPotionModel
{
    // 稀有度：稀有
    public override PotionRarity Rarity => PotionRarity.Rare;

    // 战斗中专用
    public override PotionUsage Usage => PotionUsage.CombatOnly;

    // 目标类型：自身
    public override TargetType TargetType => TargetType.Self;

    // 定义动态变量：基础次数为 1。有神圣树皮时 Cards 会变成 2。
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    // 添加鼠标悬浮提示
    public override IEnumerable<IHoverTip> ExtraHoverTips => WolfCurseHelper.GetWolfCurseHoverTips(false);

    // 药水图片路径
    public override string? CustomPackedImagePath => "res://PenanceMod/images/potions/BottleOfWolfBlood.png";
    public override string? CustomPackedOutlinePath => "res://PenanceMod/images/potions/BottleOfWolfBlood.png";

    // 打出时的效果逻辑
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var owner = Owner;
        if (owner?.Creature?.CombatState == null)
            return;

        int amount = DynamicVars.Cards.IntValue;
        var allCurses = WolfCurseHelper.GetAllWolfCurses();

        for (int i = 0; i < amount; i++)
        {
            var cardsToChoose = CardFactory.GetDistinctForCombat(
                owner,
                allCurses,
                3,
                owner.RunState.Rng.CombatCardGeneration
            ).ToList();

            if (HasCarnivalMoment(owner))
            {
                UpgradeWolfCursePreviewCards(cardsToChoose);
            }

            var chosenCard = await CardSelectCmd.FromChooseACardScreen(
                choiceContext,
                cardsToChoose,
                owner,
                canSkip: false
            );

            if (chosenCard != null)
            {
                await CardPileCmd.AddGeneratedCardToCombat(chosenCard, PileType.Hand, owner);
            }
        }
    }

    private static bool HasCarnivalMoment(Player owner)
    {
        return owner.Relics.Any(relic => relic is CarnivalMoment);
    }

    private static void UpgradeWolfCursePreviewCards(IEnumerable<CardModel> cards)
    {
        foreach (var card in cards)
        {
            if (card.Tags.Contains(PenanceCardTags.CurseOfWolves) && card.IsUpgradable)
            {
                card.UpgradeInternal();
                card.FinalizeUpgradeInternal();
            }
        }
    }
}