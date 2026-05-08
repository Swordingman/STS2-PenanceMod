using System.Linq; // 引入 LINQ 以过滤死去的敌人
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace PenanceMod.PenanceModCode.Powers;

public class LawIsCanonPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(LawIsCanonPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(LawIsCanonPower)}.png";

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Owner != null && side == Owner.Side && Amount > 0)
        {
            // 1. 检查屏障
            var barrier = Owner.GetPower<BarrierPower>();
            if (barrier != null && barrier.Amount > 0)
            {
                // 2. 检查玩家身上是否有裁决，如果没有裁决就没必要触发
                var judgement = Owner.GetPower<JudgementPower>();
                if (judgement != null && judgement.Amount > 0)
                {
                    // 3. 获取所有存活的敌人
                    var aliveEnemies = Owner.CombatState.GetOpponentsOf(Owner).Where(e => !e.IsDead).ToList();
                    
                    if (aliveEnemies.Count > 0)
                    {
                        Flash(); // 闪烁本能力图标

                        // 获取战斗随机数生成器
                        var rng = Owner.Player?.RunState.Rng.CombatTargets;

                        // 4. 根据能力层数，决定触发几次
                        for (int i = 0; i < Amount; i++)
                        {
                            var currentAlive = Owner.CombatState.GetOpponentsOf(Owner).Where(e => !e.IsDead).ToList();
                            if (currentAlive.Count == 0) break;

                            // 随机挑选一个幸运儿
                            var target = rng != null ? rng.NextItem(currentAlive) : currentAlive[0];

                            // 🌟 直接调用刚刚公开的异步裁决方法！
                            await judgement.TriggerJudgementDamageAsync(target); 
                            
                            // 延迟 0.15 秒，机枪式裁决！
                            await Cmd.Wait(0.15f);
                        }
                    }
                }
            }
        }
    }
}