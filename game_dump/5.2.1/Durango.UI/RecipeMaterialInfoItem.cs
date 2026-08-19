using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class RecipeMaterialInfoItem : MonoBehaviour
{
	public Action<RecipeMaterialInfoItem> Clicked;

	[SerializeField]
	private UISprite _checkSprite;

	[SerializeField]
	private SpriteData _checkIcon;

	[SerializeField]
	private SpriteData _normalIcon;

	[SerializeField]
	private UISpriteLabel _textLabel;

	[SerializeField]
	private SpriteData _linkIcon;

	[SerializeField]
	private UILabel _countLabel;

	private UIWidget _widget;

	private int _vPadding;

	private bool _isInit;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_vPadding = Widget.height - _textLabel.height;
		}
	}

	public void Set(RecipeInfoWidget.SlotStruct data)
	{
		Init();
		int count = data.Count;
		int requiredCount = data.RequiredCount;
		_countLabel.text = string.Format((count >= requiredCount) ? "[FFD85B]{0}[-] [71716B]/[-] [E8E5DF]{1}[-]" : "[DD5C56]{0}[-] [71716B]/[-] [E8E5DF]{1}[-]", count, requiredCount);
		string text = ((data.RequiredLevel <= 1) ? data.Name : T._("{0} {1:lv:}", data.Name, data.RequiredLevel));
		_textLabel.overflowWidth = (int)((float)Widget.width - Mathf.Abs(Widget.localCorners[0].x - _textLabel.GetPosition(0f, 0f).x) - Mathf.Abs(Widget.localCorners[3].x - _countLabel.GetPosition(0f, 0f).x) - 20f);
		_textLabel.text = text + " [size=24][icon=" + _linkIcon.sprite + "][/size]";
		if (count < requiredCount)
		{
			_normalIcon.Set(_checkSprite);
		}
		else
		{
			_checkIcon.Set(_checkSprite);
		}
		Widget.height = _textLabel.height + _vPadding;
	}

	private void OnClick()
	{
		if (Clicked != null)
		{
			Clicked(this);
		}
	}
}
