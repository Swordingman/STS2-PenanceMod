using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using PenanceMod.Scripts.Utils;

namespace PenanceMod.PenanceModCode.Networking;

public sealed class SkinSelectionMessage : ICustomMessage
{
    public int SkinIndex { get; private set; }

    public bool ShouldBroadcast => true;
    public bool ShouldBuffer => false;

    public SkinSelectionMessage() { }

    public SkinSelectionMessage(int skinIndex)
    {
        SkinIndex = PenanceSkinState.Normalize(skinIndex);
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteByte((byte)SkinIndex, 2);
    }

    public void Deserialize(PacketReader reader)
    {
        SkinIndex = PenanceSkinState.Normalize(reader.ReadByte(2));
    }

    public void HandleMessage(ulong senderId)
    {
        PenanceSkinState.SetSkin(senderId, SkinIndex);
        GD.Print($"[PenanceMod Skin] 收到玩家皮肤：NetId={senderId}, Skin={SkinIndex}");
    }
}