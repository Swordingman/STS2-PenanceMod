using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using PenanceMod.PenanceModCode.Character;
using MegaCrit.Sts2.Core.Entities.Powers;
using PenanceMod.Scripts;
using MegaCrit.Sts2.Core.Entities.Players;

namespace PenanceMod.PenanceModCode.Powers;

public class DestructionBeforeCreationPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(DestructionBeforeCreationPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(DestructionBeforeCreationPower)}.png";

    // 🌟 核心 1：每当消耗诅咒牌时回能
    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool carsedByEthereal)
    {
        // 判定：如果是诅咒牌且拥有者是自己
        if (card.Type == CardType.Curse && Owner != null)
        {
            Flash();
            // 获得 1 点能量（StS2 标准回能命令）
            await PlayerCmd.GainEnergy(Amount, Owner.Player);
            await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
        }
    }

    // 🌟 核心 2：回合开始时洗入随机狼群诅咒
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != null && player.Creature == Owner && Amount > 0)
        {
            Flash();

            // 根据层数（Amount），决定洗入几张。通常是 1 张，但如果玩家打了两张这张牌，就会洗入两张。
            for (int i = 0; i < Amount; i++)
            {
                // 使用你提供的模板生成随机狼群诅咒
                var randomCurse = WolfCurseHelper.GetRandomWolfCurse(player, Owner.CombatState, false);
                
                // 洗入弃牌堆
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(randomCurse, PileType.Discard, player), 1.5f);
            }
        }
    }
}