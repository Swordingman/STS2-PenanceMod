using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class ContinuousRain : PenanceBaseCard
{
    public ContinuousRain() : base(1, CardType.Curse, CardRarity.Curse, TargetType.None, true)
    {
    }

    // ⭐ 完美套用模板：同时注册消耗和狼群诅咒的 UI 关键词（写全路径防报错）
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => 
        [MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];

    // ⭐ 完美套用模板：注入后台逻辑检测用的标签
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    // 顺序注册变量：0 = 伤害, 1 = 给自己的Debuff(2), 2 = 给敌人的Debuff(2)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Unpowered),
        new DynamicVar("Rain-SelfDebuff", 2m),
        new DynamicVar("Rain-EnemyDebuff", 2m)
    ];

    // ==========================================
    // 抽到时触发 (二代最新异步钩子)
    // ==========================================
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        // 必须判断抽到的牌是不是自己，因为这个钩子可能会广播给所有监听者
        if (card == this)
        {
            await Cmd.Wait(0.25f); // 稍微等待抽牌动画，防止逻辑卡死
            await TriggerWolfAutoplay(); 
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        var vars = DynamicVars.Values.ToList();
        int selfDebuff = vars.Count > 1 ? vars[1].IntValue : 2;
        int enemyDebuff = vars.Count > 2 ? vars[2].IntValue : 2;

        // 1. 对玩家造成伤害 (无视防御但受护甲阻挡，吃不到力量加成)
        await CreatureCmd.Damage(choiceContext, creature, DynamicVars.Damage.BaseValue, ValueProp.Unpowered, this);

        // 2. 对所有敌人造成伤害 (同理)
        var aliveEnemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        foreach (var enemy in aliveEnemies)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(enemy)
                .Unpowered() 
                .Execute(choiceContext);
        }

        // 3. 给自己施加易伤和脆弱
        await PowerCmd.Apply<VulnerablePower>(creature, selfDebuff, creature, this);
        await PowerCmd.Apply<FrailPower>(creature, selfDebuff, creature, this);

        // 4. 给所有存活敌人施加易伤和脆弱
        foreach (var enemy in aliveEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(enemy, enemyDebuff, creature, this);
            await PowerCmd.Apply<FrailPower>(enemy, enemyDebuff, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级逻辑：对自己施加的 Debuff 从 2 层减少到 1 层
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 1)
        {
            vars[1].UpgradeValueBy(-1);
        }
    }
}