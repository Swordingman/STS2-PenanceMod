using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Random;

namespace PenanceMod.Scripts.Characters; // 确保和你的项目命名空间一致

internal partial class PenanceMerchantCharacter : NMerchantCharacter
{
    public override void _Ready()
    {
        // 你的 tscn 里，SpineSprite 正好是第一个子节点，所以 GetChild(0) 完美适用
        var animNode = GetChild(0);
        
        // 你的预览动画写的是 "relaxed_loop"，所以这里填 "relaxed_loop"
        MegaTrackEntry megaTrackEntry = new MegaSprite(animNode).GetAnimationState().SetAnimation("relaxed_loop");
        
        // 随机化起始时间，让动作不那么死板
        if (megaTrackEntry != null)
        {
            megaTrackEntry.SetTrackTime(megaTrackEntry.GetAnimationEnd() * Rng.Chaotic.NextFloat());
        }
    }
}