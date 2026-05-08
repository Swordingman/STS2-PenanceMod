using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // 👈 必须有这个，才能使用 ToList()
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.PenanceModCode.Powers;

public class ThornAuraPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(ThornAuraPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(ThornAuraPower)}.png";

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Owner != null && side == Owner.Side && Amount > 0)
        {
            Flash();

            var enemies = Owner.CombatState.GetOpponentsOf(Owner).ToList();

            VfxCmd.PlayOnSide(CombatSide.Enemy, VfxCmd.giantHorizontalSlashPath, Owner.CombatState);

            await CreatureCmd.Damage(
                choiceContext,        
                enemies,               
                Amount,               
                ValueProp.Unpowered,  
                Owner,                
                null                  
            );

            var lawPower = Owner.GetPower<InTheNameOfTheLawPower>();
            int weakAmount = lawPower?.Amount ?? 0;

            if (weakAmount > 0)
            {
                // 注意：刚刚的荆棘伤害可能已经扎死了某些敌人，所以需要重新过滤出存活的敌人
                var aliveEnemies = enemies.Where(e => !e.IsDead).ToList();
                
                if (aliveEnemies.Count > 0)
                {   
                    foreach (var enemy in aliveEnemies)
                    {
                        await PowerCmd.Apply<WeakPower>(choiceContext, enemy, weakAmount, Owner, null);
                    }
                }
            }
        }
    }
}