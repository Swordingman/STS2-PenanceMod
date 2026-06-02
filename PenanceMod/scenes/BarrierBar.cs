using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using PenanceMod.PenanceModCode.Powers;

namespace PenanceMod.PenanceModCode.UI;

public partial class BarrierBar : Control
{
    public Creature TargetCreature;

    private Control _barContainer;
    private Label _label;
    private TextureRect _icon;

    private int _lastAmount = -1;
    private bool _lastVisible;

    private float _fullBarWidth = 120f;

    public override void _Ready()
    {
        _icon = GetNodeOrNull<TextureRect>("HBoxContainer/Icon");
        _barContainer = GetNodeOrNull<Control>("HBoxContainer/BarContainer");
        _label = GetNodeOrNull<Label>("HBoxContainer/BarContainer/Label");

        if (_barContainer != null)
        {
            // 绑定 Godot 的原生绘制事件
            _barContainer.Draw += OnBarDraw;
            _fullBarWidth = _barContainer.CustomMinimumSize.X;
        }

        MouseFilter = MouseFilterEnum.Ignore;
        Visible = false;
        ZIndex = 1000;

        // 【位置修正】向左偏移一半宽度 (-74) 使其居中，向上移动 (-25) 悬浮于血条上方
        Position = new Vector2(-134f, -25f);

        GD.Print("[PenanceMod] BarrierBar ready. Parent=", GetParent()?.Name);
    }

    public override void _Process(double delta)
    {
        if (TargetCreature == null)
        {
            SetDisplay(false, 0);
            return;
        }

        var barrier = TargetCreature.GetPower<BarrierPower>();
        int amount = barrier?.Amount ?? 0;

        bool shouldShow = amount > 0;

        if (_lastAmount == amount && _lastVisible == shouldShow)
            return;

        SetDisplay(shouldShow, amount);
    }

    private void SetDisplay(bool shouldShow, int amount)
    {
        _lastVisible = shouldShow;
        _lastAmount = amount;
        Visible = shouldShow;

        if (_label != null)
        {
            _label.Text = amount.ToString();
        }

        // 数值变化时，通知 Godot 重新绘制进度条形状
        if (_barContainer != null && TargetCreature != null)
        {
            _barContainer.QueueRedraw();
        }
    }

    // ========== 核心绘图逻辑 ==========
    private void OnBarDraw()
    {
        if (_barContainer == null) return;

        float h = _barContainer.Size.Y; // 当前高度 (12px)
        float w = _fullBarWidth;        // 总宽度 (120px)
        float tip = h / 2f;             // 三角形尖端的水平长度

        // 1. 绘制青灰色的背景槽
        Color bgColor = new Color(0.2f, 0.25f, 0.3f, 0.8f);
        Vector2[] bgPoly = GetHexPolygon(w, h, tip);
        _barContainer.DrawColoredPolygon(bgPoly, bgColor);

        // 2. 绘制你指定的颜色 (144, 119, 22) 的动态进度条
        if (TargetCreature != null && TargetCreature.MaxHp > 0)
        {
            float ratio = Mathf.Min((float)_lastAmount / TargetCreature.MaxHp, 1f);
            float fillW = w * ratio;

            if (fillW > 0)
            {
                // RGB 转为 0-1 的比例
                Color fillColor = new Color(144f / 255f, 119f / 255f, 22f / 255f, 1f);
                Vector2[] fillPoly = GetFillPolygon(fillW, h, tip, w);
                _barContainer.DrawColoredPolygon(fillPoly, fillColor);
            }
        }
    }

    // 生成完整两端尖角的六边形顶点
    private Vector2[] GetHexPolygon(float w, float h, float tip)
    {
        return new Vector2[] {
            new Vector2(0, h / 2f),
            new Vector2(tip, 0),
            new Vector2(w - tip, 0),
            new Vector2(w, h / 2f),
            new Vector2(w - tip, h),
            new Vector2(tip, h)
        };
    }

    // 根据当前宽度，动态切割多边形（防止半满时右侧形状错乱）
    private Vector2[] GetFillPolygon(float w, float h, float tip, float fullW)
    {
        if (w <= tip) 
        {
            // 如果很短，只画左边的半个三角形
            float yOffset = (w / tip) * (h / 2f);
            return new Vector2[] {
                new Vector2(0, h / 2f),
                new Vector2(w, h / 2f - yOffset),
                new Vector2(w, h / 2f + yOffset)
            };
        }
        else if (w <= fullW - tip) 
        {
            // 常规情况：左边尖角 + 中间矩形
            return new Vector2[] {
                new Vector2(0, h / 2f),
                new Vector2(tip, 0),
                new Vector2(w, 0),
                new Vector2(w, h),
                new Vector2(tip, h)
            };
        }
        else 
        {
            // 快满的情况：左边尖角 + 中间矩形 + 右边被切断的半个尖角
            float remaining = w - (fullW - tip);
            float yOffset = (remaining / tip) * (h / 2f);
            return new Vector2[] {
                new Vector2(0, h / 2f),
                new Vector2(tip, 0),
                new Vector2(fullW - tip, 0),
                new Vector2(w, yOffset),
                new Vector2(w, h - yOffset),
                new Vector2(fullW - tip, h),
                new Vector2(tip, h)
            };
        }
    }
}