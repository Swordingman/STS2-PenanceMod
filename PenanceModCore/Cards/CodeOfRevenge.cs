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
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class CodeOfRevenge : PenanceBaseCard
{
    // 耗能 2，类�?Power，稀有度 Rare，目�?Self
    public CodeOfRevenge() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    // 🌟 注册魔法数字：屏障获取量�?3
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("CodeOfRevenge-Magic", 3m).WithTooltip("PENANCEMOD-BARRIER")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抓取第一个变�?(屏障获取�?
        var vars = DynamicVars.Values.ToList();
        int barrierGain = vars.Count > 0 ? vars[0].IntValue : 3;

        // 施加复仇法典能力
        await PowerCmd.Apply<CodeOfRevengePower>(choiceContext,Owner.Creature, barrierGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        this.EnergyCost.UpgradeBy(-1); 
    }
}
