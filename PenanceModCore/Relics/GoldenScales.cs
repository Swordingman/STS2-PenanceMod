using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat; // 引入 CombatSide 枚举
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class GoldenScales : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{Id.Entry.ToLowerInvariant()}.png";

    // 注册变量：正当防卫层数 (1)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Scales-Def", 1m)
    ];

    // 加上存档修饰，防止玩家在此回合 S/L 后丢弃记录丢失
    [SavedProperty]
    public bool DiscardedAttackThisTurn { get; set; }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 回合开始时：重置判定变量，并将遗物状态恢复为正常（关闭脉冲发光）
        DiscardedAttackThisTurn = false;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

    // 供你的 Patch 调用的方法（用于“自证其罪”等手动弃牌的监听）
    public void OnAttackDiscarded()
    {
        DiscardedAttackThisTurn = true;
        
        // 修改状态为 Active，引擎底层会自动播放高亮脉冲特效
        Status = RelicStatus.Active; 
    }

    // 🌟 核心修正：使用 BeforeTurnEnd，参数为 PlayerChoiceContext 和 CombatSide
    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        var player = Owner;
        // 判定：仅在己方（玩家侧）回合结束时触发
        if (player == null || side != player.Creature.Side) 
            return;

        bool trigger = DiscardedAttackThisTurn;

        // 如果本回合还没有手动弃置过攻击牌，检查手牌中是否有即将被回合结束机制自动丢弃的攻击牌
        if (!trigger)
        {
            var handPile = PileType.Hand.GetPile(player);
            if (handPile != null)
            {
                trigger = handPile.Cards.Any(c => 
                    c.Type == CardType.Attack && 
                    !c.ShouldRetainThisTurn
                );
            }
        }

        if (trigger)
        {
            Flash();
            int defAmt = DynamicVars["Scales-Def"].IntValue;
            
            // 给予正当防卫
            await PowerCmd.Apply<JustifiedDefensePower>(player.Creature, defAmt, player.Creature, null);
        }
    }
}