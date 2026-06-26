using BaseLib.Config;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.Scripts.Utils; // 确保引入你的人物主类所在的命名空间

public partial class SkinSelectorUI : Control
{
    private TextureButton? _leftBtn;
    private TextureButton? _rightBtn;
    private Label? _nameLabel;
    private Node2D? _modelPlaceholder;

    // 当前支持的最大皮肤数量
    private const int MaxSkins = 3; 
    private PenanceMod.PenanceModCode.Character.PenanceMod? _currentCharacter;

    public override void _Ready()
    {
        // 1. 严格对应 .tscn 的节点树层级获取节点 (已修改为 ArrowContainer)
        _leftBtn = GetNode<TextureButton>("SkinSelectorPanel/ArrowContainer/LeftArrow");
        _rightBtn = GetNode<TextureButton>("SkinSelectorPanel/ArrowContainer/RightArrow");
        _nameLabel = GetNode<Label>("SkinSelectorPanel/SkinName");
        _modelPlaceholder = GetNode<Node2D>("SkinSelectorPanel/SubViewportContainer/SubViewport/ModelInstancePlaceholder");

        // 2. 动态加载解包出来的官方界面箭头贴图
        var leftArrowTex = GD.Load<Texture2D>("res://images/packed/common_ui/settings_tiny_left_arrow.png");
        var rightArrowTex = GD.Load<Texture2D>("res://images/packed/common_ui/settings_tiny_right_arrow.png");
        
        // 3. 给按钮赋予贴图 (解决箭头不可见的问题)
        if (_leftBtn != null) _leftBtn.TextureNormal = leftArrowTex;
        if (_rightBtn != null) _rightBtn.TextureNormal = rightArrowTex;

        // 4. 绑定左右箭头点击事件
        if (_leftBtn != null) _leftBtn.Pressed += OnLeftPressed;
        if (_rightBtn != null) _rightBtn.Pressed += OnRightPressed;

        // 5. 从官方 ModelDb 获取全局唯一的斥罪角色模型实例
        _currentCharacter = ModelDb.Character<PenanceMod.PenanceModCode.Character.PenanceMod>();
        
        UpdateUI();
    }

    private void OnLeftPressed()
    {
        // 直接修改静态配置变量！
        PenanceConfig.CurrentSkinIndex--;
        if (PenanceConfig.CurrentSkinIndex < 0) 
        {
            PenanceConfig.CurrentSkinIndex = MaxSkins - 1;
        }

        ModConfig.SaveDebounced<PenanceConfig>();
            
        UpdateUI();
    }

    private void OnRightPressed()
    {
        // 直接修改静态配置变量！
        PenanceConfig.CurrentSkinIndex++;
        if (PenanceConfig.CurrentSkinIndex >= MaxSkins) 
        {
            PenanceConfig.CurrentSkinIndex = 0;
        }

        ModConfig.SaveDebounced<PenanceConfig>();
            
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_currentCharacter != null && _nameLabel != null && _modelPlaceholder != null)
        {
            // --- 1. 读取本地化 JSON 里的皮肤名称 ---
            string currentSkinKey = $"PENANCEMOD-PENANCE_MOD.skin{PenanceConfig.CurrentSkinIndex}";
            
            // 假设你在游戏内注册的语言 Table 名字是 "characters"
            if (LocString.Exists("characters", currentSkinKey))
            {
                _nameLabel.Text = new LocString("characters", currentSkinKey).GetFormattedText();
            }
            else
            {
                _nameLabel.Text = $"Missing Loc: {currentSkinKey}";
            }
            
            // --- 2. 销毁 SubViewport 内旧的小人预览模型 ---
            foreach (Node child in _modelPlaceholder.GetChildren())
            {
                _modelPlaceholder.RemoveChild(child);
                child.QueueFree();
            }

            // --- 3. 根据当前皮肤编号，动态载入对应的动画 .tscn ---
            // 此时由于 PenanceConfig 变了，这里获取到的 CustomVisualPath 也会跟着变！
            string modelPath = _currentCharacter.CustomVisualPath;

            if (Godot.FileAccess.FileExists(modelPath))
            {
                PackedScene skinScene = GD.Load<PackedScene>(modelPath);
                if (skinScene != null)
                {
                    Node2D skinInstance = skinScene.Instantiate<Node2D>();
                    _modelPlaceholder.AddChild(skinInstance);
                    skinInstance.Position = Vector2.Zero; // 归零置于 Viewport 中心

                    // --- 4. 召唤地毯式搜索，播放待机动画 ---
                    PlayIdleAnimation(skinInstance);
                }
            }
        }
    }

    // ==========================================
    // 🌟 终极动画播放器：自动遍历节点寻找控制器
    // ==========================================
    private void PlayIdleAnimation(Node rootNode)
    {
        var queue = new System.Collections.Generic.Queue<Node>();
        queue.Enqueue(rootNode);

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();

            // 情况 1: 如果找到了官方的 NCreatureVisuals 节点
            if (current is NCreatureVisuals visuals)
            {
                try
                {
                    MegaTrackEntry? entry = visuals.SpineAnimation.SetAnimation("idle_loop");
                    if (entry != null)
                    {
                        entry.SetLoop(true);
                        entry.SetTimeScale(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(0.9f, 1.1f));
                        float animationEnd = entry.GetAnimationEnd();
                        if (animationEnd > 0f)
                        {
                            entry.SetTrackTime((animationEnd + MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(-0.5f, 0.5f)) % animationEnd);
                        }
                    }
                    return; // 播放成功，直接收工
                }
                catch (System.Exception e)
                {
                    GD.PushWarning($"[PenanceMod UI] NCreatureVisuals 播放报错: {e.Message}");
                }
            }

            // 情况 2: 专门针对原生 Godot SpineSprite 节点
            if (current.GetClass() == "SpineSprite")
            {
                try
                {
                    // 通过 Godot 反射，获取 Spine 的动画状态机并播放
                    var animState = current.Call("get_animation_state").AsGodotObject();
                    if (animState != null)
                    {
                        animState.Call("set_animation", "idle_loop", true, 0);
                        return; // 播放成功，直接收工
                    }
                }
                catch (System.Exception e)
                {
                    GD.PushWarning($"[PenanceMod UI] SpineSprite 播放报错: {e.Message}");
                }
            }

            // 情况 3: 基础的 AnimationPlayer 节点
            if (current is AnimationPlayer animPlayer)
            {
                if (animPlayer.HasAnimation("idle_loop"))
                {
                    animPlayer.Play("idle_loop");
                    return; // 播放成功，直接收工
                }
            }
            
            // 情况 4: 终极兜底方案
            if (current.HasMethod("set_animation"))
            {
                try 
                {
                    current.Call("set_animation", "idle_loop", true, 0);
                    return;
                }
                catch {}
            }

            // 把当前节点的所有子节点加入队列，继续往下找
            foreach (Node child in current.GetChildren())
            {
                queue.Enqueue(child);
            }
        }

        // 如果找遍了所有的子节点都没找到能播放的
        GD.PushWarning("[PenanceMod UI] 地毯式搜索完毕：未能在模型中找到包含 idle_loop 的 NCreatureVisuals、SpineSprite 或 AnimationPlayer 节点。");
    }
}