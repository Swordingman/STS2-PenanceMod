using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Collections.Generic;
using System.Threading.Tasks;
using PenanceMod.PenanceModCode.Powers;
using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Adjourn : PenanceBaseCard // 🌟 继承你自己的基类，享受工具方法的便利
{
    public Adjourn() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    // 🌟 核心修复：注册名�?"Magic" 的动态变量，初始值为 2
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 2m).WithTooltip("PENANCEMOD-BARRIER") 
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;
        
        // 1. 安全读取注册�?Magic 变量�?
        int magicValue = DynamicVars.ContainsKey("Magic") ? DynamicVars["Magic"].IntValue : 2;

        // 2. 直接调用基类的方法，取代原本冗长�?LINQ 查询
        int cardsPlayedCount = GetCardsPlayedThisTurn();
        
        // 3. 计算最终数�?
        int effectValue = cardsPlayedCount * magicValue;

        // 4. 调用基类的半血判定方法
        if (IsHalfHealth(creature))
        {
            await CreatureCmd.Heal(creature, effectValue);
        }
        else
        {
            // 调用基类的施加屏障方�?
            await ApplyBarrier(creature, effectValue);
        }

        // 强制结束回合
        PlayerCmd.EndTurn(player, false);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1); 
    }
}
