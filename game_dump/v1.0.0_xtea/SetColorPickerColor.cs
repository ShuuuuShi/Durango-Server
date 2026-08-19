using System;
using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class SetColorPickerColor : MonoBehaviour
{
	[NonSerialized]
	private UIWidget mWidget;

	public void SetToCurrent()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mWidget == (Object)null)
		{
			mWidget = ((Component)this).GetComponent<UIWidget>();
		}
		if ((Object)(object)UIColorPicker.current != (Object)null)
		{
			mWidget.color = UIColorPicker.current.value;
		}
	}
}
