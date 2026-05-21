using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Silence : PenanceBaseCard
{
    // 耗能 1，类�?Skill，稀有度 Common，目�?Self
    public Silence() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    // 🌟 注册变量：获得的屏障�?(初始 8)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Silence-Barrier", 8m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    // ==========================================
    // 🌟 发光逻辑 (金边提示)
    // ==========================================
    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (!IsInCombat)
                return false;

                return IsHalfHealth(Owner.Creature);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner?.Creature;
        if (creature == null) return;

        var vars = DynamicVars.Values.ToList();
        int barrierAmount = vars.Count > 0 ? vars[0].IntValue : 8;

        // 1. 获得基础屏障 (复用基类快捷方法)
        await ApplyBarrier(creature, barrierAmount);

        // 2. 检查条件：是否低于半血
        if (IsHalfHealth(creature))
        {
            // 稍微停顿一下，让玩家看清金边触发带来的两段独立增益
            await Cmd.Wait(0.15f);

            // 3. 额外获得 3 点裁�?(固定数值，复用基类快捷方法)
            await ApplyJudgement(creature, 3);
        }
    }

    protected override void OnUpgrade()
    {
        // 屏障数值提�?3 (8 -> 11)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(3);
        }
    }
}
