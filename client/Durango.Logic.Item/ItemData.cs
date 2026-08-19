using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Messages;
using Shared.Item;
using Shared.Skill;
using UnityEngine;
using Yaml;

namespace Durango.Logic.Item;

public class ItemData
{
	public bool Valid;

	private readonly List<TagData> _tags = new List<TagData>();

	private readonly List<TagData> _tagModifications = new List<TagData>();

	private readonly List<Performance> _performances = new List<Performance>();

	private readonly List<ReformSlot> _reformSlots = new List<ReformSlot>();

	private ItemData[] _content;

	private bool[] _dyeables;

	[NotNull]
	public List<TagData> Tags => _tags;

	[NotNull]
	public List<TagData> TagModifications => _tagModifications;

	[NotNull]
	public List<Performance> Performances => _performances;

	public List<ReformSlot> ReformSlots => _reformSlots;

	public int ContentCount => (_content != null) ? _content.Length : 0;

	public string Id { get; set; }

	[CanBeNull]
	public Prototype Prototype { get; private set; }

	public string Name { get; set; }

	public ItemIcon Icon { get; set; }

	public ItemColor Colors => Icon.Colors;

	public string Description { get; private set; }

	public string PrototypeId { get; private set; }

	public string PrototypeName { get; set; }

	public int Level { get; private set; }

	public int ModifiableCount { get; private set; }

	public int ModifiedCount { get; private set; }

	public int Size { get; set; }

	public string FounderId { get; private set; }

	public string FounderCategory { get; private set; }

	public SafeLevel SafeLevel { get; set; }

	public bool Locked => SafeLevel == SafeLevel.Locked;

	[CanBeNull]
	public Gauge Durability { get; private set; }

	public int OriginalLevel { get; private set; }

	public bool IsEquipments { get; set; }

	public int Width { get; private set; }

	public int Height { get; private set; }

	public bool Unstable { get; set; }

	public Reins? Reins { get; private set; }

	public Messages.Pet? Pet => (!Reins.HasValue) ? null : Reins.Value.Pet;

	public ArtifactCapsule? Capsule { get; private set; }

	public RepairRequirement? RepairRequirement { get; private set; }

	public BlueprintItem? Blueprint { get; private set; }

	public string CollectibleId { get; private set; }

	public string GeneratorId { get; private set; }

	public bool Tradable { get; private set; }

	public bool Dumpable => Prototype == null || !Prototype.DumpLocked;

	public bool IsNew { get; set; }

	public bool IsRepairable => RepairRequirement.HasValue && !string.IsNullOrEmpty(RepairRequirement.Value.TagId);

	public string[] EmotionalMotions { get; private set; }

	public float PioneerCost { get; private set; }

	public LootBoxItem? LootBox { get; private set; }

	public ItemData()
	{
	}

	public ItemData(Messages.Item itemInfo)
	{
		Set(itemInfo);
	}

	public void Set(Messages.Item itemInfo)
	{
		_tags.Clear();
		_tagModifications.Clear();
		_performances.Clear();
		_reformSlots.Clear();
		Prototype = PrototypeYaml.GetItemPrototype(itemInfo.Prototype, itemInfo.Level);
		PrototypeId = itemInfo.Prototype;
		Name = itemInfo.Name;
		Icon = new ItemIcon(itemInfo);
		Description = ((itemInfo.Description != null) ? itemInfo.Description : ((Prototype == null) ? string.Empty : Prototype.Description.ToString()));
		PrototypeName = ((Prototype == null) ? string.Empty : Prototype.Name.ToString());
		Id = itemInfo.Id;
		Level = itemInfo.Level;
		ModifiableCount = itemInfo.ModifiableCount;
		ModifiedCount = itemInfo.ModifiedCount;
		Size = itemInfo.Size;
		Durability = itemInfo.Durability;
		OriginalLevel = itemInfo.OriginalLevel;
		Unstable = itemInfo.Unstable;
		IsEquipments = false;
		CollectibleId = itemInfo.CollectibleId;
		GeneratorId = itemInfo.GeneratorId;
		Tradable = itemInfo.Tradable;
		SetDyeable((Prototype == null) ? null : Prototype.Dyeables);
		FounderId = itemInfo.FounderId;
		FounderCategory = itemInfo.FounderCategory;
		EmotionalMotions = itemInfo.EmotionalMotions;
		PioneerCost = itemInfo.PioneerCost;
		AllocatedIconSize(Size);
		int num = itemInfo.Tags.Length;
		for (int i = 0; i < num; i++)
		{
			Messages.Tag tag = itemInfo.Tags[i];
			TagData tagData = TagData.Create(tag.Id, tag.Level);
			if (tagData != null)
			{
				_tags.Add(tagData);
			}
		}
		for (int j = 0; j < itemInfo.TagModifications.Length; j++)
		{
			Messages.Tag tag2 = itemInfo.TagModifications[j];
			TagData tagData2 = TagData.Create(tag2.Id, tag2.Level);
			if (tagData2 != null)
			{
				_tagModifications.Add(tagData2);
			}
		}
		if (itemInfo.Performance != null)
		{
			_performances.AddRange(itemInfo.Performance);
		}
		if (itemInfo.ReformSlots != null)
		{
			_reformSlots.AddRange(itemInfo.ReformSlots);
		}
		RepairRequirement = itemInfo.RepairRequirement;
		SetExtension(itemInfo.Ext);
		_tags.Sort((TagData t1, TagData t2) => t2.Grade - t1.Grade);
	}

	public void Set(PrototypePreset preset)
	{
		_tags.Clear();
		_tagModifications.Clear();
		_performances.Clear();
		_reformSlots.Clear();
		Id = null;
		Prototype = PrototypeYaml.GetItemPrototype(preset.PrototypeId, preset.Level);
		PrototypeId = preset.PrototypeId;
		Name = preset.Name;
		Icon = new ItemIcon(preset);
		Description = preset.Description;
		PrototypeName = ((Prototype == null) ? string.Empty : Prototype.Name.ToString());
		int level2 = (OriginalLevel = preset.Level);
		Level = level2;
		ModifiableCount = preset.ModifiableCount;
		ModifiedCount = 0;
		Size = preset.Size;
		Durability = new Gauge(preset.MaxDurability, 0f, new GaugeNode[1]
		{
			new GaugeNode(0.0, preset.MaxDurability)
		});
		Unstable = false;
		IsEquipments = false;
		CollectibleId = null;
		GeneratorId = null;
		Tradable = !preset.TradeLocked;
		SetDyeable((Prototype == null) ? null : Prototype.Dyeables);
		FounderId = null;
		FounderCategory = null;
		EmotionalMotions = preset.EmotionalMotions;
		PioneerCost = 0f;
		AllocatedIconSize(Size);
		int num = preset.Tags.Length;
		for (int i = 0; i < num; i++)
		{
			PrototypePresetTag prototypePresetTag = preset.Tags[i];
			TagData tagData = TagData.Create(prototypePresetTag.Id, prototypePresetTag.Level);
			if (tagData != null)
			{
				_tags.Add(tagData);
			}
		}
		if (preset.Performances != null)
		{
			PrototypePresetPerformance[] performances = preset.Performances;
			foreach (PrototypePresetPerformance prototypePresetPerformance in performances)
			{
				_performances.Add(new Performance
				{
					Id = prototypePresetPerformance.Id,
					Icon = prototypePresetPerformance.Icon,
					Name = prototypePresetPerformance.Name,
					Nums = prototypePresetPerformance.Nums,
					Strs = prototypePresetPerformance.Strs
				});
			}
		}
		RepairRequirement = ((preset.RepairRequirement != null) ? new RepairRequirement?(new RepairRequirement
		{
			TagId = preset.RepairRequirement.TagId,
			RepairPerformance = preset.RepairRequirement.RepairPerformance
		}) : null);
		Capsule = null;
		Reins = null;
		Blueprint = null;
		string extClassName = preset.ExtClassName;
		if (extClassName != null && extClassName == "ArtifactCapsule" && KUtility.GetSize(preset.ExtClassArgs) > 0)
		{
			Capsule = new ArtifactCapsule
			{
				BlueprintId = preset.ExtClassArgs.First(),
				ArtifactLevel = Level
			};
		}
		_content = null;
		_tags.Sort((TagData t1, TagData t2) => t2.Grade - t1.Grade);
	}

	private void SetDyeable(IList<ColorChannel> dyeables)
	{
		int i = 0;
		for (int size = KUtility.GetSize(dyeables); i < size; i++)
		{
			ColorChannel colorChannel = dyeables[i];
			if (_dyeables == null)
			{
				_dyeables = new bool[Icon.Colors.Count];
			}
			int num = (int)colorChannel;
			if (num >= 0 && num < _dyeables.Length)
			{
				_dyeables[num] = true;
			}
		}
	}

	private void SetExtension(object extension)
	{
		if (extension is Container container)
		{
			_content = new ItemData[container.Contents.Length];
			for (int i = 0; i < _content.Length; i++)
			{
				_content[i] = new ItemData(container.Contents[i]);
			}
		}
		else if (extension is Reins)
		{
			Reins = (Reins)extension;
		}
		else if (extension is ArtifactCapsule)
		{
			Capsule = (ArtifactCapsule)extension;
			ArtifactCapsule value = Capsule.Value;
			int j = 0;
			for (int size = KUtility.GetSize(value.Tags); j < size; j++)
			{
				TagData tagData = TagData.Create(value.Tags[j].Id, value.Tags[j].Level);
				if (tagData != null)
				{
					_tags.Add(tagData);
				}
			}
			if (value.Performance != null)
			{
				_performances.AddRange(value.Performance);
			}
		}
		else if (extension is BlueprintItem)
		{
			Blueprint = (BlueprintItem)extension;
		}
		else if (extension is LootBoxItem)
		{
			LootBox = (LootBoxItem)extension;
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ItemData itemData))
		{
			return false;
		}
		return itemData.Id == Id;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public ItemData GetContent(int index)
	{
		if (_content == null || index < 0 || index >= _content.Length)
		{
			return null;
		}
		return _content[index];
	}

	public Performance? GetPerformanceData(string performance)
	{
		int count = _performances.Count;
		for (int i = 0; i < count; i++)
		{
			Performance value = _performances[i];
			if (value.Id == performance)
			{
				return value;
			}
		}
		return null;
	}

	public TagData GetTagData(string tag)
	{
		int count = _tags.Count;
		for (int i = 0; i < count; i++)
		{
			TagData tagData = _tags[i];
			if (tagData.Id == tag)
			{
				return tagData;
			}
		}
		return null;
	}

	public bool HasTag(string tag)
	{
		Stack<ItemData> stack = new Stack<ItemData>();
		stack.Push(this);
		while (stack.Count > 0)
		{
			ItemData itemData = stack.Pop();
			if (itemData.GetTagData(tag) != null)
			{
				return true;
			}
			int contentCount = itemData.ContentCount;
			for (int i = 0; i < contentCount; i++)
			{
				stack.Push(itemData._content[i]);
			}
		}
		return false;
	}

	public bool HasTag(OrTagFilter tagFilters, bool ignoreLevel = false)
	{
		if (ContentCount > 0 && ExistTagInContents(tagFilters, ignoreLevel))
		{
			return true;
		}
		return ExistTag(tagFilters, ignoreLevel);
	}

	public bool HasTagsAndMaterials(OrTagFilter requiredTags, OrTagFilter requiredMaterials, bool ignoreLevel = false)
	{
		bool flag = requiredTags.Count <= 0 || HasTagInContents(requiredTags, ignoreLevel);
		bool flag2 = requiredMaterials.Count <= 0 || HasTagInContents(requiredMaterials, ignoreLevel);
		return flag && flag2;
	}

	public bool HasAttribute(string attr)
	{
		Stack<ItemData> stack = new Stack<ItemData>();
		stack.Push(this);
		while (stack.Count > 0)
		{
			ItemData itemData = stack.Pop();
			if (HasAttr(attr))
			{
				return true;
			}
			int contentCount = itemData.ContentCount;
			for (int i = 0; i < contentCount; i++)
			{
				stack.Push(_content[i]);
			}
		}
		return false;
	}

	public bool HasAttribute(string key, string value)
	{
		Stack<ItemData> stack = new Stack<ItemData>();
		stack.Push(this);
		while (stack.Count > 0)
		{
			ItemData itemData = stack.Pop();
			if (HasAttr(key, value))
			{
				return true;
			}
			int contentCount = itemData.ContentCount;
			for (int i = 0; i < contentCount; i++)
			{
				stack.Push(_content[i]);
			}
		}
		return false;
	}

	public bool HasAttribute(IList<KeyValuePair<string, string>> keyValues)
	{
		if (keyValues == null)
		{
			return true;
		}
		int count = keyValues.Count;
		for (int i = 0; i < count; i++)
		{
			KeyValuePair<string, string> keyValuePair = keyValues[i];
			if (HasAttribute(keyValuePair.Key, keyValuePair.Value))
			{
				return true;
			}
		}
		return false;
	}

	public string GetStringAttribute(string performance, [NotNull] string key)
	{
		Performance? performanceData = GetPerformanceData(performance);
		if (!performanceData.HasValue)
		{
			return null;
		}
		if (performanceData.Value.Strs == null)
		{
			return null;
		}
		return performanceData.Value.Strs.Get(key);
	}

	public string GetStringAttribute([NotNull] string key)
	{
		int count = _performances.Count;
		for (int i = 0; i < count; i++)
		{
			Performance performance = _performances[i];
			if (performance.Strs != null && performance.Strs.TryGetValue(key, out var value))
			{
				return value;
			}
		}
		return null;
	}

	public float GetFloatAttribute(string performance, [NotNull] string key)
	{
		Performance? performanceData = GetPerformanceData(performance);
		if (!performanceData.HasValue)
		{
			return 0f;
		}
		if (performanceData.Value.Nums == null)
		{
			return 0f;
		}
		return performanceData.Value.Nums.Get(key, 0f);
	}

	public float GetFloatAttribute([NotNull] string key)
	{
		int count = _performances.Count;
		for (int i = 0; i < count; i++)
		{
			Performance performance = _performances[i];
			if (performance.Nums != null && performance.Nums.TryGetValue(key, out var value))
			{
				return value;
			}
		}
		return 0f;
	}

	public bool IsDyeable()
	{
		int size = KUtility.GetSize(_dyeables);
		for (int i = 0; i < size; i++)
		{
			if (_dyeables[i])
			{
				return true;
			}
		}
		return false;
	}

	public bool IsDyeable(ColorChannel channel)
	{
		int size = KUtility.GetSize(_dyeables);
		return channel >= ColorChannel.ColorR && (int)channel < size && _dyeables[(int)channel];
	}

	public bool IsDestroyed()
	{
		return Durability != null && Durability.Get() <= 0f;
	}

	public bool IsDomesticatedPet()
	{
		return Reins.HasValue && Reins.Value.Domesticated;
	}

	public bool CanImprint()
	{
		return Level <= GameSystem<SkillSystem>.Instance().GetCategoryLevel(Category.Survival);
	}

	public bool GetDurabilityState(out DurabilityState state, out float refreshPeriod)
	{
		if (Durability != null)
		{
			double currentTime = Gauge.CurrentTime;
			float num = Durability.RealMax();
			float num2 = Durability.Get(currentTime);
			state = ((num2 / num < 0.2f) ? ((!IsDestroyed()) ? DurabilityState.Warning : DurabilityState.Destroyed) : DurabilityState.Good);
			refreshPeriod = (float)(Durability.When(num * 0.2f) - currentTime);
			return true;
		}
		state = DurabilityState.Good;
		refreshPeriod = 0f;
		return false;
	}

	public override string ToString()
	{
		return $"{Name} ({Id})";
	}

	private void AllocatedIconSize(int size)
	{
		float f = Mathf.Sqrt(size);
		Point2 point = default(Point2);
		point.x = Mathf.Clamp(Mathf.FloorToInt(f), 1, 3);
		point.y = Mathf.Max(Mathf.CeilToInt((float)size / (float)point.x), 1);
		if (point.x == point.y)
		{
			Width = point.x;
			Height = point.y;
			return;
		}
		int num = ((Id != null) ? Id.GetHashCode() : 0);
		if (point.y > 3 || num % 2 == 0)
		{
			Width = point.x;
			Height = point.y;
		}
		else
		{
			Width = point.y;
			Height = point.x;
		}
	}

	private TagData GetSuitableTag(TagFilterBase.Tag tag, bool ignoreLevel)
	{
		int count = _tags.Count;
		for (int i = 0; i < count; i++)
		{
			TagData tagData = _tags[i];
			if (tagData.Id == tag.Id && (ignoreLevel || tagData.Level >= tag.Level))
			{
				return tagData;
			}
		}
		return null;
	}

	private bool ExistTag(OrTagFilter tagFilters, bool ignoreLevel = false)
	{
		if (tagFilters != null)
		{
			int count = tagFilters.Count;
			for (int i = 0; i < count; i++)
			{
				TagData suitableTag = GetSuitableTag(tagFilters[i], ignoreLevel);
				if (suitableTag != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ExistTagInContents(OrTagFilter tagFilters, bool ignoreLevel = false)
	{
		for (int i = 0; i < ContentCount; i++)
		{
			if (_content[i].ExistTag(tagFilters, ignoreLevel))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasTagInContents(OrTagFilter tagFilters, bool ignoreLevel = false)
	{
		return (ContentCount <= 0) ? ExistTag(tagFilters, ignoreLevel) : ExistTagInContents(tagFilters, ignoreLevel);
	}

	private bool HasAttr(string attr)
	{
		int count = _performances.Count;
		for (int i = 0; i < count; i++)
		{
			Performance performance = _performances[i];
			if (performance.Nums != null && performance.Nums.ContainsKey(attr))
			{
				return true;
			}
			if (performance.Strs != null && performance.Strs.ContainsKey(attr))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasAttr(string key, string value)
	{
		int count = _performances.Count;
		for (int i = 0; i < count; i++)
		{
			Performance performance = _performances[i];
			if (performance.Strs != null && performance.Strs.TryGetValue(key, out var value2) && value2 == value)
			{
				return true;
			}
		}
		return false;
	}
}
