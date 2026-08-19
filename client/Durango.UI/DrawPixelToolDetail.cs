using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class DrawPixelToolDetail : UIWidget
{
	[SerializeField]
	private UIEventListener _leftButton;

	[SerializeField]
	private UIEventListener _rightButton;

	[SerializeField]
	private DrawPixelStylePreview _preview;

	public void SetStyle([NotNull] ToolDatum data, ColorSelectorWidget colorSelector)
	{
		_preview.ShowPreview(data, colorSelector.CurrentColor);
		SetToolButton(_leftButton, data, -1, colorSelector);
		SetToolButton(_rightButton, data, 1, colorSelector);
	}

	private void SetToolButton(UIEventListener targetButton, ToolDatum data, int offset, ColorSelectorWidget colorSelector)
	{
		if (!data.HasStyle(offset))
		{
			targetButton.gameObject.SetActive(value: false);
			return;
		}
		targetButton.gameObject.SetActive(value: true);
		targetButton.onClick = delegate
		{
			data.TrySwapStyle(offset);
			SetStyle(data, colorSelector);
		};
	}

	public void UpdateColor(ToolDatum tool, Color changedColor)
	{
		_preview.SetColor(tool, changedColor);
	}
}
