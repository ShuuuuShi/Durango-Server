using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Utils.Extensions;
using Shared.Item;
using Shared.Purchaser;
using Yaml;

namespace Durango.Logic.Social;

public class Motion : EmotionBase, IComparable<Motion>
{
	public const string Icon = "icon_emotionbook";

	public readonly string[] MotionNames;

	public readonly string Name;

	public readonly EmotionTier Tier;

	public readonly int PaybackMileage;

	private bool? _byEquipments;

	public bool IsRare => Tier == EmotionTier.A;

	public Motion(string key, Yaml.Motion value)
		: base(key, value.Free, value.Available)
	{
		MotionNames = value.MotionNames;
		Name = value.Name;
		Tier = value.Tier;
		PaybackMileage = value.PaybackMileage;
	}

	public int CompareTo(Motion other)
	{
		if (other == null)
		{
			return -1;
		}
		bool flag = IsEquipmentsMotion();
		bool flag2 = other.IsEquipmentsMotion();
		if (flag != flag2)
		{
			return (!flag) ? 1 : (-1);
		}
		if (base.Favorite != other.Favorite)
		{
			if (base.Favorite)
			{
				return -1;
			}
			return 1;
		}
		if (base.FavoriteIndex.HasValue != other.FavoriteIndex.HasValue)
		{
			if (base.FavoriteIndex.HasValue)
			{
				return -1;
			}
			return 1;
		}
		if (base.FavoriteIndex.HasValue && other.FavoriteIndex.HasValue)
		{
			int num = base.FavoriteIndex.Value - other.FavoriteIndex.Value;
			if (num != 0)
			{
				return num;
			}
		}
		int num2 = Tier - other.Tier;
		if (num2 != 0)
		{
			return num2;
		}
		return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
	}

	public bool IsEquipmentsMotion()
	{
		bool? byEquipments = _byEquipments;
		if (!byEquipments.HasValue)
		{
			_byEquipments = IsEquipmentsMotion(GameSystem<EquipSystem>.Instance().GetEquipPreset(GameSystem<EquipSystem>.Instance().CurrentEquipPreset)) || IsEquipmentsMotion(GameSystem<EquipSystem>.Instance().GetEquipPreset(EquipSlotType.Avatar));
		}
		return _byEquipments.Value;
	}

	private bool IsEquipmentsMotion(EquipSystem.EquipPreset preset)
	{
		if (preset == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, string> slotItem in preset.SlotItems)
		{
			ItemData itemData = GameSystem<InventorySystem>.Instance().FindItem(slotItem.Value);
			if (itemData == null || itemData.EmotionalMotions == null || itemData.EmotionalMotions.IndexOf(Key) == -1)
			{
				continue;
			}
			return true;
		}
		return false;
	}

	protected override void OnDirty()
	{
		base.OnDirty();
		_byEquipments = null;
	}

	public override bool IsSubscribe()
	{
		return base.IsSubscribe() || IsEquipmentsMotion();
	}
}
