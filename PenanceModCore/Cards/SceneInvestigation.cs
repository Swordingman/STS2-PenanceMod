using PenanceMod.PenanceModCode.Character;
using BaseLib.Utils;
using BaseLib.Abstracts;
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
public class SceneInvestigation : PenanceBaseCard
{
    // 耗能 1，类型 Power，稀有度 Rare，目标 Self
    public SceneInvestigation() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    // 🌟 注册基础关键词：升级后获得固有 (Innate)
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded 
        ? [CardKeyword.Innate] 
        : [];

    // 🌟 注册变量：能力牌提供的能量数 (初始 2)
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Scene-Energy", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 给自己挂上 1 层调查现场能力
        // 注意最后一个参数传了 this，这样你的 Power 就可以通过 SourceCard 读取到这张牌的 IsUpgraded 和 DynamicVars，完美解决叠加属性同步问题！
        await PowerCmd.Apply<SceneInvestigationPower>(Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
    }
}