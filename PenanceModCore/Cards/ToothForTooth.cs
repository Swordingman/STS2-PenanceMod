using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class ToothForTooth : PenanceBaseCard
{
    public ToothForTooth() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("Tooth-Draw", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        WolfCurseHelper.GetWolfCurseHoverTips(IsUpgraded);

    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (!IsInCombat)
                return false;

            return IsHalfHealth(Owner.Creature);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner as Player;
        var creature = player?.Creature;
        if (creature == null || cardPlay.Target == null) return;

        // 1. 失去 2 点生命值
        var selfDamageVar = new DamageVar(2, ValueProp.Unblockable);
        await CreatureCmd.Damage(choiceContext, creature, selfDamageVar, this);
        await Cmd.Wait(0.1f);

        // 2. 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash_heavy")
            .Execute(choiceContext);

        // 3. 向弃牌堆洗入一张随机狼群诅咒
        var randomCurse = WolfCurseHelper.GetRandomWolfCurse(player, false);
        if (randomCurse != null)
        {
            randomCurse.Owner = player;
            await CardPileCmd.Add(randomCurse, PileType.Discard, CardPilePosition.Random);
        }

        // 4. 半血抽牌
        if (IsHalfHealth(creature))
        {
            int drawAmt = DynamicVars["Tooth-Draw"].IntValue;
            await CardPileCmd.Draw(choiceContext, drawAmt, player);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Tooth-Draw"].UpgradeValueBy(1);
    }
}