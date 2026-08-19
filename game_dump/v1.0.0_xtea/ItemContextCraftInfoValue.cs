using System;
using Building_;
using Crafting;
using UnityEngine;

public class ItemContextCraftInfoValue : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private GameObject _craftIcon;

	private bool _initialized;

	private bool _enableCraftLink;

	private RecipeSystem.RecipeType _type;

	private string _id = string.Empty;

	public void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			UIEventListener uIEventListener = UIEventListener.Get(((Component)this).gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickThis));
		}
	}

	public void Set(Recipe recipe, bool enableCraftLink)
	{
		_type = RecipeSystem.RecipeType.Crafting;
		_id = recipe.Id;
		_enableCraftLink = enableCraftLink;
		SetIcon(recipe.Icon);
		_craftIcon.SetActive(enableCraftLink);
		_text.text = recipe.LocalizedName;
	}

	public void Set(Blueprint blueprint, bool enableCraftLink)
	{
		_type = RecipeSystem.RecipeType.Building;
		_id = blueprint.Id;
		_enableCraftLink = enableCraftLink;
		SetIcon(blueprint.Icon);
		_craftIcon.SetActive(enableCraftLink);
		_text.text = blueprint.LocalizedName;
	}

	private void SetIcon(string icon)
	{
		_icon.spriteName = icon;
	}

	private void OnClickThis(GameObject go)
	{
		if (_enableCraftLink)
		{
			UIManager.FindScript<RecipeSelectorGroup>().Open(_type, _id);
			TooltipBase componentInParent = ((Component)this).GetComponentInParent<TooltipBase>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				componentInParent.Hide();
			}
		}
	}
}
