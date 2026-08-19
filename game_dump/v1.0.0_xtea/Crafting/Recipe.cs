using ItemSystem;
using Shared.Item;
using UnityEngine;

namespace Crafting;

public abstract class Recipe : CategoryItem
{
	public string Subcategory;

	public string Description;

	public CraftType Type;

	public int MinLevel;

	public int MaxLevel;

	public bool Entrusts;

	public float DurationWait;

	public TagFilter[] RequiredWorkbenches;

	public TagFilter[] ToolTags;

	public RecipeSlot[] Slots;

	public override string LocalizedName
	{
		get
		{
			if (string.IsNullOrEmpty(Name))
			{
				return LocalizeSystem.Get("#craft_" + Id);
			}
			return Name;
		}
	}

	public bool WorkbenchRequired => RequiredWorkbenches.Length > 0;

	public bool ToolRequired => ToolTags.Length > 0;

	public bool IsvalidWorkbench(Artifact workbench)
	{
		if (!WorkbenchRequired)
		{
			return true;
		}
		if ((Object)(object)workbench != (Object)null)
		{
			if (workbench.Durability.Get() <= workbench.Durability.Min())
			{
				return false;
			}
			int i = 0;
			for (int num = RequiredWorkbenches.Length; i < num; i++)
			{
				TagFilter tagFilter = RequiredWorkbenches[i];
				TagData tag = workbench.GetTag(tagFilter.TagId);
				if (tag != null)
				{
					return true;
				}
			}
		}
		return false;
	}
}
