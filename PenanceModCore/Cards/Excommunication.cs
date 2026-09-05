using PenanceMod.PenanceModCode.Character;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Excommunication : PenanceBaseCard
{
    public Excommunication() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromKeyword(PenanceKeywords.Barrier)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Excom-Barrier", 3m),
        new DynamicVar("Excom-Str", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;

        if (player == null || CombatState == null)
            return;

        var handCurses = PileType.Hand.GetPile(player)
            .Cards
            .Where(c => c.Type == CardType.Curse)
            .ToList();

        var drawCurses = PileType.Draw.GetPile(player)
            .Cards
            .Where(c => c.Type == CardType.Curse)
            .ToList();

        var discardCurses = PileType.Discard.GetPile(player)
            .Cards
            .Where(c => c.Type == CardType.Curse)
            .ToList();


        var cursesToExhaust = new List<(CardModel card, bool skipVisuals)>();


        foreach (var curse in handCurses)
        {
            cursesToExhaust.Add((curse, false));
        }

        foreach (var curse in drawCurses)
        {
            cursesToExhaust.Add((curse, true));
        }

        foreach (var curse in discardCurses)
        {
            cursesToExhaust.Add((curse, true));
        }


        int curseCount = cursesToExhaust.Count;

        if (curseCount <= 0)
            return;


        // 串行 Exhaust，保证多人同步顺序一致
        for (int i = 0; i < cursesToExhaust.Count; i++)
        {
            var item = cursesToExhaust[i];

            bool skipVisuals = i >= 5;

            await CardCmd.Exhaust(
                choiceContext,
                item.card,
                causedByEthereal: false,
                skipVisuals: skipVisuals
            );

            if (skipVisuals)
            {
                item.card.Pile?.InvokeCardAddFinished();
            }
        }

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", 0.2f);
        await Cmd.Wait(0.2f);


        int totalBarrier = curseCount * DynamicVars["Excom-Barrier"].IntValue;
        int totalStr = curseCount * DynamicVars["Excom-Str"].IntValue;

        await ApplyBarrier(player.Creature, totalBarrier);

        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            player.Creature,
            totalStr,
            player.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);   
    }
}