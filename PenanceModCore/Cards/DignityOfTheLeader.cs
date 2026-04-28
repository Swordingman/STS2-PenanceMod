using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class DignityOfTheLeader : PenanceBaseCard
{
    // 耗能 1，诅咒，特殊稀有度，目标 None (因为对全场生效且不需要选定)
    public DignityOfTheLeader() : base(1, CardType.Curse, CardRarity.Curse, TargetType.None, true)
    {
    }

    // 绑定消耗和狼群诅咒关键词 (写全路径防报错)
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => 
        [MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];

    // 绑定狼群诅咒后台标签
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    // 🌟 注册变量：获得的能量数 (初始 2)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Dignity-Energy", 2m)
    ];

    // ==========================================
    // 抽到时触发变身 (狼群诅咒模板)
    // ==========================================
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this)
        {
            await Cmd.Wait(0.25f);
            await TriggerWolfAutoplay();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        var vars = DynamicVars.Values.ToList();
        int energyGain = vars.Count > 0 ? vars[0].IntValue : 2;

        // 1. 给自己施加 1 层“止戈”
        // 假设官方底层没有这个能力，这里调用你自己写的 CeasefirePower
        await PowerCmd.Apply<CeasefirePower>(creature, 1, creature, this);

        // 2. 给所有存活的敌人施加 1 层“止戈”
        var aliveEnemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        foreach (var enemy in aliveEnemies)
        {
            await PowerCmd.Apply<CeasefirePower>(enemy, 1, creature, this);
        }

        // 3. 获得能量
        if (energyGain > 0)
        {
            await PlayerCmd.GainEnergy(energyGain, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后能量获取量增加 (2 -> 3)
        var vars = DynamicVars.Values.ToList();
        if (vars.Count > 0)
        {
            vars[0].UpgradeValueBy(1);
        }
    }
}