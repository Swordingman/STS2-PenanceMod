using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using System.Collections.Generic;
using System.Threading.Tasks;
using PenanceMod.PenanceModCode.Character; // 引入药水池
using PenanceMod.PenanceModCode.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;    // 引入裁决能力

namespace PenanceMod.Scripts.Potions;

// 注册到斥罪专属的药水池中
[Pool(typeof(PenanceModPotionPool))]
public class JudgementPotion : CustomPotionModel
{
    // 稀有度：罕见 (根据你的注释，设定为 Uncommon)
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    // 战斗中专用
    public override PotionUsage Usage => PotionUsage.CombatOnly;

    // 目标类型：自身
    public override TargetType TargetType => TargetType.Self;

    // 定义动态变量：基础数值设为 5
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<JudgementPower>(5)];

    // 添加鼠标悬浮提示：显示裁决能力的 Tooltip
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<JudgementPower>()];

    // 药水图片路径
    public override string? CustomPackedImagePath => "res://PenanceMod/images/potions/JudgementPotion.png";
    public override string? CustomPackedOutlinePath => "res://PenanceMod/images/potions/JudgementPotion.png";

    // 打出时的效果逻辑
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        // 提取 CanonicalVars 中定义的裁决层数 (默认是 5)
        int potency = DynamicVars["JudgementPower"].IntValue;

        // 执行动作：给予玩家裁决能力
        // 参数依次为：(玩家选择上下文, 目标, 层数, 来源者, 卡牌来源(药水填null))
        await PowerCmd.Apply<JudgementPower>(choiceContext, Owner.Creature, potency, Owner.Creature, null);
    }
}