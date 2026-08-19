using System.Collections.Generic;
using Durango.Logic.Clusters;
using Durango.Utils;
using JetBrains.Annotations;

namespace Durango.Logic.Item;

public static class Useable
{
	private static bool?[] _usableTypes;

	private static bool?[] UsableTypes
	{
		get
		{
			if (_usableTypes == null)
			{
				_usableTypes = new bool?[(int)(Enums<UseType>.Max() + 1)];
			}
			for (int i = 0; i < _usableTypes.Length; i++)
			{
				_usableTypes[i] = null;
			}
			return _usableTypes;
		}
	}

	private static void FillTagUsable(string tagId, bool?[] array)
	{
		if (tagId == null)
		{
			return;
		}
		switch (tagId.Length)
		{
		case 15:
		{
			char c = tagId[0];
			if (c != 'd')
			{
				if (c != 'g' || !(tagId == "gain_blueprints"))
				{
					break;
				}
				goto IL_0115;
			}
			if (!(tagId == "display_changes"))
			{
				break;
			}
			goto IL_010d;
		}
		case 12:
			switch (tagId[5])
			{
			default:
				return;
			case 'r':
				if (!(tagId == "gain_recipes"))
				{
					return;
				}
				break;
			case 'm':
				if (!(tagId == "gain_motions"))
				{
					return;
				}
				break;
			}
			goto IL_0115;
		case 7:
			if (tagId == "eatable")
			{
				AddType(array, UseType.Eat);
			}
			break;
		case 9:
			if (tagId == "drinkable")
			{
				AddType(array, UseType.Drink);
			}
			break;
		case 11:
			if (tagId == "skill_reset")
			{
				AddType(array, UseType.Ticket);
			}
			break;
		case 14:
			if (!(tagId == "gender_changes"))
			{
				break;
			}
			goto IL_010d;
		case 6:
			if (tagId == "usable")
			{
				AddType(array, UseType.Use);
			}
			break;
		case 5:
			if (tagId == "reins" && GameManager.ClusterMode != 0)
			{
				AddType(array, UseType.Grazing);
			}
			break;
		case 8:
		case 10:
		case 13:
			break;
			IL_010d:
			AddType(array, UseType.ChangeDisplay);
			break;
			IL_0115:
			AddType(array, UseType.GainRecipes);
			break;
		}
	}

	public static bool IsMultiUse(UseType type)
	{
		if (type == UseType.ResurrectionRewards || type == UseType.TakeOut || type == UseType.PutIn)
		{
			return true;
		}
		return false;
	}

	public static void FillUsable(List<UseType> result, List<ItemData> itemList, Inventory current, Inventory other, Inventory.InventoryMode mode)
	{
		result.Clear();
		if (KUtility.GetSize(itemList) == 0)
		{
			return;
		}
		bool?[] usableTypes = UsableTypes;
		for (int i = 0; i < itemList.Count; i++)
		{
			GetUsable(itemList[i], current, other, mode, usableTypes);
		}
		bool flag = itemList.Count > 1;
		for (int j = 0; j < usableTypes.Length; j++)
		{
			UseType useType = (UseType)j;
			if (usableTypes[j].HasValue && usableTypes[j].Value && (!flag || IsMultiUse(useType)))
			{
				result.Add(useType);
			}
		}
	}

	private static void GetUsable(ItemData item, [NotNull] Inventory current, [CanBeNull] Inventory other, Inventory.InventoryMode mode, bool?[] result)
	{
		if (item == null)
		{
			return;
		}
		if (other != null)
		{
			AddType(result, (current.Type != 0) ? UseType.TakeOut : UseType.PutIn);
		}
		switch (mode)
		{
		case Inventory.InventoryMode.Dead:
			AddType(result, UseType.ResurrectionRewards);
			return;
		case Inventory.InventoryMode.Exchange:
			return;
		}
		if (current.Type != 0)
		{
			return;
		}
		for (int i = 0; i < item.Tags.Count; i++)
		{
			FillTagUsable(item.Tags[i].Id, result);
		}
		if (GameManager.ClusterMode != Mode.Editable && PlayerBehavior.LocalPlayer != null && !string.IsNullOrEmpty(item.GetModel(PlayerBehavior.LocalPlayer.IsMale)))
		{
			AddType(result, UseType.Dye);
		}
		if (item.IsDomesticatedPet())
		{
			AddType(result, UseType.Imprint);
		}
		else if (item.Reins.HasValue)
		{
			AddType(result, UseType.Taming);
		}
		if (item.HasAttribute("slot"))
		{
			if (item.IsEquipments)
			{
				AddType(result, UseType.UnEquip);
			}
			else if (item.Durability != null && item.Durability.Ratio() > 0f)
			{
				AddType(result, UseType.Equip);
			}
		}
		if (item.Capsule.HasValue)
		{
			AddType(result, UseType.Place);
		}
		if (item.IsRepairable && item.Durability != null && item.Durability.Ratio() < 0.2f)
		{
			AddType(result, UseType.Repair);
		}
		if (item.Blueprint.HasValue)
		{
			AddType(result, UseType.Build);
		}
		if (item.LootBox.HasValue)
		{
			AddType(result, UseType.OpenBox);
		}
	}

	private static void AddType(bool?[] array, UseType type)
	{
		bool? flag = array[(int)type];
		if (!flag.HasValue)
		{
			ref bool? reference = ref array[(int)type];
			reference = true;
		}
	}
}
