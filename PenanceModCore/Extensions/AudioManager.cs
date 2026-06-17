using Godot;
using System.Threading.Tasks; // 确保引入了 Task

namespace PenanceMod.PenanceModCode.Extensions;

// 1. 类加上 static
public static class AudioManager 
{
    // 2. 方法改为 public static
    public static async Task PlayCustomSfx(string resPath, Node parentNode) 
    {
        var stream = GD.Load<AudioStream>(resPath);
        if (stream == null) 
        {
            GD.PrintErr($"[PenanceMod] 找不到音效文件: {resPath}");
            return;
        }

        var audioPlayer = new AudioStreamPlayer();
        audioPlayer.Stream = stream;
        audioPlayer.VolumeDb = 0; 

        parentNode.AddChild(audioPlayer);
        audioPlayer.Play();

        audioPlayer.Finished += () => 
        {
            audioPlayer.QueueFree(); 
        };
        
        // 消除 async 警告：因为方法体内没有真正的 await 异步操作，但为了签名一致我们可以加一句 Task.CompletedTask
        await Task.CompletedTask; 
    }
}