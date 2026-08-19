using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.Skill;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Shared.Ability;
using Shared.Item;

namespace Crafting;

public abstract class Recipe : CategoryItem
{
	public int Count;

	public bool DeductModifiableCount;

	public string Subcategory;

	public string Description;

	public CraftType Type;

	public int MinLevel;

	public int MaxLevel;

	public bool Entrusts;

	public float DurationWait;

	public Derived? RequiredAbility;

	public string RequiredRecipe;

	public OrTagFilter AllowedWorkbench;

	public OrTagFilter AllowedTool;

	public RecipeSlot[] Slots;

	private bool _isValidOwnerSkill;

	private Node _ownerSkill;

	public bool HasRequiredWorkbench => AllowedWorkbench.Length > 0;

	public bool HasRequiredTool => AllowedTool.Length > 0;

	public bool IsValidWorkbench([CanBeNull] Artifact workbench)
	{
		if (!HasRequiredWorkbench)
		{
			return true;
		}
		if (workbench == null || (workbench.Durability != null && workbench.Durability.Get() <= workbench.Durability.Min()))
		{
			return false;
		}
		int i = 0;
		for (int length = AllowedWorkbench.Length; i < length; i++)
		{
			TagFilterBase.Tag tag = AllowedWorkbench[i];
			TagData tag2 = workbench.GetTag(tag.Id);
			if (tag2 != null && tag2.Level >= tag.Level)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRequiredRecipe()
	{
		if (string.IsNullOrEmpty(RequiredRecipe))
		{
			return true;
		}
		Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(RequiredRecipe);
		if (recipe == null)
		{
			return false;
		}
		if (recipe.Available)
		{
			return recipe.HasRequiredRecipe();
		}
		return false;
	}

	public Node GetOwnerSkill()
	{
		if (_isValidOwnerSkill)
		{
			return _ownerSkill;
		}
		_isValidOwnerSkill = true;
		_ownerSkill = GameSystem<SkillSystem>.Instance().FindSkill(delegate(Node s)
		{
			int i = 0;
			for (int size = KUtility.GetSize(s.Rewards); i < size; i++)
			{
				Reward reward = s.Rewards[i];
				if (reward.RecipeIds != null && reward.RecipeIds.Contains(Id))
				{
					return true;
				}
			}
			return false;
		});
		return _ownerSkill;
	}
}
