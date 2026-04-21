using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace PenanceMod.PenanceModCode.Cards;

// 注意这里是 abstract 抽象类，它自己不会变成一张具体的卡，而是作为模板
public abstract class PenanceBaseCard : CustomCardModel
{
    // 核心魔法在这里：GetType().Name 会自动获取子类的类名。
    // 如果子类叫 Strike，这里就会变成 Strike.png
    // 如果子类叫 Defend，这里就会变成 Defend.png
    public override string PortraitPath => $"res://PenanceMod/images/cards/{GetType().Name}.png";

    // 构造函数，原封不动地把参数传递给底层的 CustomCardModel
    public PenanceBaseCard(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) 
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}