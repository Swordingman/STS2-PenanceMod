using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using PenanceMod.PenanceModCode.Character;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenanceMod.Scripts.Cards;

[Pool(typeof(PenanceModCardPool))]
public class Upright : PenanceBaseCard
{
    public Upright() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Upright-Str", 1m)
    ];

    private int GetTotalStrengthGain()
    {
        var player = Owner;
        if (player == null) 
            return (int)DynamicVars["Upright-Str"].BaseValue;

        int curseCount = 0;
        
        curseCount += PileType.Hand.GetPile(player).Cards.Count(c => c.Type == CardType.Curse);
        curseCount += PileType.Draw.GetPile(player).Cards.Count(c => c.Type == CardType.Curse);
        curseCount += PileType.Discard.GetPile(player).Cards.Count(c => c.Type == CardType.Curse);

        return (int)DynamicVars["Upright-Str"].BaseValue + curseCount;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner?.Creature;
        if (creature == null) 
            return;

        int totalStr = GetTotalStrengthGain();

        if (totalStr > 0)
        {
            await PowerCmd.Apply<StrengthPower>(creature, totalStr, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}