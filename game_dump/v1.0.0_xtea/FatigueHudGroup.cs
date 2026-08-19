using UnityEngine;

public class FatigueHudGroup : UIBase
{
	[SerializeField]
	private HudFatigueGauge _fatigueGauge;

	[SerializeField]
	private HudFatigueMomentumControl _fatigueMomentums;

	public HudFatigueGauge FatigueGauge => _fatigueGauge;

	public HudFatigueMomentumControl FatigueMomentums => _fatigueMomentums;

	private void Start()
	{
		ToDoListGroup toDoListGroup = UIManager.FindScript<ToDoListGroup>();
		toDoListGroup.WidthRatioChanged += OnChangeTodoWidthRatio;
	}

	private void OnChangeTodoWidthRatio(float ratio)
	{
		base.Alpha = ratio;
	}
}
