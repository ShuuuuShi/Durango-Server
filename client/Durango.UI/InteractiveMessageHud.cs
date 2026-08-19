using System;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class InteractiveMessageHud : MonoBehaviour
{
	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UIWidget _bottomWidget;

	[SerializeField]
	private UILabel _mainLabel;

	[SerializeField]
	private SelectableButton _cancel;

	[SerializeField]
	private SelectableButton _accept;

	[SerializeField]
	private UILabel _subLabel;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private GameObject _questionIcon;

	[SerializeField]
	private RectLayoutComponent _rectLayout;

	public void Show([NotNull] string iconName, [NotNull] string mainText, string subText, string cancelButtonText, Action cancelClicked, int titleMargin, string acceptButtonText = null, Action acceptClicked = null, Action titleClicked = null)
	{
		_icon.spriteName = iconName;
		_mainLabel.text = mainText;
		_titleWidget.height = (int)(_mainLabel.printedSize.y + (float)(titleMargin * 2));
		_subLabel.gameObject.SetActive(!string.IsNullOrEmpty(subText));
		_subLabel.text = subText;
		bool flag = titleClicked != null;
		_questionIcon.gameObject.SetActive(flag);
		if (flag)
		{
			UIEventListener.Get(_titleWidget.gameObject).onClick = delegate
			{
				titleClicked();
			};
		}
		else
		{
			UIEventListener.Get(_titleWidget.gameObject).onClick = null;
		}
		_accept.gameObject.SetActive(acceptClicked != null);
		_accept.Text = acceptButtonText;
		_accept.Clicked = acceptClicked;
		_cancel.gameObject.SetActive(cancelClicked != null);
		_cancel.Text = cancelButtonText;
		_cancel.Clicked = cancelClicked;
		base.gameObject.SetActive(value: true);
		_rectLayout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
