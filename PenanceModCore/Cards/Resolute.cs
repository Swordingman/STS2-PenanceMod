using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Powers; 
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Resolute : PenanceBaseCard, ITranscendenceCard
{
    public CardModel GetTranscendenceTransformedCard() => ModelDb.Card<Unyield>();

    // 🌟 核心：开�?X 费卡牌标�?
    protected override bool HasEnergyCostX => true;

    // 基础费用�?0，因�?HasEnergyCostX 会接管费用面板显示和消耗逻辑
    public Resolute() : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self, true)
    {
    }

    // 注册变量：X 屏障的单次倍率，初�?3
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Resolute-BarrierX", 3m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        if (creature == null) return;

        // 1. 获得固定�?3 点屏�?
        await ApplyBarrier(creature, 3);
        await Cmd.Wait(0.1f);

        // 2. 获取最终的 X �?(自动计算当前能量并包含化学物X等遗物加�?
        int xValue = ResolveEnergyXValue();

        // 3. 计算 X 相关的屏障和裁决
        int barrierMultiplier = DynamicVars["Resolute-BarrierX"].IntValue;
        int totalXBarrier = xValue * barrierMultiplier;
        
        // 如果未升级，获得 X 点裁决；如果升级，获�?X+1 点裁�?
        int judgeAmt = xValue + (IsUpgraded ? 1 : 0);

        if (totalXBarrier > 0)
        {
            await ApplyBarrier(creature, totalXBarrier);
            await Cmd.Wait(0.1f);
        }

        if (judgeAmt > 0)
        {
            await ApplyJudgement(creature, judgeAmt);
        }
    }

    protected override void OnUpgrade()
    {
        // 屏障倍率提升 1 (3 -> 4)
        DynamicVars["Resolute-BarrierX"].UpgradeValueBy(1);

        // 注意：裁决的 "X -> X+1" 是直接通过 IsUpgraded 判断并在 JSON 里写双文本解决的，不需要在这里额外改变量�?
    }
}
