using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using PenanceMod.PenanceModCode.Powers; // 引入你的能力命名空间

namespace PenanceMod.PenanceModCode.UI;

public partial class BarrierBarUI : Control
{
	// 这个 UI 需要盯着谁看？
	public Creature TargetCreature; 
	private Label _label;

	public override void _Ready()
	{
		// 获取里面的数字标签
		_label = GetNode<Label>("Label");
	}

	// Godot 的帧更新神技：每秒执行 60 次
	public override void _Process(double delta)
	{
		if (TargetCreature == null) return;

		// 实时去模型里摸取屏障数据
		var barrier = TargetCreature.GetPower<BarrierPower>();
		
		if (barrier != null && barrier.Amount > 0)
		{
			this.Visible = true; // 有屏障就显示
			_label.Text = barrier.Amount.ToString(); // 实时更新数字
		}
		else
		{
			this.Visible = false; // 屏障碎了就隐藏
		}
	}
}
