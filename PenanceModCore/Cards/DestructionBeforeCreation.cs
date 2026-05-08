using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class DestructionBeforeCreation : PenanceBaseCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(PenanceKeywords.CurseOfWolves),
        ..WolfCurseHelper.GetWolfCurseHoverTips(IsUpgraded)
    ];

    // 耗能 2，类型 Power，稀有度 Rare，目标 Self
    public DestructionBeforeCreation() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 挂载核心能力
        await PowerCmd.Apply<DestructionBeforeCreationPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级减费 (2 -> 1)
        EnergyCost.UpgradeBy(-1);
    }
}