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

	public int Level
	{
		get
		{
			if (Template == null)
			{
				return 0;
			}
			return Template.Level;
		}
	}

	public bool IsUnstable
	{
		get
		{
			if (_role != Shared.Region.Role.Risky)
			{
				return _role == Shared.Region.Role.Outpost;
			}
			return true;
		}
	}

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

	public bool LifespanInvisible
	{
		get
		{
			if (Template != null)
			{
				return Template.LifespanInvisible;
			}
			return false;
		}
	}

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
		if (Template == null)
		{
			return Biome.Invalid;
		}
		return Template.MajorBiome();
	}

	public Role Role()
	{
		if (_role == Shared.Region.Role.Invalid)
		{
			if (Template != null)
			{
				return Template.Role;
			}
			return Shared.Region.Role.Invalid;
		}
		return _role;
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
		if (role != Shared.Region.Role.Tutorial)
		{
			return role != Shared.Region.Role.Safehouse;
		}
		return false;
	}

	public bool IsAfterRural()
	{
		Role role = Role();
		if (role != Shared.Region.Role.Tutorial && role != Shared.Region.Role.Safehouse && role != Shared.Region.Role.Rural)
		{
			return role != Shared.Region.Role.Personal;
		}
		return false;
	}

	public bool IsWarpRush()
	{
		if (Template != null && Template.Tags.Contains("season_02"))
		{
			return Role() == Shared.Region.Role.Instance;
		}
		return false;
	}

	public bool IsPvpIsland()
	{
		if (Template != null && Template.Tags.Contains("pvpisland"))
		{
			return Role() == Shared.Region.Role.Instance;
		}
		return false;
	}

	public bool CanRevive()
	{
		if (Template != null)
		{
			return !Template.CannotRevive;
		}
		return false;
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
		GameObject gameObject = Resources.Load<GameObject>("TerrainIcons/" + emblem);
		if (gameObject == null)
		{
			return Resources.Load<GameObject>("TerrainIcons/unknown");
		}
		return gameObject;
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
