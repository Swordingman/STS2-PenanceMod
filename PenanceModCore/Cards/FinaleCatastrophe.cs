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

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(CurseCardPool))]
public class FinaleCatastrophe : PenanceBaseCard
{
    // 耗能 1，诅咒，特殊，全场随机目标
    public FinaleCatastrophe() : base(1, CardType.Curse, CardRarity.Curse, TargetType.None, true)
    {
    }

    // 绑定词条与后台标签
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => 
        [MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Exhaust, PenanceKeywords.CurseOfWolves];
    protected override HashSet<CardTag> CanonicalTags => [PenanceCardTags.CurseOfWolves];

    // 🌟 注册总伤害次数 (30)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Finale-Hits", 30m)
    ];

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
                // 造成 1 点不受力量加成的伤害 (完美还原一代的 1 点 Thorns 伤害)
                await CreatureCmd.Damage(choiceContext, target, 1, ValueProp.Unpowered, this);

                // 🌟 等待 0.05 秒，形成子弹连发倾泻的视觉效果！
                // 二代的底层 Damage 会自带极其优秀的受击闪烁动画，不用像一代那样去手动生成 FlashAtkImgEffect。
                await Cmd.Wait(0.05f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 此卡的升级不改变数值，只改变内部概率，概率判定已在 OnPlay 中通过 IsUpgraded 实现
    }
}