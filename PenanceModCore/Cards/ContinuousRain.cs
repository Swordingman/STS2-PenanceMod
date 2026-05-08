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
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.HoverTips;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class ContinuousRain : PenanceBaseCard
{
    public ContinuousRain() : base(1, CardType.Curse, CardRarity.Curse, TargetType.None, true)
    {
    }

    // ⭐ 完美套用模板：同时注册消耗和狼群诅咒的 UI 关键词（写全路径防报错）
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves)
    ];
    
    // 顺序注册变量：0 = 伤害, 1 = 给自己的Debuff(2), 2 = 给敌人的Debuff(2)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Unpowered),
        new DynamicVar("Rain-SelfDebuff", 2m),
        new DynamicVar("Rain-EnemyDebuff", 2m)
    ];
    
    private bool _autoPlaying;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this)
            return;

        if (_autoPlaying)
            return;

        _autoPlaying = true;

        try
        {
            await TriggerWolfAutoplay(choiceContext, card);
        }
        finally
        {
            _autoPlaying = false;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var allCreatures = CombatState.Creatures;
        var player = Owner.Creature;
        int selfDebuff = DynamicVars.ContainsKey("Rain-SelfDebuff") ? DynamicVars["Rain-SelfDebuff"].IntValue : 2;
        int enemyDebuff = DynamicVars.ContainsKey("Rain-EnemyDebuff") ? DynamicVars["Rain-EnemyDebuff"].IntValue : 2;

        VfxCmd.PlayFullScreenInCombat(VfxCmd.giantHorizontalSlashPath, player);
        
        await CreatureCmd.Damage(choiceContext, allCreatures, DynamicVars.Damage.BaseValue, ValueProp.Unpowered, player, this);

        await PowerCmd.Apply<VulnerablePower>(choiceContext,player, selfDebuff, player, this);
        await PowerCmd.Apply<WeakPower>(choiceContext,player, selfDebuff, player, this);

        var aliveEnemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        foreach (var enemy in aliveEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext,enemy, enemyDebuff, player, this);
            await PowerCmd.Apply<WeakPower>(choiceContext,enemy, enemyDebuff, player, this);
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