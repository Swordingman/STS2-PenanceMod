using BaseLib.Config;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using PenanceMod.PenanceModCode.Character;
using PenanceMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using PenanceMod.PenanceModCode.Networking;

public partial class SkinSelectorUI : Control
{
    // --- 皮肤选择器相关的节点 ---
    private TextureButton? _leftBtn;
    private TextureButton? _rightBtn;
    private Label? _nameLabel;
    private Node2D? _modelPlaceholder;

    // --- 挑战选项相关的节点 ---
    private Button? _toggleMenuBtn;
    private PanelContainer? _challengeMenuPanel;
    private Label? _challengeTitleLabel;
    private VBoxContainer? _challengesVBox;

    // 当前支持的最大皮肤数量
    private const int MaxSkins = 3;
    private PenanceMod.PenanceModCode.Character.PenanceMod? _currentCharacter;

    public override void _Ready()
    {
        if (IsInsideMultiplayerLoadScreen())
        {
            GetNode<TextureRect>("TextureRect").Visible = true;
            GetNode<Control>("SkinSelectorPanel").Visible = false;
            GetNode<Control>("ChallengeSelectorPanel").Visible = false;
            return;
        }

        // 1. 严格对应 .tscn 的节点树层级获取节点
        _leftBtn = GetNode<TextureButton>("SkinSelectorPanel/ArrowContainer/LeftArrow");
        _rightBtn = GetNode<TextureButton>("SkinSelectorPanel/ArrowContainer/RightArrow");
        _nameLabel = GetNode<Label>("SkinSelectorPanel/SkinName");
        _modelPlaceholder = GetNode<Node2D>("SkinSelectorPanel/SubViewportContainer/SubViewport/ModelInstancePlaceholder");

        // 2. 获取挑战选项相关节点
        _toggleMenuBtn = GetNode<Button>("ChallengeSelectorPanel/ToggleMenuButton");
        _challengeMenuPanel = GetNode<PanelContainer>("ChallengeSelectorPanel/ChallengeMenuPanel");
        _challengeTitleLabel = GetNode<Label>("ChallengeSelectorPanel/ChallengeMenuPanel/VBoxContainer/ChallengeLabel");
        _challengesVBox = GetNode<VBoxContainer>("ChallengeSelectorPanel/ChallengeMenuPanel/VBoxContainer/ScrollContainer/OptionsVBox");

        // 3. 动态加载解包出来的官方界面箭头贴图
        var leftArrowTex = GD.Load<Texture2D>("res://images/packed/common_ui/settings_tiny_left_arrow.png");
        var rightArrowTex = GD.Load<Texture2D>("res://images/packed/common_ui/settings_tiny_right_arrow.png");

        // 4. 给按钮赋予贴图
        if (_leftBtn != null) _leftBtn.TextureNormal = leftArrowTex;
        if (_rightBtn != null) _rightBtn.TextureNormal = rightArrowTex;

        // 5. 绑定左右箭头点击事件
        if (_leftBtn != null) _leftBtn.Pressed += OnLeftPressed;
        if (_rightBtn != null) _rightBtn.Pressed += OnRightPressed;

        // 从官方 ModelDb 获取全局唯一的斥罪角色模型实例
        _currentCharacter = ModelDb.Character<PenanceMod.PenanceModCode.Character.PenanceMod>();

        // 6. 初始化挑战选项 UI
        InitChallengeUI();

        UpdateUI();
    }

    private void InitChallengeUI()
    {
        if (_toggleMenuBtn != null)
            _toggleMenuBtn.Text = new LocString("characters", "PENANCEMOD_CHALLENGE_TOGGLE_BTN").GetFormattedText();

        if (_challengeTitleLabel != null)
            _challengeTitleLabel.Text = new LocString("characters", "PENANCEMOD_CHALLENGE_TITLE").GetFormattedText();

        // 核心改造：动态探测并生成复选框
        if (_challengesVBox != null)
        {
            int index = 1;
            while (true)
            {
                string locKey = $"PENANCEMOD-CHAPTER_OF_PENANCE.challenge.description.{index}";

                // 如果本地化文件里存在这个词条，就生成一个选项
                if (LocString.Exists("relics", locKey))
                {
                    CheckBox cb = new CheckBox();
                    cb.Text = new LocString("relics", locKey).GetFormattedText();

                    // 还原你在 tscn 里设置的字体大小
                    cb.AddThemeFontSizeOverride("font_size", 24);

                    // 读取配置，如果列表里有这个序号，说明被勾选了
                    cb.ButtonPressed = PenanceConfig.EnabledChallenges.Contains(index);

                    // 局部变量捕获（重要！防止委托闭包问题）
                    int challengeId = index;

                    cb.Toggled += (isToggled) =>
                    {
                        if (isToggled && !PenanceConfig.EnabledChallenges.Contains(challengeId))
                            PenanceConfig.EnabledChallenges.Add(challengeId);
                        else if (!isToggled)
                            PenanceConfig.EnabledChallenges.Remove(challengeId);

                        ModConfig.SaveDebounced<PenanceConfig>();
                    };

                    _challengesVBox.AddChild(cb);
                    index++;
                }
                else
                {
                    // 如果连续找不到词条，说明挑战条目读取完毕，跳出循环
                    break;
                }
            }
        }

        if (_toggleMenuBtn != null && _challengeMenuPanel != null)
        {
            _toggleMenuBtn.Pressed += () =>
            {
                _challengeMenuPanel.Visible = !_challengeMenuPanel.Visible;
            };
        }
    }

    private void OnLeftPressed()
    {
        PenanceConfig.CurrentSkinIndex--;

        if (PenanceConfig.CurrentSkinIndex < 0)
            PenanceConfig.CurrentSkinIndex = MaxSkins - 1;

        ModConfig.SaveDebounced<PenanceConfig>();
        UpdateUI();
        LobbySkinNetwork.PublishLocalSkin();
    }

    private void OnRightPressed()
    {
        PenanceConfig.CurrentSkinIndex++;

        if (PenanceConfig.CurrentSkinIndex >= MaxSkins)
            PenanceConfig.CurrentSkinIndex = 0;

        ModConfig.SaveDebounced<PenanceConfig>();
        UpdateUI();
        LobbySkinNetwork.PublishLocalSkin();
    }

    private void PublishSkinSelection()
    {
        var screen = FindCharacterSelectScreen();
        if (screen == null) return;

        LobbySkinNetwork.PublishLocalSkin(screen.Lobby);
    }

    private NCharacterSelectScreen? FindCharacterSelectScreen()
    {
        Node? current = this;

        while (current != null)
        {
            if (current is NCharacterSelectScreen screen)
                return screen;

            current = current.GetParent();
        }

        return null;
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
                    #if STS2_BETA
                    visuals.SpineAnimation.SetAnimation("idle_loop");
                    #else
                    MegaTrackEntry? entry = visuals.SpineAnimation.SetAnimation("idle_loop");
                    if (entry != null)
                    {
                        entry.SetLoop(true);
                        entry.SetTimeScale(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(0.9f, 1.1f));

                        float animationEnd = entry.GetAnimationEnd();
                        if (animationEnd > 0f)
                        {
                            float offset = MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(-0.5f, 0.5f);
                            entry.SetTrackTime((animationEnd + offset) % animationEnd);
                        }
                    }
                    #endif
                    return;
                }
                catch (Exception e)
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
                catch { }
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

    private bool IsInsideMultiplayerLoadScreen()
    {
        Node? current = this;

        while (current != null)
        {
            if (current is NMultiplayerLoadGameScreen)
                return true;

            current = current.GetParent();
        }

        return false;
    }
}