using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.PenanceModCode.Powers; 
using System.Linq;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Asceticism : PenanceBaseCard
{
    // 定义动态变量键�?
    private const string MagicKey = "Asceticism-Magic";

    // 耗能 1，类型为 Power，稀有度 Rare，目�?Self
    public Asceticism() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    // 基础荆棘获得量为 2
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(MagicKey, 2m).WithTooltip("PENANCEMOD-THORN_AURA")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        
        // 抓取第一个动态变�?
        var magicVar = DynamicVars.Values.First();
        int buffAmount = magicVar.IntValue;

        // 施加苦行能力
        await PowerCmd.Apply<AsceticismPower>(choiceContext,creature, buffAmount, creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级后获得量增加 (2 -> 3)
        var magicVar = DynamicVars.Values.First();
        magicVar.UpgradeValueBy(1); 
    }
}
