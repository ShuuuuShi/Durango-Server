using System;
using System.Collections.Generic;
using Crafting;
using Yaml;

namespace Building_;

public class BlueprintContainer : GenericRecipeContainer<Blueprint, BlueprintCategory>
{
	private Dictionary<string, KeyValuePair<int, ArtifactPrototype>> _prototypes;

	public void SetArtifactPrototypes(Dictionary<int, ArtifactPrototype> prototypes)
	{
		_prototypes = new Dictionary<string, KeyValuePair<int, ArtifactPrototype>>();
		foreach (KeyValuePair<int, ArtifactPrototype> prototype in prototypes)
		{
			string _name__ = prototype.Value.__name__;
			_prototypes.Add(_name__, new KeyValuePair<int, ArtifactPrototype>(prototype.Key, prototype.Value));
		}
		if (_categoryList == null)
		{
			return;
		}
		foreach (KeyValuePair<string, KeyValuePair<int, ArtifactPrototype>> prototype2 in _prototypes)
		{
			GetBlueprint(prototype2.Key)?.SetPrototypeInfo(prototype2.Value.Key, prototype2.Value.Value);
		}
	}

	public void SetBlueprints(Dictionary<string, Yaml.Blueprint> dict)
	{
		Clear();
		Dictionary<string, BlueprintCategory> dictionary = new Dictionary<string, BlueprintCategory>();
		foreach (KeyValuePair<string, Yaml.Blueprint> item in dict)
		{
			if (!dictionary.TryGetValue(item.Value.category, out var value))
			{
				value = new BlueprintCategory();
				value.Id = item.Value.category;
				value.Name = LocalizeSystem.Get("#recipe_category_" + item.Value.category);
				value.Blueprints = new List<Blueprint>();
				dictionary.Add(value.Id, value);
			}
			Blueprint blueprint = new Blueprint();
			value.Blueprints.Add(blueprint);
			blueprint.Id = item.Key;
			blueprint.SetBlueprintInfo(item.Key, item.Value);
			if (_prototypes != null && _prototypes.TryGetValue(item.Key, out var value2))
			{
				blueprint.SetPrototypeInfo(value2.Key, value2.Value);
			}
		}
		foreach (KeyValuePair<string, BlueprintCategory> item2 in dictionary)
		{
			_categoryList.Add(item2.Value);
		}
		OnInit();
	}

	public Blueprint GetBlueprint(string id)
	{
		return GetRecipe(id);
	}

	public Blueprint GetBlueprint(int entityType)
	{
		for (int i = 0; i < _categoryList.Count; i++)
		{
			BlueprintCategory blueprintCategory = _categoryList[i];
			for (int j = 0; j < blueprintCategory.Blueprints.Count; j++)
			{
				if (blueprintCategory.Blueprints[j].EntityType == entityType)
				{
					return blueprintCategory.Blueprints[j];
				}
			}
		}
		return null;
	}

	public void Enumerate(Action<Blueprint> delegator)
	{
		if (_categoryList == null)
		{
			return;
		}
		for (int i = 0; i < _categoryList.Count; i++)
		{
			BlueprintCategory blueprintCategory = _categoryList[i];
			for (int j = 0; j < blueprintCategory.Blueprints.Count; j++)
			{
				delegator(blueprintCategory.Blueprints[j]);
			}
		}
	}
}
