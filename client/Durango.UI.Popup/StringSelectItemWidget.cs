using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class StringSelectItemWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UISprite _separator;

	private UIWidget _widget;

	public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());

	public float TextWidth => _label.printedSize.x;

	public event Action<StringSelectItemWidget> Clicked;

	public event Action<Vector2> Draged;

	public void SetText(string text)
	{
		_label.text = text;
		_label.color = Color.white;
		_label.fontStyle = FontStyle.Normal;
	}

	public void SetColor(Color color)
	{
		_label.color = color;
	}

	public void SetBold(bool bold)
	{
		_label.fontStyle = (bold ? FontStyle.Bold : FontStyle.Normal);
	}

	public void EnableSeparator(bool enable)
	{
		if (_separator != null)
		{
			_separator.gameObject.SetActive(enable);
		}
	}

	public void SetWidth(int width)
	{
		Widget.width = width;
		RectLayoutComponent component = GetComponent<RectLayoutComponent>();
		if (component != null)
		{
			component.UpdateLayout();
		}
		_label.ProcessText();
		Widget.UpdateAnchors();
	}

	private void OnClick()
	{
		if (this.Clicked != null)
		{
			this.Clicked(this);
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (this.Draged != null)
		{
			this.Draged(delta);
		}
	}
}
