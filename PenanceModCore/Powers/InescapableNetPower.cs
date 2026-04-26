using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class InescapableNetPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(InescapableNetPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(InescapableNetPower)}.png";

    // 记录累计的抽牌数
    private int _drawAmount = 0;

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount > 0)
        {
            // 每次获得此状态，抽牌数加 1
            _drawAmount += 1;
        }
        await Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == player.Creature)
        {
            Flash(); 

            // 获得屏障
            await PowerCmd.Apply<BarrierPower>(Owner, Amount, Owner, null);

            // 抽多张牌！
            if (_drawAmount > 0)
            {
                await CardPileCmd.Draw(choiceContext, _drawAmount, player);
            }

            // 移除自身
            await PowerCmd.Remove(this);
        }
    }

    // ==========================================
    // 🌟 修正：完全符合底层 API 的 LocString 用法
    // ==========================================
    // 注意：二代的 PowerModel 通常有一个 Description 属性可以被重写
    public override LocString Description
    {
        get
        {
            // 1. 按照源码的构造函数实例化
            var loc = new LocString("powers", "PENANCEMOD-INESCAPABLE_NET_POWER.description");
            
            // 2. 使用源码暴露的 Add 方法注入变量
            loc.Add("Amount", Amount); 
            loc.Add("draw", _drawAmount); 
            
            return loc;
        }
    }
}