using System;
using UnityEngine;

public class TagSearchCard : MonoBehaviour
{
	public Action<TagSearchCard> Clicked;

	[SerializeField]
	private UIWidget _labelWithIconContainer;

	[SerializeField]
	private UIWidget _labelOnlyContainer;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _nameLabelWithIcon;

	[SerializeField]
	private UILabel _nameLabelOnly;

	private UIWidget _widget;

	private Selectable _selectable;

	private bool _hideIcon;

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

	private Selectable Selectable
	{
		get
		{
			if ((Object)(object)_selectable == (Object)null)
			{
				_selectable = ((Component)this).GetComponent<Selectable>();
			}
			return _selectable;
		}
	}

	public string Id { get; private set; }

	public string Name { get; private set; }

	public string Category { get; private set; }

	public bool Select
	{
		get
		{
			return Selectable.Select;
		}
		set
		{
			Selectable.Select = value;
		}
	}

	public bool HideIcon
	{
		get
		{
			return _hideIcon;
		}
		set
		{
			if (_hideIcon != value)
			{
				_hideIcon = value;
				((Component)_labelWithIconContainer).gameObject.SetActive(!_hideIcon);
				((Component)_labelOnlyContainer).gameObject.SetActive(_hideIcon);
				Widget.height = ((!_hideIcon) ? _labelWithIconContainer.height : _labelOnlyContainer.height);
			}
		}
	}

	public void Set(string category, TagFilterSelectorWidget.ItemStruct val)
	{
		Id = val.Id;
		Name = val.Name;
		Category = category;
		_iconSprite.spriteName = ((!string.IsNullOrEmpty(val.Icon)) ? val.Icon : "icon_question");
		_nameLabelOnly.text = val.Name;
		_nameLabelWithIcon.text = val.Name;
		Select = false;
	}

	private void OnClick()
	{
		if (Clicked != null)
		{
			Clicked(this);
		}
	}
}
