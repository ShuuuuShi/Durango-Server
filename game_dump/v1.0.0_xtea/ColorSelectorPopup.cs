using System;
using UnityEngine;

public class ColorSelectorPopup : TooltipBase
{
	[SerializeField]
	private ColorSelectorWidget _colorSelector;

	protected override void OnAwake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = Vector3.right * ((float)UIManager.ScreenWidth / 2f - 10f);
	}

	public void Set(Color[] colors, Color currentSelect, Action<int, Color> onSelectColor)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Set(new Color[1][] { colors }, (Color[])(object)new Color[1] { currentSelect }, null, 0, onSelectColor);
	}

	public void Set(Color[][] colors, Color[] currentSelect, string[] tabs, int currentTab, Action<int, Color> onSelectColor)
	{
		_colorSelector.Set(colors, currentSelect, tabs, currentTab, onSelectColor);
	}

	protected override void FillData()
	{
		_colorSelector.FillData();
	}

	protected override void UpdateLayout()
	{
		_colorSelector.UpdateLayout();
	}
}
