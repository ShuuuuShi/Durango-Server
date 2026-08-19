using System;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class PlayerSearchInfoWidget : PlayerInfoWidget
{
	[SerializeField]
	private UIWidget _nameWidget;

	[SerializeField]
	private SelectableWidget _check;

	[NotNull]
	public SelectableWidget Check => _check;

	public bool Selected
	{
		set
		{
			Selectable component = GetComponent<Selectable>();
			if (component != null)
			{
				component.Selected = value;
			}
		}
	}

	private void Start()
	{
		if (_portraitTexture != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(_portraitTexture.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
			{
				ShowProfileTooltip();
			});
		}
	}

	public void EnableCheckMode(bool enable)
	{
		_check.gameObject.SetActive(enable);
		_nameWidget.leftAnchor.absolute = ((!enable) ? 10 : _check.Widget.width);
		_nameWidget.ResetAndUpdateAnchors();
	}
}
