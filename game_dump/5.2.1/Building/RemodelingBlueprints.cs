using System.Collections.Generic;
using Yaml;
using Yaml.Util;

namespace Building;

public class RemodelingBlueprints
{
	private readonly Dictionary<string, Dictionary<string, Blueprint>> _blueprints = new Dictionary<string, Dictionary<string, Blueprint>>();

	public void Initialize(Dictionary<string, Dictionary<string, RemodelingBlueprint>> data, Dictionary<int, ArtifactPrototype> prototypes)
	{
		foreach (KeyValuePair<string, Dictionary<string, RemodelingBlueprint>> datum in data)
		{
			string key = datum.Key;
			Yaml.Blueprint blueprint = SingletonDict<string, Yaml.Blueprint>.Get(key);
			if (blueprint == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, RemodelingBlueprint> item in datum.Value)
			{
				if (item.Value != null)
				{
					Blueprint blueprint2 = new Blueprint(key, blueprint, item.Key, item.Value);
					Add(blueprint2);
				}
			}
		}
		FillArtifactPrototypes(prototypes);
	}

	private void FillArtifactPrototypes(Dictionary<int, ArtifactPrototype> data)
	{
		foreach (KeyValuePair<int, ArtifactPrototype> datum in data)
		{
			string _name__ = datum.Value.__name__;
			Dictionary<string, Blueprint> dictionary = _blueprints.Get(_name__);
			if (dictionary == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, Blueprint> item in dictionary)
			{
				item.Value.SetPrototypeInfo(datum.Key, datum.Value);
			}
		}
	}

	private void Add(Blueprint blueprint)
	{
		if (blueprint != null && !string.IsNullOrEmpty(blueprint.RemodelingParentId))
		{
			if (!_blueprints.TryGetValue(blueprint.RemodelingParentId, out var value))
			{
				value = new Dictionary<string, Blueprint>();
				_blueprints[blueprint.RemodelingParentId] = value;
			}
			value[blueprint.Id] = blueprint;
		}
	}

	public Blueprint Get(string id, string slotId)
	{
		Blueprint value = null;
		if (_blueprints.TryGetValue(id, out var value2))
		{
			value2.TryGetValue(slotId, out value);
		}
		return value;
	}

	public Dictionary<string, Blueprint> Get(string id)
	{
		return _blueprints.Get(id);
	}
}
