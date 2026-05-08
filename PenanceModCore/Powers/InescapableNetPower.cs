using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;

namespace PenanceMod.PenanceModCode.Powers;

public class InescapableNetPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(InescapableNetPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(InescapableNetPower)}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("draw", 0m)
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == player.Creature)
        {
            Flash(); 

            // 1. 获得屏障（读取官方自带的 Amount）
            await PowerCmd.Apply<BarrierPower>(choiceContext, Owner, Amount, Owner, null);

            // 2. 抽牌（读取我们刚刚注册的动态变量 "draw"）
            int currentDraw = DynamicVars["draw"].IntValue;
            if (currentDraw > 0)
            {
                await CardPileCmd.Draw(choiceContext, currentDraw, player);
            }

            await PowerCmd.Remove(this);
        }
    }
}