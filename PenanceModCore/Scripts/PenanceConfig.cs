using BaseLib.Config;

namespace PenanceMod.Scripts.Utils;

// 定义语音选项的枚举
public enum VoiceLanguage
{
    CN,
    EN,
    JP,
    KR,
    IT
}

[ConfigHoverTipsByDefault] // 开启默认悬浮提示（可选）
public sealed class PenanceConfig : SimpleModConfig
{
    [ConfigHideInUI]
    public static int CurrentSkinIndex { get; set; } = 0;

    // ===================================
    // 下拉菜单配置：语音语言设置
    // ===================================
    [ConfigSection("Voice Settings")] // 会在 UI 中生成一个带标题的折叠区域
    [ConfigHoverTip] // 鼠标悬浮时会显示提示（需要在本地化文件里配置提示文本）
    public static VoiceLanguage CharacterVoice { get; set; } = VoiceLanguage.CN; 
}