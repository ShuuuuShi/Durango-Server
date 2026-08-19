using System.Collections.Generic;
using ItemSystem;
using NCalc;
using Yaml;

namespace Building_;

public class BlueprintSlot : IItemSlot
{
	public string Id;

	public string Name;

	public int RequiredCount;

	public Expression SizeFactor;

	public TagFilter[] RequiredTags;

	public TagFilter[] RequiredMaterials;

	public Dictionary<string, ArtifactLook> Looks;

	public bool HasLook => Looks != null && Looks.Count > 0;

	public string LocalizedName
	{
		get
		{
			if (string.IsNullOrEmpty(Name))
			{
				return LocalizeSystem.Get("#recipe_slot_" + Id);
			}
			return Name;
		}
	}

	public bool IsSuitableItem(ItemData itemData, bool ignoreLevel = false)
	{
		return !itemData.IsEquipments && itemData.HasTagsAndMaterials(RequiredTags, RequiredMaterials, ignoreLevel);
	}

	public override string ToString()
	{
		return LocalizedName + " " + LocalizeSystem.Format("#craft_require_count", RequiredCount.ToString());
	}
}
