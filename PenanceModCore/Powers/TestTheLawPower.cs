using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using PenanceMod.PenanceModCode.Relics;

namespace PenanceMod.PenanceModCode.Powers;

public class TestTheLawPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None; // 唯一神权，不可叠层
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(TestTheLawPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(TestTheLawPower)}.png";

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        int amount = result.UnblockedDamage;
        
        if (target == Owner && amount > 0)
        {
            // 🌟 终极判定：伤害源必须是卡牌 + 卡牌拥有者是玩家 + 伤害不可格挡 (Unblockable 自残伤害)
            if (cardSource != null && cardSource.Owner == Owner.Player && props.HasFlag(ValueProp.Unblockable))
            {
                int dmg = amount;
                Flash();

                // 1. 获得等量治疗
                await CreatureCmd.Heal(Owner, dmg);

                // 2. 获得 3 倍屏障
                int barrierAmt = dmg * 3;
                await PowerCmd.Apply<BarrierPower>(choiceContext, Owner, barrierAmt, Owner, null);

                // 3. 将等量 (3倍) 屏障存入遗物 (双重检测兼容升级版)
                var bossRelic = Owner.Player.GetRelic<ThornyRoad>();
                if (bossRelic != null) 
                {
                    bossRelic.AddStoredHeal(barrierAmt);
                }
                else
                {
                    var basicRelic = Owner.Player.GetRelic<PenanceBasicRelic>();
                    if (basicRelic != null) basicRelic.AddStoredHeal(barrierAmt);
                }

                // 4. 获得 3 裁决和 3 荆棘环身
                await PowerCmd.Apply<JudgementPower>(choiceContext, Owner, 3, Owner, null);
                await PowerCmd.Apply<ThornAuraPower>(choiceContext, Owner, 3, Owner, null);
            }
        }
    }
}