using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.Scripts.Utils;
using System.Collections.Generic;

namespace PenanceMod.PenanceModCode.Relics;

[Pool(typeof(PenanceModRelicPool))]
public class ChapterOfPenance : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event; 

    public override string PackedIconPath => $"res://PenanceMod/images/relics/large/{nameof(ChapterOfPenance)}.png";
    protected override string PackedIconOutlinePath => $"res://PenanceMod/images/relics/large/{nameof(ChapterOfPenance)}.png";
    protected override string BigIconPath => $"res://PenanceMod/images/relics/large/{nameof(ChapterOfPenance)}.png";

    // 🌟 修复：改用 string 来完美兼容 STS2 的联机与存档序列化
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public string SavedChallenges { get; set; } = "";

    // 辅助方法 1：用于战斗中判断是否包含某个挑战
    public bool HasChallenge(int challengeId)
    {
        if (string.IsNullOrEmpty(SavedChallenges)) return false;
        
        // 分割字符串并比对
        string[] parts = SavedChallenges.Split(',');
        foreach (string part in parts)
        {
            if (part == challengeId.ToString()) return true;
        }
        return false;
    }

    // 辅助方法 2：用于 UI 界面获取完整的挑战列表
    public List<int> GetChallengeList()
    {
        List<int> list = new List<int>();
        if (string.IsNullOrEmpty(SavedChallenges)) return list;

        string[] parts = SavedChallenges.Split(',');
        foreach (string part in parts)
        {
            if (int.TryParse(part, out int val))
            {
                list.Add(val);
            }
        }
        return list;
    }
}