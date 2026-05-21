using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PenanceMod.PenanceModCode.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class SolidDefense : PenanceBaseCard
{
    // 耗能 1，类�?Skill，稀有度 Common，目�?Self
    public SolidDefense() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    // 如果你希望这张牌显示“格挡”相�?hover tip，可以保留这个�?
    // CardModel 里有 GainsBlock 属性，返回 true 后，UI 会把格挡说明加到 hover tips 里�?
    public override bool GainsBlock => true;

    // 注册变量�?
    // [Block]：格挡数值，初始 6
    // [Solid-Barrier]：屏障数值，初始 4
    //
    // BlockVar 的作用：
    // 1. 给描述文本里�?{Block:diff()} 使用�?
    // 2. 支持升级前后数值差异显示；
    // 3. 逻辑里可以用 DynamicVars.Block.BaseValue 读取当前格挡值�?
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(6, ValueProp.Move),
        new DynamicVar("Solid-Barrier", 4m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;

        // 1. 获得格挡
        //
        // CreatureCmd.GainBlock 接口用法�?
        //
        // await CreatureCmd.GainBlock(
        //     creature,     // 谁获得格�?
        //     amount,       // 获得多少格挡
        //     props,        // 格挡属性，一般卡牌给的格挡用 ValueProp.Move
        //     cardPlay,     // 当前这次卡牌打出信息，用�?hook / 历史 / 来源判断
        //     fast          // 可选参数，是否快速播放，默认 false
        // );
        //
        // 返回值是 Task<decimal>，一般表示最终实际获得的格挡量�?
        // 如果你不需要这个返回值，可以直接 await�?
        await CreatureCmd.GainBlock(
            creature,
            DynamicVars.Block.BaseValue,
            ValueProp.Move,
            cardPlay
        );

        // 稍微停顿一下，让格挡和屏障的表现错开�?
        await Cmd.Wait(0.1f);

        // 2. 获得屏障
        //
        // DynamicVars["Solid-Barrier"] 用来读取上面注册�?DynamicVar�?
        // 初始�?4，升级后会变�?6�?
        int barrierAmount = DynamicVars["Solid-Barrier"].IntValue;

        // ApplyBarrier 是你�?PenanceBaseCard 里封装的屏障方法�?
        await ApplyBarrier(creature, barrierAmount);
    }

    protected override void OnUpgrade()
    {
        // 格挡提升 2�? -> 8
        DynamicVars.Block.UpgradeValueBy(2);

        // 屏障提升 2�? -> 6
        DynamicVars["Solid-Barrier"].UpgradeValueBy(1);
    }
}
