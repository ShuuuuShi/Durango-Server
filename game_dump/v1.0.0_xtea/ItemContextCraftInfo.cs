using System.Collections.Generic;
using Building_;
using Crafting;
using L10N;
using UnityEngine;

public class ItemContextCraftInfo : ItemContextBase
{
	[SerializeField]
	protected ListObjectPool _valueControls;

	public void Set(HashSet<Recipe> recipes, bool enableCraftLink)
	{
		base.Id = RecipeSystem.RecipeType.Crafting.ToString();
		base.HeaderText = T._("제작 가능한 아이템");
		_valueControls.Clear();
		foreach (Recipe recipe in recipes)
		{
			ItemContextCraftInfoValue itemContextCraftInfoValue = ((ListObjectPoolBase<GameObject>)_valueControls).Add<ItemContextCraftInfoValue>();
			itemContextCraftInfoValue.Init();
			itemContextCraftInfoValue.Set(recipe, enableCraftLink);
		}
		UpdateLayout();
	}

	public void Set(HashSet<Blueprint> blueprints, bool enableCraftLink)
	{
		base.Id = RecipeSystem.RecipeType.Building.ToString();
		base.HeaderText = T._("건설 가능한 건축물");
		_valueControls.Clear();
		foreach (Blueprint blueprint in blueprints)
		{
			ItemContextCraftInfoValue itemContextCraftInfoValue = ((ListObjectPoolBase<GameObject>)_valueControls).Add<ItemContextCraftInfoValue>();
			itemContextCraftInfoValue.Init();
			itemContextCraftInfoValue.Set(blueprint, enableCraftLink);
		}
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		_body.height = (int)(_valueControls.Reposition(Vector3.down) - _valueControls.BaseObject.GetComponent<UIWidget>().GetPosition(0f, 1f).y);
	}
}
