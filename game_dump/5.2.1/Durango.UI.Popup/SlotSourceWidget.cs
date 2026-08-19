using System.Collections.Generic;
using Crafting;
using L10N;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class SlotSourceWidget : UIWidget
{
	[SerializeField]
	private SlotSourceItem _source;

	private ListObjectPool<SlotSourceItem> _slotSources;

	private bool _isInit;

	public SlotInfoPopup Parent { get; set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_slotSources = new ListObjectPool<SlotSourceItem>();
			_slotSources.BaseObject = _source;
		}
	}

	public int Set(IList<SlotSourceInfo> infos, int level, int limitWidth)
	{
		Init();
		int num = 0;
		int limitTextWidth = limitWidth - 60;
		_slotSources.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(infos); i < size; i++)
		{
			int b = SetInfoText(infos[i], level, limitTextWidth);
			num = Mathf.Max(num, b);
		}
		_slotSources.EndLoad();
		float num2 = _slotSources.Reposition(Vector3.down);
		base.height = (int)(Mathf.Abs(_source.GetPosition(0f, 1f).y - localCorners[1].y) + num2);
		return num;
	}

	private int SetInfoText(SlotSourceInfo info, int level, int limitTextWidth)
	{
		switch (info.type)
		{
		case SourceDescription.Text:
			return _slotSources.GetNext().Set(info.text, limitTextWidth);
		case SourceDescription.Craft:
		{
			SlotSourceItem next2 = _slotSources.GetNext();
			if (string.IsNullOrEmpty(info.recipe_id2))
			{
				string recipeName2 = GetRecipeName(info.recipe_id);
				return next2.Set(T._("<ref>ui://Recipe/Crafting/{1},{0}</ref>{0:-으로} 제작합니다.", recipeName2, info.recipe_id), limitTextWidth);
			}
			string recipeName3 = GetRecipeName(info.recipe_id);
			string recipeName4 = GetRecipeName(info.recipe_id2);
			return next2.Set(T._("<ref>ui://Recipe/Crafting/{1},{0}</ref> 결과물로 <ref>ui://Recipe/Crafting/{3},{2}</ref>{2:-을} 제작합니다.", recipeName3, info.recipe_id, recipeName4, info.recipe_id2), limitTextWidth);
		}
		case SourceDescription.Collect:
			return _slotSources.GetNext().Set(T._("<em>{0}</em>에서 <em>{1}</em>{1:-을} 채집합니다.", GetCollectibleName(info.collectible_id), GetGeneratorName(info.generator_id)), limitTextWidth);
		case SourceDescription.BoxCollect:
			return _slotSources.GetNext().Set(T._("<em>{0}</em>에서 <em>{1}</em>{1:-을} 채집합니다.", GetCollectibleName(info.collectible_id), GetPrototypeName(info.prototype_id, level)), limitTextWidth);
		case SourceDescription.CollectAndCraft:
		{
			SlotSourceItem next = _slotSources.GetNext();
			string recipeName = GetRecipeName(info.recipe_id);
			return next.Set(T._("<em>{0}</em>에서 <em>{1}</em>{1:-을} 채집하여 <ref>ui://Recipe/Crafting/{3},{2}</ref>{2:-을} 제작합니다.", GetCollectibleName(info.collectible_id), GetGeneratorName(info.generator_id), recipeName, info.recipe_id), limitTextWidth);
		}
		default:
			return 0;
		}
	}

	private static string GetRecipeName(string recipeId)
	{
		Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(recipeId);
		if (recipe == null)
		{
			return recipeId;
		}
		return recipe.Name;
	}

	private static string GetCollectibleName(string collectibleId)
	{
		string text = SingletonDict<string, Gettext>.Get(collectibleId).ToString();
		if (string.IsNullOrEmpty(text))
		{
			return collectibleId;
		}
		return text;
	}

	private static string GetGeneratorName(string generatorId)
	{
		GeneratorData generatorData = SingletonDict<string, GeneratorData>.Get(generatorId);
		if (generatorData == null)
		{
			return generatorId;
		}
		return generatorData.name.ToString();
	}

	private static string GetPrototypeName(string prototypeId, int level)
	{
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(prototypeId, level);
		if (itemPrototype == null)
		{
			return prototypeId;
		}
		return itemPrototype.Name.ToString();
	}
}
