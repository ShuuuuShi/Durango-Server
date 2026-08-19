using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class KeyValueTooltip : TooltipBase
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _commentWidget;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private UIWidget _spaceWidget;

	[SerializeField]
	private KeyValueLabel _keyValueLabel;

	private ListObjectPool<UIWidget> _spaceWidgets;

	private ListObjectPool<KeyValueLabel> _keyValueLabels;

	private readonly List<UIWidget> _widgets = new List<UIWidget>();

	private const int LabelMargin = 10;

	protected override void Start()
	{
		base.Start();
		_spaceWidgets = new ListObjectPool<UIWidget>();
		_spaceWidgets.BaseObject = _spaceWidget;
		_spaceWidgets.UseBase = true;
		_keyValueLabels = new ListObjectPool<KeyValueLabel>();
		_keyValueLabels.BaseObject = _keyValueLabel;
		_keyValueLabels.UseBase = true;
	}

	protected override void OnHide()
	{
		_spaceWidgets.Clear();
		_keyValueLabels.Clear();
		_widgets.Clear();
	}

	public void Set(string title, string comment, IEnumerable<KeyValuePair<string, string>> keyValuePairs, int width)
	{
		base.Widget.width = width;
		_spaceWidgets.BeginLoad();
		_titleLabel.text = title;
		_titleWidget.height = (int)_titleLabel.printedSize.y + 20;
		_widgets.Add(_titleWidget);
		if (string.IsNullOrEmpty(comment))
		{
			_commentLabel.gameObject.SetActive(value: false);
		}
		else
		{
			AddSpaceWidget(10);
			_commentLabel.gameObject.SetActive(value: true);
			_commentLabel.text = comment;
			_commentWidget.height = (int)_commentLabel.printedSize.y;
			_widgets.Add(_commentWidget);
			AddSpaceWidget(10);
		}
		AddSpaceWidget(10);
		_keyValueLabels.BeginLoad();
		if (keyValuePairs != null)
		{
			foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
			{
				KeyValueLabel next = _keyValueLabels.GetNext();
				next.Set(keyValuePair.Key, keyValuePair.Value);
				next.UpdateLayout(base.Widget.width);
				_widgets.Add(next.Widget);
				AddSpaceWidget(10);
			}
		}
		else
		{
			AddSpaceWidget(10);
		}
		_keyValueLabels.EndLoad();
		AddSpaceWidget(10);
		_spaceWidgets.EndLoad();
		base.Widget.height = (int)UIUtility.WidgetsReposition(_widgets, base.Widget, Vector3.down);
		UIUtility.UpdateAnchors(base.transform);
	}

	private void AddSpaceWidget(int space)
	{
		UIWidget next = _spaceWidgets.GetNext();
		next.height = space;
		_widgets.Add(next);
	}
}
