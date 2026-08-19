using Crafting;
using JetBrains.Annotations;
using UnityEngine;

public class CategoryNodeControl : Selectable
{
	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISprite _nameBG;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UISprite _selector;

	[SerializeField]
	private GameObject _newIcon;

	[SerializeField]
	private UISprite _guidedIcon;

	private bool _isPress;

	private UIWidget _widget;

	private CategoryItem _categoryItem;

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

	public RecipeSystem.RecipeType Type { get; private set; }

	public string Id => _categoryItem.Id;

	public Vector3 Position
	{
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Component)this).transform.localPosition = value;
		}
	}

	public bool IsGuided
	{
		set
		{
			((Component)_guidedIcon).gameObject.SetActive(value);
		}
	}

	[UsedImplicitly]
	private void OnPress(bool press)
	{
		_isPress = press;
		Refresh();
	}

	protected override void OnInit()
	{
	}

	protected override void Refresh(bool isSelect)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if (isSelect || _isPress)
		{
			_icon.color = PresetColor.UIYellow;
			_nameBG.color = PresetColor.UIYellow;
			_nameLabel.color = PresetColor.UIBlack;
			_selector.color = PresetColor.UIYellow;
			_guidedIcon.color = PresetColor.UIYellow;
		}
		else if (base.Disable)
		{
			_icon.color = PresetColor.UIDarkBrown;
			_nameBG.color = PresetColor.UIBlackAlpha50;
			_selector.color = PresetColor.UIDarkBrownGray;
			_nameLabel.color = PresetColor.UIGrayBrown;
			_guidedIcon.color = PresetColor.UIDarkBrown;
		}
		else
		{
			_icon.color = PresetColor.UILightBrown;
			_nameBG.color = PresetColor.UIDeepDarkBrown;
			_selector.color = PresetColor.UIBrown;
			_nameLabel.color = PresetColor.UIRedBrown;
			_guidedIcon.color = PresetColor.UILightBrown;
		}
		((Component)_selector).gameObject.SetActive(isSelect);
		Widget.alpha = ((!base.Disable) ? 1f : 0.8f);
	}

	public void Set(CategoryItem item, RecipeSystem.RecipeType type)
	{
		if (_categoryItem != null)
		{
			EventDelegate.Remove(_categoryItem.NewChecker.OnChangeList, OnNewStateChanged);
		}
		_categoryItem = item;
		Type = type;
		_nameLabel.text = item.LocalizedName;
		_icon.spriteName = item.Icon;
		item.NewChecker.RegisterCallback(OnNewStateChanged);
		OnNewStateChanged();
	}

	private void OnNewStateChanged()
	{
		_newIcon.gameObject.SetActive(_categoryItem.NewChecker.IsNew);
	}
}
