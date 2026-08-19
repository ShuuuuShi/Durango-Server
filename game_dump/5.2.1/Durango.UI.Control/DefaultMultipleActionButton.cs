using System;
using System.Collections.Generic;
using Durango.UI.Popup;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI.Control;

public class DefaultMultipleActionButton : SelectableButton
{
	[SerializeField]
	private PresetButton.Style _multipleActionStyle = PresetButton.Style.SolidPopup;

	private PresetButton.Style? _defaultStyle;

	private StringSelector _stringSelector;

	private readonly List<string> _actionList = new List<string>();

	public int Index { get; private set; }

	protected override void OnInit()
	{
		base.OnInit();
		SubClicked = (Action)Delegate.Combine(SubClicked, (Action)delegate
		{
			ShowActionList(_stringSelector == null);
		});
	}

	private PresetButton.Style GetDefaultStyle()
	{
		PresetButton.Style? defaultStyle = _defaultStyle;
		if (!defaultStyle.HasValue)
		{
			_defaultStyle = GetStyle();
		}
		return _defaultStyle.Value;
	}

	public void BeginLoadAction()
	{
		_actionList.Clear();
	}

	public void AddAction(string text)
	{
		_actionList.Add(text);
	}

	public void EndLoadAction(int defaultIndex = 0)
	{
		Index = defaultIndex;
		PresetButton.Style defaultStyle = GetDefaultStyle();
		SetStyle((_actionList.Count <= 1) ? defaultStyle : _multipleActionStyle);
		RefreshButtonText();
	}

	private void ShowActionList(bool show)
	{
		if (show)
		{
			StringSelector stringSelector = UIManager.Popup.Tooltip<StringSelector>();
			stringSelector.Set(_actionList, OnSelectUsableAction);
			stringSelector.MinWidth = base.Widget.width;
			stringSelector.AutoPosition = false;
			stringSelector.AddOnFinished(SelectorHided);
			stringSelector.Show();
			Vector3 pos = stringSelector.transform.parent.InverseTransformPoint(base.transform.TransformPoint(base.Widget.localCorners[1]));
			stringSelector.Widget.SetPosition(pos, 0f, 0f);
			_stringSelector = stringSelector;
		}
		else if (_stringSelector != null)
		{
			_stringSelector.Hide();
			_stringSelector = null;
		}
	}

	private void SelectorHided()
	{
		_stringSelector = null;
		ShowActionList(show: false);
	}

	private void OnSelectUsableAction(int index)
	{
		Index = index;
		RefreshButtonText();
		OnClick();
	}

	private void RefreshButtonText()
	{
		if (Index < _actionList.Count)
		{
			base.Text = _actionList.Get(Index);
		}
	}
}
