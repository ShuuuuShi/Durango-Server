using Durango.Network;
using JetBrains.Annotations;
using Messages;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Explore;

public class Region
{
	private readonly Role _role;

	public static readonly Region UnknownRegion = new Region(Shared.Region.Role.Invalid);

	private string _emblem;

	public string Id { get; private set; }

	public string Name { get; private set; }

	public string TemplateId { get; private set; }

	public string TerrainId { get; private set; }

	public double CreatedAt { get; private set; }

	[CanBeNull]
	public RegionTemplate Template { get; private set; }

	public int Level => (Template != null) ? Template.Level : 0;

	public bool IsUnstable => _role == Shared.Region.Role.Risky || _role == Shared.Region.Role.Outpost;

	public double DestroyAt
	{
		get
		{
			if (Template != null)
			{
				return CreatedAt + Template.ExpiresIn;
			}
			return 0.0;
		}
	}

	public bool LifespanInvisible => Template != null && Template.LifespanInvisible;

	public double RemaingTime
	{
		get
		{
			if (Template != null)
			{
				return DestroyAt - Connections.Frontend.GetPredictedServerTime();
			}
			return 0.0;
		}
	}

	private Region(Role role)
	{
		Id = string.Empty;
		Name = string.Empty;
		TerrainId = string.Empty;
		CreatedAt = 0.0;
		_role = role;
		Init(null);
		if (Template != null)
		{
			_role = Template.Role;
		}
	}

	public Region(Messages.Region region)
	{
		Id = region.Id;
		Name = region.Name;
		TerrainId = region.TerrainId;
		CreatedAt = region.CreatedAt;
		_role = region.Role;
		Init(region.TemplateId);
	}

	public Region(RegionJson json)
	{
		Id = json.id;
		Name = json.name;
		TerrainId = json.terrain_id;
		CreatedAt = json.created_at;
		_role = json.role;
		Init(json.template_id);
	}

	private void Init(string templateId)
	{
		TemplateId = templateId;
		Template = ((templateId != null) ? SingletonDict<string, RegionTemplate>.Get(templateId) : null);
		_emblem = ((Template != null) ? Template.Emblem : null);
	}

	public Biome MajorBiome()
	{
		return (Template != null) ? Template.MajorBiome() : Biome.Invalid;
	}

	public Role Role()
	{
		return (_role != Shared.Region.Role.Invalid) ? _role : ((Template == null) ? Shared.Region.Role.Invalid : Template.Role);
	}

	public bool IsTutorial()
	{
		return Role() == Shared.Region.Role.Tutorial;
	}

	public bool IsSafeHouse()
	{
		return Role() == Shared.Region.Role.Safehouse;
	}

	public bool IsAfterSafeHouse()
	{
		Role role = Role();
		return role != Shared.Region.Role.Tutorial && role != Shared.Region.Role.Safehouse;
	}

	public bool IsAfterRural()
	{
		Role role = Role();
		return role != Shared.Region.Role.Tutorial && role != Shared.Region.Role.Safehouse && role != Shared.Region.Role.Rural && role != Shared.Region.Role.Personal;
	}

	public bool IsWarpRush()
	{
		return Template != null && Template.Tags.Contains("season_02") && Role() == Shared.Region.Role.Instance;
	}

	public bool IsPvpIsland()
	{
		return Template != null && Template.Tags.Contains("pvpisland") && Role() == Shared.Region.Role.Instance;
	}

	public bool CanRevive()
	{
		return Template != null && !Template.CannotRevive;
	}

	public string GetEmblem()
	{
		return _emblem;
	}

	public bool IsNew()
	{
		return Connections.Frontend.GetPredictedServerTime() - CreatedAt < 3600.0;
	}

	public static GameObject GetEmblemIcon(string emblem)
	{
		GameObject gameObject = Resources.Load<GameObject>($"TerrainIcons/{emblem}");
		return (!(gameObject == null)) ? gameObject : Resources.Load<GameObject>("TerrainIcons/unknown");
	}

	public static GameObject InstantiateIcon(Transform parent, string emblem)
	{
		for (int num = parent.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(parent.GetChild(num).gameObject);
		}
		GameObject emblemIcon = GetEmblemIcon(emblem);
		if (emblemIcon == null)
		{
			return null;
		}
		GameObject gameObject = parent.gameObject.AddChild(emblemIcon);
		gameObject.transform.localScale = emblemIcon.transform.localScale;
		return gameObject;
	}
}
