using UnityEngine;

public class ComboBoxItem : MonoBehaviour
{
	[SerializeField]
	private UILabel _textLabel;

	private UIWidget _widget;

	private string _textKey;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public ComboBox Parent { get; set; }

	public int Index { get; set; }

	public string Text
	{
		get
		{
			return _textKey;
		}
		set
		{
			_textKey = value;
			_textLabel.text = LocalizeSystem.Get(value);
		}
	}

	public float GetHeight()
	{
		return Widget.height;
	}

	public void Show()
	{
		TweenAlpha.Begin(((Component)this).gameObject, 0.3f, 1f);
	}

	public void Hide()
	{
		TweenAlpha.Begin(((Component)this).gameObject, 0.3f, 0f);
	}
}
