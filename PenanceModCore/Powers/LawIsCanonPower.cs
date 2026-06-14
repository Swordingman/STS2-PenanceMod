using System.Linq; // 引入 LINQ 以过滤死去的敌人
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace PenanceMod.PenanceModCode.Powers;

public class LawIsCanonPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override string? CustomPackedIconPath => $"res://PenanceMod/images/powers/{nameof(LawIsCanonPower)}.png";
    public override string? CustomBigIconPath => $"res://PenanceMod/images/powers/large/{nameof(LawIsCanonPower)}.png";

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        var owner = Owner;
        if (owner == null || side != owner.Side || Amount <= 0)
            return;

        var combatState = owner.CombatState;
        if (combatState == null)
            return;

        var barrier = owner.GetPower<BarrierPower>();
        if (barrier == null || barrier.Amount <= 0)
            return;

        var judgement = owner.GetPower<JudgementPower>();
        if (judgement == null || judgement.Amount <= 0)
            return;

        var aliveEnemies = combatState
            .GetOpponentsOf(owner)
            .Where(e => e != null && !e.IsDead)
            .ToList();

        if (aliveEnemies.Count == 0)
            return;

        Flash();

        var rng = owner.Player?.RunState.Rng.CombatTargets;

        for (int i = 0; i < Amount; i++)
        {
            var currentAlive = combatState.GetOpponentsOf(owner)
            .Where(e => e != null && !e.IsDead)
            .ToList();

            if (currentAlive.Count == 0)
                break;

            var target = rng != null? rng.NextItem(currentAlive) : currentAlive[0];

            if (target == null)
                continue;

            await judgement.TriggerJudgementDamageAsync(target, choiceContext);
            await Cmd.Wait(0.15f);
        }
    }
}