using System;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class SearchInfoItemNode : MonoBehaviour
{
	[SerializeField]
	private UISpriteLabel _label;

	[CanBeNull]
	[SerializeField]
	private UISprite _cancelIcon;

	private Action<GameObject> _clicked;

	private UIWidget _widget;

	public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());

	private void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate(GameObject go)
		{
			if (_clicked != null)
			{
				_clicked(go);
			}
		});
	}

	public void Set(string text, [CanBeNull] Action<GameObject> clickAction = null)
	{
		_label.text = text;
		int num = ((!(_cancelIcon == null)) ? _cancelIcon.width : 0);
		Widget.width = _label.width + num + (int)_label.transform.localPosition.x * 2;
		_clicked = clickAction;
		UIUtility.UpdateAnchors(base.transform);
	}
}
