using System;
using System.Collections.Generic;
using Messages;
using Shared.Item;
using UnityEngine;
using Yaml;

namespace ItemSystem;

public class ItemData : INewCheckerable
{
	public bool Valid;

	private readonly List<TagData> _tags = new List<TagData>();

	private readonly List<TagData> _tagModifications = new List<TagData>();

	private readonly List<PerformanceData> _performances = new List<PerformanceData>();

	private ItemData[] _content;

	private bool[] _dyeables;

	public IList<TagData> Tags => _tags;

	public IList<TagData> TagModifications => _tagModifications;

	public IList<PerformanceData> Performances => _performances;

	public int ContentCount => (_content != null) ? _content.Length : 0;

	public ulong Id { get; set; }

	public string Name { get; set; }

	public string Icon { get; set; }

	public string Description { get; private set; }

	public string RawPrototypename { get; set; }

	public string PrototypeName { get; set; }

	public int Level { get; private set; }

	public int ModifiableCount { get; private set; }

	public int Size { get; set; }

	public ulong FounderId { get; set; }

	public string FounderCategory { get; set; }

	public bool Like { get; set; }

	public Gauge Durability { get; private set; }

	public int EquipLevel { get; private set; }

	public bool IsEquipments { get; set; }

	public NewChecker NewChecker { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public ItemColor Colors { get; set; }

	public Reins Reins { get; private set; }

	public ArtifactCapsule Capsule { get; private set; }

	public ArtifactPackage ArtifactPackage { get; private set; }

	public ItemData()
	{
	}

	public ItemData(Item itemInfo)
	{
		Set(itemInfo);
	}

	public ItemData(ItemJson itemInfo)
	{
		Set(itemInfo);
	}

	public void Set(Item itemInfo)
	{
		_tags.Clear();
		_tagModifications.Clear();
		_performances.Clear();
		ulong id = Id;
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(itemInfo.Prototype, itemInfo.Level);
		RawPrototypename = itemInfo.Prototype;
		Name = itemInfo.Name;
		Icon = ((itemInfo.Icon != null) ? itemInfo.Icon : ((itemPrototype == null) ? "icon_question" : itemPrototype.icon));
		Description = ((itemInfo.Description != null) ? itemInfo.Description : ((itemPrototype == null) ? string.Empty : itemPrototype.description.ToString()));
		PrototypeName = ((itemPrototype == null) ? string.Empty : itemPrototype.name.ToString());
		Id = itemInfo.Id;
		Level = itemInfo.Level;
		ModifiableCount = itemInfo.ModifiableCount;
		Size = itemInfo.Size;
		Durability = itemInfo.Durability;
		EquipLevel = itemInfo.EquipLevel;
		IsEquipments = false;
		SetColors(itemInfo);
		SetDyeable(itemPrototype?.dyeables);
		FounderId = itemInfo.FounderId;
		FounderCategory = itemInfo.FounderCategory;
		if (NewChecker == null || id != Id)
		{
			NewChecker = new NewCheckerNode();
			NewChecker.Key = $"item:{Id}";
		}
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
		int num2 = itemInfo.Performance.Length;
		for (int k = 0; k < num2; k++)
		{
			Performance performance = itemInfo.Performance[k];
			PerformanceData item = new PerformanceData(performance);
			_performances.Add(item);
		}
		SetCargo(itemInfo.Cargo);
	}

	private void SetColors(Item itemInfo)
	{
		SetColors(new string[3] { itemInfo.ColorR, itemInfo.ColorG, itemInfo.ColorB });
	}

	private void SetColors(IList<string> cols)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		int num = KUtility.GetSize(cols);
		int num2 = num - 1;
		while (num2 >= 0 && string.IsNullOrEmpty(cols[num2]))
		{
			num--;
			num2--;
		}
		ItemColor colors;
		if (num == 0)
		{
			colors = default(ItemColor);
		}
		else
		{
			colors = new ItemColor(num);
			for (int i = 0; i < num; i++)
			{
				colors.SetColor(i, KUtility.ToColor(cols[i]));
			}
		}
		Colors = colors;
	}

	private void SetDyeable(IList<ColorChannel> dyeables)
	{
		int i = 0;
		for (int size = KUtility.GetSize(dyeables); i < size; i++)
		{
			ColorChannel colorChannel = dyeables[i];
			if (_dyeables == null)
			{
				_dyeables = new bool[Colors.Count];
			}
			int num = (int)colorChannel;
			if (num >= 0 && num < _dyeables.Length)
			{
				_dyeables[num] = true;
			}
		}
	}

	private void SetCargo(object cargo)
	{
		if (cargo is Container container)
		{
			_content = new ItemData[container.Contents.Length];
			for (int i = 0; i < _content.Length; i++)
			{
				_content[i] = new ItemData(container.Contents[i]);
			}
		}
		else if (cargo is Messages.Reins msg)
		{
			Reins = new Reins(msg);
		}
		else if (cargo is Messages.ArtifactCapsule capsule)
		{
			Capsule = new ArtifactCapsule(capsule);
		}
		else if (cargo is Messages.ArtifactPackage package)
		{
			ArtifactPackage = new ArtifactPackage(package);
		}
	}

	public void Set(ItemJson itemInfo)
	{
		_tags.Clear();
		_tagModifications.Clear();
		_performances.Clear();
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(itemInfo.prototype, itemInfo.level);
		RawPrototypename = itemInfo.prototype;
		Name = itemInfo.name;
		Icon = ((itemInfo.icon != null) ? itemInfo.icon : ((itemPrototype == null) ? "icon_question" : itemPrototype.icon));
		Description = itemInfo.description;
		PrototypeName = ((itemPrototype == null) ? string.Empty : itemPrototype.name.ToString());
		Id = itemInfo.id;
		Level = itemInfo.level;
		ModifiableCount = itemInfo.modifiable_count;
		Size = itemInfo.size;
		Durability = new Gauge(itemInfo.durability);
		EquipLevel = itemInfo.equip_level;
		IsEquipments = false;
		SetColors(new string[3] { itemInfo.color_r, itemInfo.color_g, itemInfo.color_b });
		FounderId = itemInfo.founder_id;
		FounderCategory = itemInfo.founder_category;
		NewChecker = new NewCheckerNode();
		NewChecker.Key = $"item:{Id}";
		AllocatedIconSize(Size);
		int count = itemInfo.tags.Count;
		for (int i = 0; i < count; i++)
		{
			TagJson tagJson = itemInfo.tags[i];
			TagData tagData = TagData.Create(tagJson.id, tagJson.level);
			if (tagData != null)
			{
				_tags.Add(tagData);
			}
		}
		int count2 = itemInfo.performance.Count;
		for (int j = 0; j < count2; j++)
		{
			PerformanceJson json = itemInfo.performance[j];
			PerformanceData item = new PerformanceData(json);
			_performances.Add(item);
		}
		CargoJson? cargo = itemInfo.cargo;
		if (!cargo.HasValue)
		{
			return;
		}
		CargoJson value = itemInfo.cargo.Value;
		switch (value.type)
		{
		case "Container":
		{
			_content = new ItemData[value.capacity];
			int k = 0;
			for (int count3 = value.items.Count; k < count3; k++)
			{
				_content[k] = new ItemData(value.items[k]);
			}
			break;
		}
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

	public PerformanceData GetPerformanceData(string performance)
	{
		int count = _performances.Count;
		for (int i = 0; i < count; i++)
		{
			PerformanceData performanceData = _performances[i];
			if (performanceData.id == performance)
			{
				return performanceData;
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

	public bool HasTag(IList<TagFilter> tagFilters, bool ignoreLevel = false)
	{
		if (ContentCount > 0 && ExistTagInContents(tagFilters, ignoreLevel))
		{
			return true;
		}
		return ExistTag(tagFilters, ignoreLevel);
	}

	public bool HasTagsAndMaterials(IList<TagFilter> requiredTags, IList<TagFilter> requiredMaterials, bool ignoreLevel = false)
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

	public string GetStringAttribute(string key)
	{
		int count = _performances.Count;
		for (int i = 0; i < count; i++)
		{
			PerformanceData performanceData = _performances[i];
			if (performanceData.str_attrs.TryGetValue(key, out var value))
			{
				return value;
			}
		}
		return null;
	}

	public float GetFloatAttribute(string key)
	{
		int count = _performances.Count;
		for (int i = 0; i < count; i++)
		{
			PerformanceData performanceData = _performances[i];
			if (performanceData.num_attrs.TryGetValue(key, out var value))
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

	public override string ToString()
	{
		return $"{Name} ({Id})";
	}

	private void AllocatedIconSize(int size)
	{
		float num = Mathf.Pow((float)size, 0.5f);
		Point2 one = Point2.one;
		one.x = Mathf.Max(Mathf.FloorToInt(num), 1);
		one.y = Mathf.Max(Mathf.CeilToInt((float)size / (float)one.x), 1);
		if (one.x == one.y)
		{
			Width = one.x;
			Height = one.y;
			return;
		}
		Random random = new Random(Id.GetHashCode());
		if (random.Next(2) == 0)
		{
			Width = one.y;
			Height = one.x;
		}
		else
		{
			Width = one.x;
			Height = one.y;
		}
	}

	private TagData GetSuitableTag(TagFilter tagFilter, bool ignoreLevel)
	{
		int count = _tags.Count;
		for (int i = 0; i < count; i++)
		{
			TagData tagData = _tags[i];
			if (tagData.Id == tagFilter.TagId && (ignoreLevel || tagData.Level >= tagFilter.RequiredLevel))
			{
				return tagData;
			}
		}
		return null;
	}

	private bool ExistTag(IList<TagFilter> tagFilters, bool ignoreLevel = false)
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

	private bool ExistTagInContents(IList<TagFilter> tagFilters, bool ignoreLevel = false)
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

	private bool HasTagInContents(IList<TagFilter> tagFilters, bool ignoreLevel = false)
	{
		return (ContentCount <= 0) ? ExistTag(tagFilters, ignoreLevel) : ExistTagInContents(tagFilters, ignoreLevel);
	}

	private bool HasAttr(string attr)
	{
		int count = _performances.Count;
		for (int i = 0; i < count; i++)
		{
			PerformanceData performanceData = _performances[i];
			if (performanceData.num_attrs.ContainsKey(attr))
			{
				return true;
			}
			if (performanceData.str_attrs.ContainsKey(attr))
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
			PerformanceData performanceData = _performances[i];
			if (performanceData.str_attrs.TryGetValue(key, out var value2) && value2 == value)
			{
				return true;
			}
		}
		return false;
	}
}
