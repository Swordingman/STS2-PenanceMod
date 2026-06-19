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
using MegaCrit.Sts2.Core.Entities.Cards;
using BaseLib.Utils;    // 引入屏障能力

namespace PenanceMod.Scripts.Potions;

// 注册到斥罪专属的药水池中
[Pool(typeof(PenanceModPotionPool))]
public class BarrierPotion : CustomPotionModel
{
    // 稀有度：普通 (与原版格挡药水一致)
    public override PotionRarity Rarity => PotionRarity.Common;

    // 战斗中专用
    public override PotionUsage Usage => PotionUsage.CombatOnly;

    // 目标类型：自身
    public override TargetType TargetType => TargetType.Self;

    // 定义动态变量：数值设为 12。
    // 使用 PowerVar 能够自动和本地化文本的 {BarrierPower} 绑定
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<BarrierPower>(12)];

    // 添加鼠标悬浮提示：显示屏障能力的 Tooltip
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BarrierPower>()];

    // 药水图片路径 (请确保在对应路径下有贴图文件)
    public override string? CustomPackedImagePath => "res://PenanceMod/images/potions/BarrierPotion.png";
    public override string? CustomPackedOutlinePath => "res://PenanceMod/images/potions/BarrierPotion.png";

    // 打出时的效果逻辑
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        // 【修复 1】：使用字符串索引器来获取泛型 PowerVar 的数值
        int potency = DynamicVars["BarrierPower"].IntValue;

        // 【修复 2】：PowerCmd.Apply 是泛型方法，不传实例传类型。
        // 重载参数依次为：(玩家选择上下文, 目标, 数值, 来源者, 卡牌来源(药水填null))
        await PowerCmd.Apply<BarrierPower>(choiceContext, Owner.Creature, potency, Owner.Creature, null);
    }
}