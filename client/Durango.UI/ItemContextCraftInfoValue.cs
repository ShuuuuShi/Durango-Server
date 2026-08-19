using Building;
using Crafting;
using Durango.UI.Popup;
using UnityEngine;

namespace Durango.UI;

public class ItemContextCraftInfoValue : UIWidget
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _text;

	private bool _enableCraftLink;

	private RecipeSystem.RecipeType _type;

	private string _id = string.Empty;

	public void Set(Recipe recipe, bool enableCraftLink)
	{
		_type = RecipeSystem.RecipeType.Crafting;
		_id = recipe.Id;
		_enableCraftLink = enableCraftLink;
		SetIcon(recipe.Icon);
		if (enableCraftLink)
		{
			_text.text = $"{recipe.Name} [FFFFFF7F]>[-]";
		}
		else
		{
			_text.text = recipe.Name;
		}
	}

	public void Set(Blueprint blueprint, bool enableCraftLink)
	{
		_type = RecipeSystem.RecipeType.Building;
		_id = blueprint.Id;
		_enableCraftLink = enableCraftLink;
		SetIcon(blueprint.Icon);
		if (enableCraftLink)
		{
			_text.text = $"{blueprint.Name} [FFFFFF7F]>[-]";
		}
		else
		{
			_text.text = blueprint.Name;
		}
	}

	private void SetIcon(string icon)
	{
		_icon.spriteName = icon;
	}

	private void OnClick()
	{
		if (_enableCraftLink)
		{
			RecipeSelectorGroup.OpenRecipeOrLearnableUI(_type, _id);
			TooltipBase componentInParent = GetComponentInParent<TooltipBase>();
			if (componentInParent != null)
			{
				componentInParent.Hide();
			}
		}
	}
}
