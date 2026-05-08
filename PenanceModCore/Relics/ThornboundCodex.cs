using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.Scripts;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class ThornboundCodex : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override string PackedIconPath => $"res://PenanceMod/images/relics/large/{nameof(ThornboundCodex)}.png";

    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/large/{nameof(ThornboundCodex)}.png";

    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{nameof(ThornboundCodex)}.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        WolfCurseHelper.GetWolfCurseHoverTips(false);

    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        var player = Owner;
        if (player != null)
        {
            player.MaxEnergy += 1;
        }
    }

    public override async Task AfterRemoved()
    {
        await base.AfterRemoved();

        var player = Owner;
        if (player != null)
        {
            player.MaxEnergy -= 1;
        }
    }

    public override async Task BeforeTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side
    )
    {
        var player = Owner;

        if (player == null || player.Creature == null)
            return;

        if (side != player.Creature.Side)
            return;

        var combatState = player.Creature.CombatState;
        if (combatState == null)
            return;

        Flash();

        var curse = WolfCurseHelper.GetRandomWolfCurse(player, combatState);

        CardCmd.PreviewCardPileAdd(
            await CardPileCmd.AddGeneratedCardToCombat(curse, PileType.Draw, player, CardPilePosition.Random), 
            2.2f);
    }
}