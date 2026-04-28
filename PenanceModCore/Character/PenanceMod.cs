using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using PenanceMod.PenanceModCode.Cards;
using PenanceMod.PenanceModCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using PenanceMod.Scripts.Cards;

namespace PenanceMod.PenanceModCode.Character;

public class PenanceMod : PlaceholderCharacterModel
{
    public const string CharacterId = "Penance";

    public static readonly Color Color = new(71, 63, 34);
    public override Color NameColor => Color;

    // 能量图标轮廓颜色
    public override Color EnergyLabelOutlineColor => new(218f/255f, 165f/255f, 32f/255f, 1f);

    // 人物性别（男女中立）
    public override CharacterGender Gender => CharacterGender.Feminine;

    // 初始血量
    public override int StartingHp => 80;

    // 人物模型tscn路径。要自定义见下。
    public override string CustomVisualPath => "res://PenanceMod/scenes/Penance_anim.tscn";
    // 卡牌拖尾场景。
    // public override string CustomTrailPath => "res://scenes/vfx/card_trail_ironclad.tscn";
    // 人物头像路径。
    public override string CustomIconTexturePath => "res://icon.svg";
    // 人物头像2号。
    // public override string CustomIconPath => "res://scenes/ui/character_icons/ironclad_icon.tscn";
    // 能量表盘tscn路径。要自定义见下。
    public override string CustomEnergyCounterPath => "res://PenanceMod/scenes/Penance_eneg.tscn";
    // 篝火休息场景。
    public override string CustomRestSiteAnimPath => "res://PenanceMod/scenes/Penance_rest_site.tscn";
    // 商店人物场景。
    public override string CustomMerchantAnimPath => "res://PenanceMod/scenes/Relaxed_Penance_anim.tscn";
    // 多人模式-手指。
    // public override string CustomArmPointingTexturePath => null;
    // 多人模式剪刀石头布-石头。
    // public override string CustomArmRockTexturePath => null;
    // 多人模式剪刀石头布-布。
    // public override string CustomArmPaperTexturePath => null;
    // 多人模式剪刀石头布-剪刀。
    // public override string CustomArmScissorsTexturePath => null;

    // 人物选择背景。
    public override string CustomCharacterSelectBg => "res://PenanceMod/scenes/Penance_bg.tscn";
    // 人物选择图标。
    public override string CustomCharacterSelectIconPath => "res://PenanceMod/scenes/Penance_select.png";
    // 人物选择图标-锁定状态。
    public override string CustomCharacterSelectLockedIconPath => "res://PenanceMod/images/charui/locked_Penance_select.png";
    // 人物选择过渡动画。
    // public override string CustomCharacterSelectTransitionPath => "res://materials/transitions/ironclad_transition_mat.tres";
    // 地图上的角色标记图标、表情轮盘上的角色头像
    // public override string CustomMapMarkerPath => null;
    // 攻击音效
    // public override string CustomAttackSfx => null;
    // 施法音效
    // public override string CustomCastSfx => null;
    // 死亡音效
    // public override string CustomDeathSfx => null;
    // 角色选择音效
    // public override string CharacterSelectSfx => null;
    // 过渡音效。这个不能删。
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    public override CardPoolModel CardPool => ModelDb.CardPool<PenanceModCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<PenanceModRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<PenanceModPotionPool>();

    // 初始卡组
    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<StrikePenance>(),
        ModelDb.Card<StrikePenance>(),
        ModelDb.Card<StrikePenance>(),
        ModelDb.Card<StrikePenance>(),
        ModelDb.Card<StrikePenance>(),
        ModelDb.Card<DefendPenance>(),
        ModelDb.Card<DefendPenance>(),
        ModelDb.Card<DefendPenance>(),
        ModelDb.Card<Censure>(),
        ModelDb.Card<Resolute>()
    ];

    // 初始遗物
    public override IReadOnlyList<RelicModel> StartingRelics => [
        ModelDb.Relic<PenanceBasicRelic>(),
    ];

    // 攻击建筑师的攻击特效列表
    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
    ];
}