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
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Saves.Runs;
using PenanceMod.Scripts.Utils;
using BaseLib.Patches.UI;

namespace PenanceMod.PenanceModCode.Character;

public class PenanceMod : PlaceholderCharacterModel
{
    public const string CharacterId = "PenanceMod";

    public static readonly Color Color = new(144, 119, 22);
    public override Color NameColor => Color;

    // 能量图标轮廓颜色
    public override Color EnergyLabelOutlineColor => new(218f/255f, 165f/255f, 32f/255f, 1f);

    // 人物性别（男女中立）
    public override CharacterGender Gender => CharacterGender.Feminine;

    // 初始血量
    public override int StartingHp => 65;

    // 人物模型tscn路径。要自定义见下。
    public override string CustomVisualPath 
    {
        get 
        {
            return PenanceConfig.CurrentSkinIndex switch
            {
                1 => "res://PenanceMod/scenes/Penance_anim_skin1.tscn", // 偶尔醉陶
                2 => "res://PenanceMod/scenes/Penance_anim_skin2.tscn", // 记叙
                _ => "res://PenanceMod/scenes/Penance_anim_skin0.tscn", // 默认皮肤 (Index 0)
            };
        }
    }

    // 卡牌拖尾场景。
    // public override string CustomTrailPath => "res://scenes/vfx/card_trail_ironclad.tscn";
    // 人物头像路径。
    public override string CustomIconTexturePath => "res://PenanceMod/Icon.svg";
    // 人物头像2号。
    public override string CustomIconPath => "res://PenanceMod/scenes/Penance_Icon.tscn";
    // 能量表盘tscn路径。要自定义见下。
    public override string CustomEnergyCounterPath => "res://PenanceMod/scenes/Penance_eneg.tscn";

    // 篝火休息场景。
    public override string CustomRestSiteAnimPath
    {
        get
        {
            return PenanceConfig.CurrentSkinIndex switch
            {
                1 => "res://PenanceMod/scenes/Penance_rest_site_skin1.tscn", // 偶尔醉陶
                2 => "res://PenanceMod/scenes/Penance_rest_site_skin2.tscn", // 记叙
                _ => "res://PenanceMod/scenes/Penance_rest_site_skin0.tscn", // 默认皮肤
            };
        }
    }
    // 商店人物场景。
    public override string CustomMerchantAnimPath
    {
        get
        {
            return PenanceConfig.CurrentSkinIndex switch
            {
                1 => "res://PenanceMod/scenes/Relaxed_Penance_anim_skin1.tscn", // 偶尔醉陶
                2 => "res://PenanceMod/scenes/Relaxed_Penance_anim_skin2.tscn", // 记叙
                _ => "res://PenanceMod/scenes/Relaxed_Penance_anim_skin0.tscn", // 默认皮肤
            };
        }
    }

    // 多人模式-手指。
    public override string CustomArmPointingTexturePath => "res://PenanceMod/images/charui/hand_point.png";
    // 多人模式剪刀石头布-石头。
    public override string CustomArmRockTexturePath => "res://PenanceMod/images/charui/hand_rock.png";
    // 多人模式剪刀石头布-布。
    public override string CustomArmPaperTexturePath => "res://PenanceMod/images/charui/hand_paper.png";
    // 多人模式剪刀石头布-剪刀。
    public override string CustomArmScissorsTexturePath => "res://PenanceMod/images/charui/hand_scissors.png";
    // 人物选择背景。
    public override string CustomCharacterSelectBg => "res://PenanceMod/scenes/Penance_bg.tscn";
    // 人物选择图标。
    public override string CustomCharacterSelectIconPath => "res://PenanceMod/scenes/Penance_select.png";
    // 人物选择图标-锁定状态。
    public override string CustomCharacterSelectLockedIconPath => "res://PenanceMod/scenes/locked_Penance_select.png";
    // 人物选择过渡动画。
    // public override string CustomCharacterSelectTransitionPath => "res://materials/transitions/ironclad_transition_mat.tres";
    // 地图上的角色标记图标、表情轮盘上的角色头像
    public override string CustomMapMarkerPath => "res://PenanceMod/Icon.svg";
    // 攻击音效
    public override string CustomAttackSfx => "res://PenanceMod/scenes/audio/p_atk_gavel_n.wav";
    // 施法音效
    // public override string CustomCastSfx => null;
    // 死亡音效
    public override string CustomDeathSfx => "res://PenanceMod/scenes/audio/die.wav";

    public override RelicIconData? CustomYummyCookie => new RelicIconData(
        BigIconPath: "res://PenanceMod/images/relics/large/Cookies.png", 
        PackedIconPath: "res://PenanceMod/images/relics/large/Cookies.png",
        PackedIconOutlinePath: "res://PenanceMod/images/relics/large/Cookies.png"
    );

    // 角色选择音效
    public override string CharacterSelectSfx => PenanceConfig.CharacterVoice switch
    {
        VoiceLanguage.EN => "res://PenanceMod/scenes/audio/select_en.wav",
        VoiceLanguage.JP => "res://PenanceMod/scenes/audio/select_jp.wav",
        VoiceLanguage.KR => "res://PenanceMod/scenes/audio/select_kr.wav",
        VoiceLanguage.IT => "res://PenanceMod/scenes/audio/select_it.wav",
        _ => "res://PenanceMod/scenes/audio/select_cn.wav"
    };

    // 过渡音效。这个不能删。
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    public override CardPoolModel CardPool => ModelDb.CardPool<PenanceModCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<PenanceModRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<PenanceModPotionPool>();

    // 初始卡组
    public override IEnumerable<CardModel> StartingDeck
    {
        get
        {
            var customDeck = new List<CardModel>
            {
                ModelDb.Card<Censure>(),
                ModelDb.Card<Resolute>()
            };

            if (PenanceConfig.EnabledChallenges.Contains(5))
            {
                customDeck.AddRange(Enumerable.Repeat(ModelDb.Card<CourtRehearsal>(), 20));
            }
            if (PenanceConfig.EnabledChallenges.Contains(6))
            {
                customDeck.Add(ModelDb.Card<ToothForTooth>());
                customDeck.Add(ModelDb.Card<FamilyArbitration>());
                customDeck.Add(ModelDb.Card<BloodDebtClause>());
                customDeck.Add(ModelDb.Card<SyracusanWolves>());
                customDeck.Add(ModelDb.Card<JuryEntry>());
            }
            if (PenanceConfig.EnabledChallenges.Contains(7))
            {
                customDeck.Add(ModelDb.Card<Quell>());
                customDeck.Add(ModelDb.Card<PendingJudgment>());
                customDeck.Add(ModelDb.Card<Overrule>());
                customDeck.Add(ModelDb.Card<SilenceWrath>());
            }
            if (PenanceConfig.EnabledChallenges.Contains(8))
            {
                customDeck.Add(ModelDb.Card<TheTrial>());
                customDeck.Add(ModelDb.Card<ASip>());
                customDeck.Add(ModelDb.Card<Upright>());
                customDeck.Add(ModelDb.Card<TippingScales>());
                customDeck.Add(ModelDb.Card<SilenceWrath>());
            }
            if (PenanceConfig.EnabledChallenges.Contains(9))
            {
                customDeck.Add(ModelDb.Card<WeightOfLaw>());
                customDeck.Add(ModelDb.Card<FinalVerdict>());
            }
            if (
                !PenanceConfig.EnabledChallenges.Contains(5)
                )
            {
                customDeck.AddRange(Enumerable.Repeat(ModelDb.Card<StrikePenance>(), 5));
                customDeck.AddRange(Enumerable.Repeat(ModelDb.Card<DefendPenance>(), 3));
            }

            return customDeck;
        }
    }

    // 初始遗物
    public override IReadOnlyList<RelicModel> StartingRelics
    {
        get
        {
            var relics = new System.Collections.Generic.List<RelicModel>
            {
                ModelDb.Relic<PenanceBasicRelic>() 
            };
            if (PenanceConfig.EnabledChallenges.Count > 0)
            {
                relics.Add(ModelDb.Relic<ChapterOfPenance>()); 
            }
            if (PenanceConfig.EnabledChallenges.Contains(6))
            {
                relics.Add(ModelDb.Relic<CarnivalMoment>()); 
            }
            if (PenanceConfig.EnabledChallenges.Contains(7))
            {
                relics.Add(ModelDb.Relic<RedMask>()); 
            }
            if (PenanceConfig.EnabledChallenges.Contains(8))
            {
                relics.Add(ModelDb.Relic<Vajra>()); 
            }
            if (PenanceConfig.EnabledChallenges.Contains(9))
            {
                relics.Add(ModelDb.Relic<SiracusanWine>());
                relics.Add(ModelDb.Relic<DragonFruit>());
                relics.Add(ModelDb.Relic<ChosenCheese>());
            }
            return relics;
        }
    }

    // 攻击建筑师的攻击特效列表
    public override List<string> GetArchitectAttackVfx() => [
        VfxCmd.heavyBluntPath,
        VfxCmd.bluntPath,
        VfxCmd.bluntPath,
        VfxCmd.slashPath,
        VfxCmd.heavyBluntPath
    ];
}