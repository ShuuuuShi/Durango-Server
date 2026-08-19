using UnityEngine;

namespace Durango.UI;

public class ContextActionGroup : ContextActionGroupBase
{
	protected override void Start()
	{
		base.Start();
		_actionButtons.MenuPressed += base.ShowTooltip;
		ToDoListGroupBase toDoListGroupBase = UIManager.FindScript<ToDoListGroupBase>();
		toDoListGroupBase.AddWidthOnChanged(OnChangeTodoWidthRatio);
	}

	private void OnChangeTodoWidthRatio(float ratio)
	{
		if (_baseActionPos == -Vector3.one)
		{
			_baseActionPos = _actionButtons.transform.localPosition;
		}
		Vector3 baseActionPos = _baseActionPos;
		baseActionPos.x -= (1f - ratio) * (float)ToDoListGroupBase.Width;
		_actionButtons.transform.localPosition = baseActionPos;
	}
}
