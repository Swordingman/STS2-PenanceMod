using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace PenanceMod.PenanceModCode.Character;

public class PenanceModCardPool : CustomCardPoolModel
{
    public override string Title => PenanceMod.CharacterId; //This is not a display name.
    
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://PenanceMod/images/charui/small_orb.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://PenanceMod/images/charui/energy_orb_p.png";


    // 卡池的主题色。
    public override Color DeckEntryCardColor => new(144, 119, 22);

    /*
    public override Texture2D CustomFrame(CustomCardModel card)
    {
        return card.Type switch
        {
            CardType.Attack => PreloadManager.Cache.GetTexture2D("res://PenanceMod/images/charui/bg_attack.png"),
            CardType.Power => PreloadManager.Cache.GetTexture2D("res://PenanceMod/images/charui/bg_power.png"),
            _ => PreloadManager.Cache.GetTexture2D("res://PenanceMod/images/charui/bg_skill.png"),
        };
    }
    */

    // 卡池是否是无色。例如事件、状态等卡池就是无色的。
    public override bool IsColorless => false;
}