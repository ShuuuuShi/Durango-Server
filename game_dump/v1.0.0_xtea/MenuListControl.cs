using MenuData;
using UnityEngine;

public class MenuListControl : Selectable
{
	[SerializeField]
	private UISprite _menuIcon;

	[SerializeField]
	private UILabel _menuLabel;

	[SerializeField]
	private GameObject _newCheck;

	[SerializeField]
	private UILabel _newCount;

	private string _menuIconSprite;

	private string _menuLabelText;

	private UIWidget _widget;

	public MenuType Type { get; set; }

	public string MenuIcon
	{
		get
		{
			return _menuIconSprite;
		}
		set
		{
			_menuIconSprite = value;
			if (!((Object)(object)_menuIcon == (Object)null))
			{
				_menuIconSprite = value;
				_menuIcon.spriteName = _menuIconSprite;
			}
		}
	}

	public string MenuLabel
	{
		get
		{
			return _menuLabelText;
		}
		set
		{
			_menuLabelText = value;
			if (!((Object)(object)_menuLabel == (Object)null))
			{
				if (string.IsNullOrEmpty(value))
				{
					((Component)_menuLabel).gameObject.SetActive(false);
					return;
				}
				((Component)_menuLabel).gameObject.SetActive(true);
				_menuLabel.text = value;
			}
		}
	}

	public UILabel TextLabel => _menuLabel;

	public int NewCount
	{
		set
		{
			if ((Object)(object)_newCheck != (Object)null)
			{
				_newCheck.SetActive(value > 0);
			}
			if ((Object)(object)_newCount != (Object)null)
			{
				_newCount.text = value.ToString();
			}
		}
	}

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

	protected override void OnInit()
	{
	}

	protected override void Refresh(bool select)
	{
		if (base.Disable)
		{
			Widget.alpha = 0.5f;
		}
	}

	protected override void OnSelected(bool select)
	{
		PressColorChange component = ((Component)this).GetComponent<PressColorChange>();
		component.Select(select);
		base.OnSelected(select);
	}

	public int GetLabelWidth()
	{
		return _menuLabel.width;
	}
}
