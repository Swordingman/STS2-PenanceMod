using BaseLib.Config;

namespace PenanceMod.Scripts.Utils;

public enum VoiceLanguage
{
    CN,
    EN,
    JP,
    KR,
    IT
}

[ConfigHoverTipsByDefault]
public sealed class PenanceConfig : SimpleModConfig
{
    [ConfigHideInUI]
    public static int CurrentSkinIndex { get; set; } = 0;
    
    [ConfigHideInUI]
    public static System.Collections.Generic.List<int> EnabledChallenges { get; set; } = new();

    // ===================================
    // 语音设置 (Voice Settings)
    // ===================================
    [ConfigSection("Voice Settings")] 
    [ConfigHoverTip] 
    public static VoiceLanguage CharacterVoice { get; set; } = VoiceLanguage.CN; 

    // 添加这一行：干员语音开关
    // 因为在同一个 ConfigSection 下，UI 里它们会挨在一起
    [ConfigHoverTip]
    public static bool EnableWolfCurseSpeak { get; set; } = true; 
}