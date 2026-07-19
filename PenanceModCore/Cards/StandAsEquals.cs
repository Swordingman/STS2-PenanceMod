// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Cards;
// using MegaCrit.Sts2.Core.Entities.Creatures;
// using MegaCrit.Sts2.Core.GameActions.Multiplayer;
// using MegaCrit.Sts2.Core.Localization.DynamicVars;
// using MegaCrit.Sts2.Core.Models.CardPools;
// using MegaCrit.Sts2.Core.HoverTips;
// using PenanceMod.PenanceModCode.Powers;
// using PenanceMod.PenanceModCode.Character;
// using BaseLib.Abstracts;
// using BaseLib.Utils;

// namespace PenanceMod.Scripts.Cards;

// [Pool(typeof(PenanceModCardPool))]
// public class StandAsEquals : PenanceBaseCard
// {
//     public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

//     public StandAsEquals() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies, true)
//     {
//     }

//     protected override IEnumerable<IHoverTip> ExtraHoverTips => [
//         HoverTipFactory.FromKeyword(PenanceKeywords.Barrier)
//     ];

//     protected override IEnumerable<DynamicVar> CanonicalVars => [
//         new DynamicVar("StandAsEquals-Barrier", 30m)
//     ];

//     protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
//     {
//         var creature = Owner?.Creature;
//         if (creature == null) return;

//         var vars = DynamicVars.Values.ToList();
//         int barrierAmount = vars.Count > 0 ? vars[0].IntValue : 30;

//         // 1. 获取现有屏障层数
//         int previousBarrier = creature.GetPower<BarrierPower>()?.Amount ?? 0;
        
//         // 计算总共可分配的屏障量
//         int totalBarrierToSplit = previousBarrier + barrierAmount;

//         // 2. 调用官方 API：给自己施加本卡牌的基础屏障
//         await PowerCmd.Apply<BarrierPower>(choiceContext, creature, barrierAmount, creature, this);

//         // 3. 获取队友列表，排除自身
//         var teammates = (from c in CombatState.GetTeammatesOf(creature)
//                          where c != null && c.IsAlive && c.IsPlayer && c != creature
//                          select c).ToList();

//         if (teammates.Count > 0 && totalBarrierToSplit > 0)
//         {
//             int splitAmount = totalBarrierToSplit / teammates.Count;

//             // 等待动作队列中的动画播放完毕
//             await Cmd.Wait(0.25f);

//             // 4. 调用官方 API：扣除自身屏障 (通过传入负数抵扣)
//             await PowerCmd.Apply<BarrierPower>(choiceContext, creature, -totalBarrierToSplit, creature, this);

//             // 5. 调用官方 API：将计算好的屏障平均分配给队友
//             foreach (var teammate in teammates)
//             {
//                 await PowerCmd.Apply<BarrierPower>(choiceContext, teammate, splitAmount, creature, this);
//             }
//         }
//     }

//     protected override void OnUpgrade()
//     {
//         var vars = DynamicVars.Values.ToList();
//         if (vars.Count > 0)
//         {
//             vars[0].UpgradeValueBy(5);
//         }
//     }
// }