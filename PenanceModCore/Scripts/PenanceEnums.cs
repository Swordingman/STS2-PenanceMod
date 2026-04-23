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

// 2. 注册关键词 (UI显示用)
public class PenanceKeywords
{
    // 自定义枚举的名字。最终会变成 PENANCE-CURSE_OF_WOLVES
    [CustomEnum("CURSE_OF_WOLVES")]
    // 自动出现在卡牌描述的最前面
    [KeywordProperties(AutoKeywordPosition.Before)] 
    public static CardKeyword CurseOfWolves;
}