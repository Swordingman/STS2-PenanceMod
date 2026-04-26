using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class GavelBarrage : PenanceBaseCard
{
    // 耗能填 0 即可，因为下面重写了 HasEnergyCostX，引擎会自动将其视为 X 费。
    // 类型 Attack，稀有度 Uncommon，目标 AnyEnemy
    public GavelBarrage() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    // ⭐ 声明这是 X 费卡
    protected override bool HasEnergyCostX => true;

    // 🌟 注册变量：基础伤害 (3)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 1. 获取 X 的实际值 (引擎自动处理当前能量和《化学X》遗物加成)
        int effectAmount = ResolveEnergyXValue();

        // 2. 如果升级了，基础次数 +1
        if (IsUpgraded)
        {
            effectAmount += 1;
        }

        // 3. 执行多次伤害和 Buff 循环
        if (effectAmount > 0)
        {
            var creature = Owner.Creature;
            for (int i = 0; i < effectAmount; i++)
            {
                // 3.1 造成伤害
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .Targeting(target)
                    .Execute(choiceContext);

                // 3.2 获得 1 层裁决
                // (注意：这里假设你之前的 ApplyJudgement 是一个通用的辅助方法，如果没有写，直接调用 PowerCmd.Apply 即可)
                await PowerCmd.Apply<JudgementPower>(creature, 1, creature, this);

                // 3.3 获得 1 层荆棘环身
                await PowerCmd.Apply<ThornAuraPower>(creature, 1, creature, this);

                // 🌟 给每次连击之间加一点微小的延迟，让打击感更强，动画不至于全部黏在一帧里
                await Cmd.Wait(0.1f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级伤害 (3 -> 4)
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}