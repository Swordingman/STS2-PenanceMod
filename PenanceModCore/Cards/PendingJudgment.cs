using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class PendingJudgment : PenanceBaseCard
{
    public PendingJudgment() : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Pending-Weak", 1m),
        new DynamicVar("Pending-Barrier", 4m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        int weakAmt = DynamicVars["Pending-Weak"].IntValue;
        int barrierPerWeak = DynamicVars["Pending-Barrier"].IntValue;

        // 1. 给予所有目标虚弱
        foreach (var enemy in combatState.HittableEnemies)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, weakAmt, Owner.Creature, this);
        }

        // 稍微停顿一下，让虚弱特效飞完，否则引擎可能还没把状态挂上去就判定了
        await Cmd.Wait(0.1f);

        // 2. 统计当前带有虚弱的敌人数量
        int weakEnemyCount = combatState.HittableEnemies.Count(e => e.GetPower<WeakPower>() != null);

        // 3. 获得屏障
        if (weakEnemyCount > 0)
        {
            await ApplyBarrier(Owner.Creature, weakEnemyCount * barrierPerWeak);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Pending-Weak"].UpgradeValueBy(1);
        DynamicVars["Pending-Barrier"].UpgradeValueBy(1);
    }
}