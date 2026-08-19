using System.Collections.Generic;
using L10N;
using Shared.Ability;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Statistics;

public class Title
{
	private readonly Yaml.Title _title;

	public bool Enabled { get; set; }

	public string Id { get; private set; }

	public bool IsNew { get; set; }

	public string Name => _title.name;

	public string Description => _title.description;

	public Title(string key, Yaml.Title title)
	{
		Id = key;
		_title = title;
	}

	public Dictionary<Basic, int> GetAbilities()
	{
		return _title.abilities;
	}

	public int GetAbility(Basic key)
	{
		return (_title.abilities != null) ? _title.abilities.Get(key, 0) : 0;
	}

	public Dictionary<string, float> GetModifiers()
	{
		return _title.modifiers;
	}

	public List<string> GetAbilityModifiersText()
	{
		List<string> list = new List<string>();
		Dictionary<Basic, int> abilities = GetAbilities();
		if (abilities != null)
		{
			foreach (KeyValuePair<Basic, int> item in abilities)
			{
				if (item.Value != 0)
				{
					list.Add($"{item.Key.GetName()} {item.Value:+#;-#}");
				}
			}
		}
		Dictionary<string, float> modifiers = GetModifiers();
		if (modifiers != null)
		{
			foreach (KeyValuePair<string, float> item2 in modifiers)
			{
				if (item2.Value != 0f)
				{
					SkillModifier skillModifier = SingletonDict<string, SkillModifier>.Get(item2.Key);
					if (skillModifier != null)
					{
						list.Add($"{skillModifier.Name} {skillModifier.GetValueString(item2.Value)}");
					}
				}
			}
		}
		return list;
	}
}
