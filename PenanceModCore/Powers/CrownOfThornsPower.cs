using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.ValueProps;

namespace PenanceMod.PenanceModCode.Powers;

public class CrownOfThornsPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 大图路径已按照你的新模板修改
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(CrownOfThornsPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(CrownOfThornsPower)}.png";

    // 对应 StS1 里的 HP_LOSS 常量
    private const int HpLossAmount = 2;

    // 完美替代 atStartOfTurnPostDraw 
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 确保只有当拥有者是当前回合的玩家时才触发
        if (Owner == player.Creature)
        {
            Flash(); // 闪烁图标

            await CreatureCmd.Damage(
                choiceContext, 
                Owner,                                        // 目标：自己
                HpLossAmount,                                 // 数值：2
                ValueProp.Unblockable | ValueProp.Unpowered,  // 标签：无视格挡 + 非攻击（纯生命流失）
                null,                                         // 伤害来源传 null，表示属于环境/Buff伤害
                null                                          // 卡牌来源传 null
            );

            // 2. 获得荆棘环身
            await PowerCmd.Apply<ThornAuraPower>(Owner, Amount, Owner, null);
        }
    }
}