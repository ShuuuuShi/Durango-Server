using UnityEngine;

public class MusicMenuButton : Selectable
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private GameObject _selector;

	public string Icon
	{
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				((Component)_iconSprite).gameObject.SetActive(false);
				return;
			}
			((Component)_iconSprite).gameObject.SetActive(true);
			_iconSprite.spriteName = value;
			UIUtility.ResizeToSquare(_iconSprite, ((Component)((Component)_iconSprite).transform.parent).GetComponent<UIWidget>().width);
		}
	}

	public string Text
	{
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				((Component)_textLabel).gameObject.SetActive(false);
				return;
			}
			((Component)_textLabel).gameObject.SetActive(true);
			_textLabel.text = LocalizeSystem.Get(value);
		}
	}

	protected override void OnInit()
	{
		Icon = _iconSprite.spriteName;
	}

	protected override void Refresh(bool select)
	{
		((Component)this).GetComponent<PressColorChange>().Select(select);
	}
}
