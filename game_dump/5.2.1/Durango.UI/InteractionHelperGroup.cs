using UnityEngine;

namespace Durango.UI;

public class InteractionHelperGroup : InteractionHelperGroupBase
{
	[SerializeField]
	private Transform _buttons;

	private Vector3? _buttonsPos;

	protected override void Start()
	{
		UIManager.FindScript<ToDoListGroupBase>().AddWidthOnChanged(OnChangeTodoWidthRatio);
		base.Start();
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		_buttonsPos = null;
	}

	private void OnChangeTodoWidthRatio(float ratio)
	{
		Vector3? buttonsPos = _buttonsPos;
		if (!buttonsPos.HasValue)
		{
			_buttonsPos = _buttons.localPosition;
		}
		Vector3 value = _buttonsPos.Value;
		value.x -= (1f - ratio) * (float)ToDoListGroupBase.Width;
		_buttons.transform.localPosition = value;
	}
}
