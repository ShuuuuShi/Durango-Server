using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Item;
using UnityEngine;

public class EquipSystem : GameSystem<EquipSystem>
{
	public enum Slot
	{
		Invalid = -1,
		Precious = 0,
		Head = 1,
		Main = 3,
		Body = 4,
		Sub = 5,
		Gloves = 6,
		Shoes = 7,
		Bag = 8
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SlotComparer : IEqualityComparer<Slot>
	{
		public bool Equals(Slot x, Slot y)
		{
			return x == y;
		}

		public int GetHashCode(Slot x)
		{
			return (int)x;
		}
	}

	public class EquipPreset
	{
		public bool IsHidden;

		public bool IsLocked;

		public string TitleId;

		public double? UnlockSince;

		public double? UnlockUntil;

		[NotNull]
		public readonly Dictionary<string, string> SlotItems = new Dictionary<string, string>();
	}

	public const string AttrForAvatarEquip = "equipment_avatar";

	private readonly Dictionary<EquipSlotType, EquipPreset> _equipPresets = new Dictionary<EquipSlotType, EquipPreset>();

	private readonly List<string> _attachableAccessories = new List<string>();

	public EquipSlotType CurrentEquipPreset { get; private set; }

	public IEnumerable<string> AttachableAccessories => _attachableAccessories;

	public event Action<string, bool> EquipRequested;

	public event Action EquipmentsUpdated;

	public event Action ChangePresetSucceeded;

	public event Action AttachableAccessoriesChanged;

	private void Awake()
	{
		foreach (EquipSlotType item in EnumerateEquipPresetTypes(includeAvatar: true))
		{
			_equipPresets[item] = new EquipPreset();
		}
		CurrentEquipPreset = EquipSlotType.Invalid;
		Connections.Frontend.On<Equipments>(EquipmentsReceived);
		Connections.Frontend.On<AttachableAccessories>(OnAttachableAccessories);
		Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			Connections.Frontend.Send(default(GetAttachableAccessories));
		});
	}

	public EquipPreset GetEquipPreset(EquipSlotType presetType)
	{
		return _equipPresets.Get(presetType);
	}

	public bool IsLockedPreset(EquipSlotType presetType)
	{
		return GetEquipPreset(presetType)?.IsLocked ?? false;
	}

	public float GetPresetRemainRatio(EquipSlotType presetType)
	{
		EquipPreset equipPreset = GetEquipPreset(presetType);
		if (equipPreset != null)
		{
			double? unlockSince = equipPreset.UnlockSince;
			if (unlockSince.HasValue)
			{
				double? unlockUntil = equipPreset.UnlockUntil;
				if (unlockUntil.HasValue)
				{
					double num = equipPreset.UnlockUntil.Value - equipPreset.UnlockSince.Value;
					double num2 = equipPreset.UnlockUntil.Value - Connections.Frontend.GetPredictedServerTime();
					double num3 = num2 / num;
					return Mathf.Clamp01((float)num3);
				}
			}
		}
		return -1f;
	}

	public double GetPresetRemainTime(EquipSlotType presetType)
	{
		EquipPreset equipPreset = GetEquipPreset(presetType);
		if (equipPreset != null)
		{
			double? unlockSince = equipPreset.UnlockSince;
			if (unlockSince.HasValue)
			{
				double? unlockUntil = equipPreset.UnlockUntil;
				if (unlockUntil.HasValue)
				{
					return equipPreset.UnlockUntil.Value - Connections.Frontend.GetPredictedServerTime();
				}
			}
		}
		return 0.0;
	}

	public DurabilityState GetDurabilityState(EquipSlotType presetType)
	{
		DurabilityState durabilityState = DurabilityState.Good;
		EquipPreset equipPreset = GetEquipPreset(presetType);
		if (equipPreset != null)
		{
			foreach (KeyValuePair<string, string> slotItem in equipPreset.SlotItems)
			{
				ItemData itemData = GameSystem<InventorySystem>.Instance().FindItem(slotItem.Value);
				if (itemData != null && itemData.GetDurabilityState(out var state, out var _) && durabilityState < state)
				{
					durabilityState = state;
				}
			}
		}
		return durabilityState;
	}

	public string GetCurrentTitleId()
	{
		return GetEquipPreset(CurrentEquipPreset)?.TitleId;
	}

	public void EquipItem(ItemData item)
	{
		if (item != null)
		{
			string stringAttribute = item.GetStringAttribute("slot");
			if (!string.IsNullOrEmpty(stringAttribute))
			{
				EquipItem((!item.HasTag("equipment_avatar")) ? CurrentEquipPreset : EquipSlotType.Avatar, stringAttribute, (!item.IsEquipments) ? item : null);
			}
		}
	}

	public void EquipItem(EquipSlotType presetType, string slot, ItemData item, Action onReply = null)
	{
		if (item != null)
		{
			if (slot == "main" && item.HasAttribute("slot", "both"))
			{
				slot = "both";
			}
			else if (slot == "body" && item.HasAttribute("slot", "hoody"))
			{
				slot = "hoody";
			}
			if (slot == "head")
			{
				ItemData body = GetBody(presetType);
				if (body != null && body.HasAttribute("slot", "hoody"))
				{
					UIManager.SystemMsg(T._("장비를 착용할 수 없습니다"));
					return;
				}
			}
			else if (slot == "sub")
			{
				ItemData weapon = GetWeapon(presetType);
				if (weapon != null && weapon.HasAttribute("slot", "both"))
				{
					UIManager.SystemMsg(T._("장비를 착용할 수 없습니다"));
					return;
				}
			}
			RequestEquipMsg(presetType, slot, item.Id, onReply);
		}
		else
		{
			EquipPreset equipPreset = GetEquipPreset(presetType);
			if (equipPreset == null)
			{
				return;
			}
			if (equipPreset.SlotItems.ContainsKey(slot))
			{
				RequestEquipMsg(presetType, slot, string.Empty, onReply);
			}
			else
			{
				if (slot == "main")
				{
					slot = "both";
				}
				else if (slot == "body")
				{
					slot = "hoody";
				}
				RequestEquipMsg(presetType, slot, string.Empty, onReply);
			}
		}
		if (this.EquipRequested != null)
		{
			this.EquipRequested(slot, item != null);
		}
	}

	public void ChangePreset(EquipSlotType presetType, [CanBeNull] Action onReply = null)
	{
		Connections.Frontend.Send(new ChangeEquipSlotType
		{
			SlotType = presetType
		}).On<OK>(delegate
		{
			CurrentEquipPreset = presetType;
			GameSystem<InventorySystem>.Instance().UpdateEquipments(CurrentEquipPreset);
			SoundManager.PlayEvent("ui_menu_equip_item_equip_on");
			if (onReply != null)
			{
				onReply();
			}
			if (this.ChangePresetSucceeded != null)
			{
				this.ChangePresetSucceeded();
			}
		}).Rest(delegate
		{
			if (onReply != null)
			{
				onReply();
			}
		});
	}

	public void AttachAccessory(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			Connections.Frontend.Send(default(ResetAccessory));
			return;
		}
		Connections.Frontend.Send(new AttachAccessory
		{
			AccessoryId = id
		});
	}

	public bool IsEquippedItem([NotNull] ItemData item)
	{
		EquipPreset equipPreset = GetEquipPreset((!item.HasTag("equipment_avatar")) ? CurrentEquipPreset : EquipSlotType.Avatar);
		if (equipPreset == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, string> slotItem in equipPreset.SlotItems)
		{
			if (item.Id == slotItem.Value)
			{
				return true;
			}
		}
		return false;
	}

	public ItemData FindEquippedItem(EquipSlotType presetType, params string[] slots)
	{
		EquipPreset equipPreset = GetEquipPreset(presetType);
		if (equipPreset == null)
		{
			return null;
		}
		int size = KUtility.GetSize(slots);
		for (int i = 0; i < size; i++)
		{
			string itemid = equipPreset.SlotItems.Get(slots[i]);
			ItemData itemData = GameSystem<InventorySystem>.Instance().FindItem(itemid);
			if (itemData != null)
			{
				return itemData;
			}
		}
		return null;
	}

	public static IEnumerable<EquipSlotType> EnumerateEquipPresetTypes(bool includeAvatar = false)
	{
		EquipSlotType[] source = Enums<EquipSlotType>.All();
		return (!includeAvatar) ? source.Where((EquipSlotType type) => type != EquipSlotType.Invalid && type != EquipSlotType.Avatar) : source.Where((EquipSlotType type) => type != EquipSlotType.Invalid);
	}

	[CanBeNull]
	private ItemData GetWeapon(EquipSlotType presetType)
	{
		ItemData itemData = FindEquippedItem(presetType, "main", "both");
		return (itemData == null) ? null : itemData;
	}

	[CanBeNull]
	private ItemData GetBody(EquipSlotType presetType)
	{
		return FindEquippedItem(presetType, "body", "hoody");
	}

	private void OnAttachableAccessories(AttachableAccessories msg, PacketHeader header)
	{
		_attachableAccessories.Clear();
		if (msg.Accessories != null)
		{
			_attachableAccessories.AddRange(msg.Accessories);
		}
		if (this.AttachableAccessoriesChanged != null)
		{
			this.AttachableAccessoriesChanged();
		}
	}

	private static void RequestEquipMsg(EquipSlotType presetType, string slot, string itemId, Action onReply)
	{
		if (presetType == EquipSlotType.Invalid)
		{
			return;
		}
		bool equip = !string.IsNullOrEmpty(itemId);
		Equip msg = default(Equip);
		msg.SlotName = slot;
		msg.SlotType = presetType;
		msg.ItemId = itemId;
		msg.Action = ((!equip) ? "unequip" : "equip");
		Connections.Frontend.Send(msg).All(delegate
		{
			SoundManager.PlayEvent((!equip) ? "ui_menu_equip_item_equip_off" : "ui_menu_equip_item_equip_on");
			if (onReply != null)
			{
				onReply();
			}
		});
	}

	private void EquipmentsReceived(Equipments msg, PacketHeader header)
	{
		CurrentEquipPreset = msg.CurrentType;
		foreach (KeyValuePair<EquipSlotType, EquipPreset> equipPreset in _equipPresets)
		{
			EquipSlotType key = equipPreset.Key;
			EquipPreset value = equipPreset.Value;
			if (!msg.Presets.ContainsKey(key))
			{
				value.IsHidden = true;
				continue;
			}
			EquipmentSlot equipmentSlot = msg.Presets.Get(key);
			value.IsHidden = false;
			value.IsLocked = equipmentSlot.IsLocked;
			value.TitleId = equipmentSlot.TitleId;
			value.UnlockSince = equipmentSlot.UnlockSince;
			value.UnlockUntil = equipmentSlot.UnlockUntil;
			value.SlotItems.Clear();
			foreach (KeyValuePair<string, Item> itemSlot in equipmentSlot.ItemSlots)
			{
				value.SlotItems[itemSlot.Key] = itemSlot.Value.Id;
			}
		}
		GameSystem<InventorySystem>.Instance().UpdateEquipments(msg.CurrentType);
		if (this.EquipmentsUpdated != null)
		{
			this.EquipmentsUpdated();
		}
	}
}
