using BaseLib.Abstracts;
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

    // 🌟 按顺序注册：索引 0 是伤害，索引 1 是屏障
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
        new DynamicVar("Castigation-Barrier", 4m)
    ];

    // ==========================================
    // 核心计算逻辑：屏障受力量加成
    // ==========================================
    private int CalculateBarrier()
    {
        // 拿到我们在 CanonicalVars 里定义的第二个数 (索引 1)
        var vars = DynamicVars.Values.ToList();
        int baseBarrier = vars.Count > 1 ? vars[1].IntValue : 4;

        if (CombatState != null && Owner != null && Owner.Creature != null)
        {
            // 假设官方的力量能力叫 StrengthPower
            var strength = Owner.Creature.GetPower<StrengthPower>();
            if (strength != null && strength.Amount > 0)
            {
                return baseBarrier + strength.Amount;
            }
        }
        return baseBarrier;
    }

    // ==========================================
    // 动态文本注入
    // ==========================================
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        if (IsInCombat)
        {
            int totalBarrier = CalculateBarrier();
            
            LocString extendedDesc = new LocString("cards", "PENANCEMOD-CASTIGATION.extended_description");
            extendedDesc.Add("amount", totalBarrier); // 把数值传给 JSON 里的变量
            
            // 将格式化后的本地化文本注入
            description.Add("DynamicBarrierText", extendedDesc.GetFormattedText());
        }
        else
        {
            description.Add("DynamicBarrierText", "");
        }
    }

    // ==========================================
    // 打出逻辑
    // ==========================================
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 1. 造成伤害 (引擎会自动把力量加成算在 DynamicVars.Damage.BaseValue 上)
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 2. 获得屏障 (使用我们计算过力量加成的最终数值)
        int barrierAmount = CalculateBarrier();
        await PowerCmd.Apply<BarrierPower>(Owner.Creature, barrierAmount, Owner.Creature, this);
    }

    // ==========================================
    // 升级逻辑
    // ==========================================
    protected override void OnUpgrade()
    {
        // 伤害提升 (5 -> 7)
        DynamicVars.Damage.UpgradeValueBy(2);

        // 屏障提升 (4 -> 6)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 1)
        {
            vars[1].UpgradeValueBy(2);
        }
    }
}