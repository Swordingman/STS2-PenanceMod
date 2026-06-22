using Godot;
using System.Threading.Tasks;

namespace PenanceMod.PenanceModCode.Extensions;

public static class AudioManager 
{
    // 将 parentNode 设为可选参数 (赋予默认值 null)
    public static async Task PlayCustomSfx(string resPath, Node? parentNode = null) 
    {
        // 核心增强：如果调用时没有传入节点，自动抓取全局根节点
        if (parentNode == null)
        {
            if (Engine.GetMainLoop() is SceneTree sceneTree && sceneTree.Root != null)
            {
                parentNode = sceneTree.Root;
            }
            else
            {
                GD.PrintErr($"[PenanceMod] 无法找到用于播放音频的节点: {resPath}");
                return; // 找不到节点直接终止，防止崩溃
            }
        }

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
        
        await Task.CompletedTask; 
    }
}