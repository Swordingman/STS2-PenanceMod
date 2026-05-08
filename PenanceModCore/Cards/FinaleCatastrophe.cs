using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.HoverTips;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class FinaleCatastrophe : PenanceBaseCard
{
    // 耗能 1，诅咒，特殊，全场随机目标
    public FinaleCatastrophe() : base(1, CardType.Curse, CardRarity.Curse, TargetType.None, true)
    {
    }

    // 绑定词条与后台标签
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves)
    ];
    // 🌟 注册总伤害次数 (30)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Finale-Hits", 30m)
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
        var vars = DynamicVars.Values.ToList();
        int totalHits = vars.Count > 0 ? vars[0].IntValue : 30;

        var creature = Owner.Creature;
        // 获取一个用于战斗的随机数生成器
        var rng = Owner.RunState.Rng.CombatTargets; 
        
        // 判断升级情况，决定打玩家的概率
        float hitPlayerChance = IsUpgraded ? 0.3f : 0.5f;

        // ==========================================
        // 直接用一个 for 循环秒杀一代的 Action 类！
        // ==========================================
        for (int i = 0; i < totalHits; i++)
        {
            var aliveEnemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
            bool hasPlayer = !creature.IsDead;
            bool hasMonsters = aliveEnemies.Count > 0;

            // 如果场上全空，直接结束循环
            if (!hasPlayer && !hasMonsters) break;

            Creature target = null;

            // 核心概率判定
            if (hasPlayer && hasMonsters)
            {
                // rng.NextFloat() 返回 0.0 到 1.0 的随机数
                if (rng.NextFloat() < hitPlayerChance)
                {
                    target = creature;
                }
                else
                {
                    target = rng.NextItem(aliveEnemies);
                }
            }
            else if (hasPlayer)
            {
                target = creature;
            }
            else
            {
                target = rng.NextItem(aliveEnemies);
            }

            if (target != null)
            {
                await CreatureCmd.Damage(choiceContext, target, 1, ValueProp.Unpowered, this);

                VfxCmd.PlayOnCreatureCenter(target, VfxCmd.slashPath);

                NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);

                await Cmd.Wait(0.05f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 此卡的升级不改变数值，只改变内部概率，概率判定已在 OnPlay 中通过 IsUpgraded 实现
    }
}