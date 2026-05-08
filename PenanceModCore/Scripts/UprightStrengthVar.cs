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

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        base.UpdateCardPreview(card, previewMode, target, runGlobalHooks);

        if (card.Owner != null)
        {
            var player = card.Owner;
            int curseCount = 0;
            
            // 安全获取各个牌堆的诅咒数量
            curseCount += PileType.Hand.GetPile(player)?.Cards.Count(c => c.Type == CardType.Curse) ?? 0;
            curseCount += PileType.Draw.GetPile(player)?.Cards.Count(c => c.Type == CardType.Curse) ?? 0;
            curseCount += PileType.Discard.GetPile(player)?.Cards.Count(c => c.Type == CardType.Curse) ?? 0;

            // 【关键】：将动态结果赋值给 PreviewValue，让 UI 渲染器去读取它并变绿！
            this.PreviewValue = this.BaseValue + curseCount; 
        }
        else
        {
            this.PreviewValue = this.BaseValue;
        }
    }
}