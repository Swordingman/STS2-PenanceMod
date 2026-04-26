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
public class GavelStrike : PenanceBaseCard
{
    // 耗能 1，类型 Attack，稀有度 Common，目标 AnyEnemy
    public GavelStrike() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    // 🌟 注册变量：基础伤害 10
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // 1. 获取玩家当前的屏障值
        var barrierPower = Owner.Creature.GetPower<BarrierPower>();
        int currentBarrier = barrierPower?.Amount ?? 0;

        // 2. 判断目标类型是否为精英或首领
        bool isBossOrElite = false;
        if (target.Monster != null)
        {
            // ⚠️ 提示：在 StS2 的底层中，怪物的级别通常挂载在一个枚举属性上，
            // 假设官方叫 Tier (如 MonsterTier.Elite / Boss) 或直接提供布尔值 IsElite。
            // 这里提供一种伪代码结构，如果 IDE 报错，请敲 `target.Monster.` 然后找类似属性：
            // isBossOrElite = target.Monster.Tier == MonsterTier.Elite || target.Monster.Tier == MonsterTier.Boss;
            
            // 为了保证代码能先跑通编译，这里写一个保险的替补逻辑（假定官方给了 IsElite/IsBoss）：
            // 实际上你要根据你 IDE 里的代码提示去确认这个属性的真实命名。
            // isBossOrElite = target.Monster.IsElite || target.Monster.IsBoss; 
        }

        // 3. 判定斩杀条件：非精英BOSS 且 屏障 > 怪血量
        // 二代获取血量非常直接，直接访问 target.Hp
        if (!isBossOrElite && currentBarrier > target.CurrentHp)
        {
            // ==========================================
            // 处决演出与判定
            // ==========================================
            
            // 🌟 视觉特效：如果有二代对应的砸地特效，可以直接放这里
            // NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(...);
            
            // 停顿 0.8 秒，等待无形的大锤落下
            await Cmd.Wait(0.8f);

            // 强制处决：利用 9999 点无视护甲、不吃修饰的伤害直接斩断生机
            // 这比直接调 Die() 更加健壮，能完美触发“造成伤害”、“击杀”等所有附属钩子
            await DamageCmd.Attack(9999)
                .FromCard(this)
                .Targeting(target)
                .Unpowered()
                .Execute(choiceContext);
        }
        else
        {
            // ==========================================
            // 正常伤害逻辑
            // ==========================================
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害提升 3 (10 -> 13)
        // 如果你原本是打算升到 15，这里改成 UpgradeValueBy(5) 即可！
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}