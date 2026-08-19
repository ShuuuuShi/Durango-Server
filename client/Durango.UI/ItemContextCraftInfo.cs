using System.Collections.Generic;
using Building;
using Crafting;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ItemContextCraftInfo : ItemContextBase
{
	[SerializeField]
	private ItemContextCraftInfoValue _baseComponent;

	private ListObjectPool<ItemContextCraftInfoValue> _components;

	public override void Init()
	{
		base.Init();
		_components = new ListObjectPool<ItemContextCraftInfoValue>();
		_components.BaseObject = _baseComponent;
	}

	public void Clear()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Set(HashSet<Recipe> recipes, bool enableCraftLink)
	{
		if (KUtility.GetSize(recipes) == 0)
		{
			Clear();
			return;
		}
		base.gameObject.SetActive(value: true);
		base.HeaderText = T._("제작 가능한 아이템");
		int count = 0;
		foreach (Recipe recipe in recipes)
		{
			ItemContextCraftInfoValue orAdd = _components.GetOrAdd(count++);
			orAdd.Set(recipe, enableCraftLink);
		}
		_components.Set(count);
		UpdateLayout();
	}

	public void Set(HashSet<Blueprint> blueprints, bool enableCraftLink)
	{
		if (KUtility.GetSize(blueprints) == 0)
		{
			Clear();
			return;
		}
		base.gameObject.SetActive(value: true);
		base.HeaderText = T._("건설 가능한 건축물");
		int count = 0;
		foreach (Blueprint blueprint in blueprints)
		{
			ItemContextCraftInfoValue orAdd = _components.GetOrAdd(count++);
			orAdd.Set(blueprint, enableCraftLink);
		}
		_components.Set(count);
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		Vector3[] array = _body.localCorners;
		float num = UIUtility.WidgetsReposition(_components, Vector3.down, Vector3.Lerp(array[1], array[2], 0.5f) + new Vector3(0f, -5f));
		_body.height = (int)num + 10;
	}
}
