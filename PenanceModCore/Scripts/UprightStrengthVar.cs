using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System.Linq;

namespace PenanceMod.Scripts.Cards;

public class UprightStrengthVar : DynamicVar
{
    public UprightStrengthVar(string key, decimal baseValue) : base(key, baseValue)
    {
    }

    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        base.UpdateCardPreview(card, previewMode, target, runGlobalHooks);

        PreviewValue = BaseValue;

        if (card.Owner == null)
        {
            return;
        }

        var player = card.Owner;

        try
        {
            int curseCount = 0;

            curseCount += PileType.Hand.GetPile(player).Cards.Count(c => c.Type == CardType.Curse);
            curseCount += PileType.Draw.GetPile(player).Cards.Count(c => c.Type == CardType.Curse);
            curseCount += PileType.Discard.GetPile(player).Cards.Count(c => c.Type == CardType.Curse);
            curseCount += PileType.Exhaust.GetPile(player).Cards.Count(c => c.Type == CardType.Curse);

            PreviewValue = BaseValue + curseCount;
        }
        catch (InvalidOperationException)
        {
            PreviewValue = BaseValue;
        }
    }
}