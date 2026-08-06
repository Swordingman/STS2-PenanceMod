using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Castigation : PenanceBaseCard
{
    public Castigation() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
        new DynamicVar("Castigation-Barrier", 4m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    // ==========================================
    // 核心计算逻辑：屏障受力量加成
    // ==========================================
    private int CalculateBarrier()
    {
        // ✅ 解决报错 1：使用 (int) 进行显式强制转换
        int baseBarrier = (int)DynamicVars["Castigation-Barrier"].BaseValue;

        if (CombatState != null && Owner != null && Owner.Creature != null)
        {
            var strength = Owner.Creature.GetPower<StrengthPower>();
            if (strength != null && strength.Amount > 0)
            {
                return baseBarrier + strength.Amount;
            }
        }
        return baseBarrier;
    }

    // ==========================================
    // 动态文本注入 / 完美伪装 diff() 变色效果
    // ==========================================
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        // 获取基础值和当前战斗中的实际值
        int baseBarrier = (int)DynamicVars["Castigation-Barrier"].BaseValue;
        int currentBarrier = IsInCombat ? CalculateBarrier() : baseBarrier;

        // 默认显示白色数字
        string barrierText = currentBarrier.ToString();

        // ✅ 解决报错 2：自己写变色逻辑，绕开底层 API 限制
        if (currentBarrier > baseBarrier)
        {
            barrierText = $"[green]{currentBarrier}[/green]"; // 增益变绿 (使用 Godot 的富文本标签)
        }
        else if (currentBarrier < baseBarrier)
        {
            barrierText = $"[red]{currentBarrier}[/red]"; // 减益变红
        }

        // 将这段带颜色的字符串注入到 JSON 里的 {DynamicBarrierText} 中
        description.Add("DynamicBarrierText", barrierText);
    }

    // ==========================================
    // 打出逻辑
    // ==========================================
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 1. 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);

        // 2. 获得屏障（每次打出时实时计算一次即可）
        int barrierAmount = CalculateBarrier();
        await PowerCmd.Apply<BarrierPower>(choiceContext, Owner.Creature, barrierAmount, Owner.Creature, this);
    }

    // ==========================================
    // 升级逻辑
    // ==========================================
    protected override void OnUpgrade()
    {
        // 伤害提升 (5 -> 7)
        DynamicVars.Damage.UpgradeValueBy(2);

        // 屏障提升 (4 -> 6)，直接通过 Key 获取更稳妥
        DynamicVars["Castigation-Barrier"].UpgradeValueBy(2);
    }
}