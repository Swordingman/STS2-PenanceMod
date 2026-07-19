using BaseLib.Extensions;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace PenanceMod.Scripts;

// 1. 注册卡牌标签 (后台逻辑用)
public class PenanceCardTags
{
    [CustomEnum]
    public static CardTag CurseOfWolves;
}

public class PenanceKeywords
{
    // 狼群诅咒
    [CustomEnum("CURSE_OF_WOLVES")]
    [KeywordProperties(AutoKeywordPosition.Before)] 
    public static CardKeyword CurseOfWolves;

    // 屏障
    [CustomEnum("BARRIER")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Barrier;

    // 裁决
    [CustomEnum("JUDGEMENT")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Judgement;

    // 荆棘环身
    [CustomEnum("THORN_AURA")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword ThornAura;

    // 止戈
    [CustomEnum("CEASE_FIRE")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword CeaseFire;

    // 正当防卫
    [CustomEnum("JUSTIFIED_DEFENSE")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword JustifiedDefense;
}