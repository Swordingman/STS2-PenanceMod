using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures; // 引入 Creature 以获取 IsPlayer 属性
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System; // 引入 Math.Abs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class HourOfReckoning : PenanceBaseCard
{
    // ==========================================
    // 🌟 卡牌专属记忆体：记录本局战斗损失的血量
    // ==========================================
    private int _hpLostThisCombat = 0;

    // 耗能 2，类型 Attack，稀有度 Uncommon，目标 AnyEnemy
    public HourOfReckoning() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    // 绑定“虚无”词条
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => 
        [MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Ethereal];

    // 注册变量：基础伤害 20
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(20, ValueProp.Move)
    ];

    // ==========================================
    // ⚔️ 核心 Hook 1：战斗开始时，清空记忆
    // 系统会在战斗初始化时自动调用此方法
    // ==========================================
    public override async Task BeforeCombatStart()
    {
        _hpLostThisCombat = 0;
        await Task.CompletedTask;
    }

    // ==========================================
    // 🩸 核心 Hook 2：监听血量变化
    // 只要此牌在你的牌库/手牌/弃牌堆，系统就会自动向它广播掉血事件
    // ==========================================
    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        // delta 是负数代表扣血
        if (creature.IsPlayer && delta < 0)
        {
            // 将扣减的血量取绝对值并累加
            _hpLostThisCombat += (int)Math.Abs(delta);
        }
        await Task.CompletedTask;
    }

    // ==========================================
    // 💥 打出卡牌时的结算
    // ==========================================
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 将基础伤害与卡牌自身记录的已损失血量合并！
        int totalDamage = (int)(DynamicVars.Damage.BaseValue + _hpLostThisCombat);

        // 造成伤害，DamageCmd 会拿这个总值自动去吃力量、易伤等修饰
        await DamageCmd.Attack(totalDamage)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);
    }

    // ==========================================
    // 📝 动态 UI：实时显示已损失血量带来的额外加成
    // ==========================================
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);

        // 如果在战斗中，并且损失了血量，就动态渲染额外文本
        if (IsInCombat && _hpLostThisCombat > 0)
        {
            LocString extendedDesc = new LocString("cards", "PENANCEMOD-HOUR_OF_RECKONING.extended_description");
            extendedDesc.Add("amount", _hpLostThisCombat);
            description.Add("DynamicHpLostText", extendedDesc.GetFormattedText());
        }
        else
        {
            // 否则留空
            description.Add("DynamicHpLostText", "");
        }
    }

    // 升级加 5 伤害
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}