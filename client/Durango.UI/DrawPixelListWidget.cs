using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class DrawPixelListWidget : UIWidget
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private SelectableWidget _button;

	private ToolDatum _data;

	public SelectableWidget Button => _button;

	public void Set(ToolDatum toggleData, Action clicked)
	{
		_data = toggleData;
		_icon.spriteName = toggleData.IconKey;
		_button.Clicked = clicked;
	}

	public void Clicked()
	{
		if (_button.Clicked != null)
		{
			_button.Clicked();
		}
	}

	public bool SetToggle(ToolDatum selectedTool)
	{
		if (!_data.IsRadioButton)
		{
			return false;
		}
		bool flag = _data.Tool == selectedTool.Tool;
		_button.Selected = flag;
		return flag;
	}

	public void SetSelection(bool isActive)
	{
		_button.Selected = isActive;
	}
}
