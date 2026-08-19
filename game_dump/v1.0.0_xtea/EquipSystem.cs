using System;
using System.Collections.Generic;
using ItemSystem;
using K1Network;
using L10N;
using Messages;
using Shared.Ability;
using Yaml;

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

	public class PlayerEquipInfo
	{
		public float damages_pierce;

		public float damages_cut;

		public float damages_impact;

		public float accuracy;

		public string accuracy_type;

		public float range;

		public float charge_move_speed;

		public float charge_time_to_max;

		public float stamina_cost;

		public float attack_cooltime;

		public float critical;

		public float defenses_pierce;

		public float defenses_cut;

		public float defenses_impact;

		public float tempcorrection;

		public float movespeed;

		public float dodge;

		public float shield_ratio;

		public float Attack;

		public float Defenses;

		public float Accuracy;

		public PlayerEquipInfo(Dictionary<Derived, int> derivedAbilities = null)
		{
			Init();
			if (derivedAbilities != null)
			{
				if (derivedAbilities.TryGetValue(Derived.Attack, out var value))
				{
					Attack = value;
				}
				if (derivedAbilities.TryGetValue(Derived.Defense, out value))
				{
					Defenses = value;
				}
				if (derivedAbilities.TryGetValue(Derived.Accuracy, out value))
				{
					Accuracy = value;
				}
			}
		}

		public PlayerEquipInfo(PlayerEquipInfo info)
		{
			damages_cut = info.damages_cut;
			damages_impact = info.damages_impact;
			damages_pierce = info.damages_pierce;
			accuracy = info.accuracy;
			accuracy_type = info.accuracy_type;
			range = info.range;
			charge_move_speed = info.charge_move_speed;
			charge_time_to_max = info.charge_time_to_max;
			stamina_cost = info.stamina_cost;
			attack_cooltime = info.attack_cooltime;
			critical = info.critical;
			defenses_cut = info.defenses_cut;
			defenses_impact = info.defenses_impact;
			defenses_pierce = info.defenses_pierce;
			tempcorrection = info.tempcorrection;
			movespeed = info.movespeed;
			dodge = info.dodge;
			shield_ratio = info.shield_ratio;
			Attack = info.Attack;
			Defenses = info.Defenses;
			Accuracy = info.Accuracy;
		}

		public void Init()
		{
			damages_cut = 0f;
			damages_impact = 0f;
			damages_pierce = 0f;
			accuracy = 0f;
			accuracy_type = string.Empty;
			range = 0f;
			charge_move_speed = 0f;
			charge_time_to_max = 0f;
			stamina_cost = 0f;
			attack_cooltime = 0f;
			critical = 0f;
			defenses_cut = 0f;
			defenses_impact = 0f;
			defenses_pierce = 0f;
			tempcorrection = 0f;
			movespeed = 0f;
			dodge = 0f;
			shield_ratio = 0f;
			Attack = 0f;
			Defenses = 0f;
			Accuracy = 0f;
		}
	}

	private Dictionary<string, ItemData> _equipItems = new Dictionary<string, ItemData>();

	private Dictionary<string, ItemData> _bodyParts = new Dictionary<string, ItemData>();

	public ItemData Barehands { get; private set; }

	public Dictionary<string, ItemData> EquipItems => _equipItems;

	public ItemData Weapon
	{
		get
		{
			ItemData itemData = FindEquipItem("main", "both");
			return (itemData == null) ? Barehands : itemData;
		}
	}

	public ItemData Sub => FindEquipItem("sub");

	public ItemData Body
	{
		get
		{
			ItemData value = FindEquipItem("body", "hoody");
			if (value == null)
			{
				_bodyParts.TryGetValue("body", out value);
			}
			return value;
		}
	}

	public ItemData Head
	{
		get
		{
			ItemData value = FindEquipItem("head");
			if (value == null)
			{
				_bodyParts.TryGetValue("head", out value);
			}
			return value;
		}
	}

	public ItemData Shoes
	{
		get
		{
			ItemData value = FindEquipItem("shoes");
			if (value == null)
			{
				_bodyParts.TryGetValue("leg", out value);
			}
			return value;
		}
	}

	public ItemData Gloves
	{
		get
		{
			ItemData value = FindEquipItem("gloves");
			if (value == null)
			{
				_bodyParts.TryGetValue("arm", out value);
			}
			return value;
		}
	}

	public ItemData Bag => FindEquipItem("bag");

	public ItemData Precious => FindEquipItem("precious");

	public event Action<string, bool> OnRequestEquip;

	public event Action OnUpdateEquipments;

	private void Awake()
	{
		Connections.Frontend.On<Equipments>(EquipmentsReceived);
	}

	private void EquipmentsReceived(Equipments m, PacketHeader header)
	{
		_equipItems.Clear();
		foreach (KeyValuePair<string, Item> slot in m.Slots)
		{
			ItemData itemData = GameSystem<InventorySystem>.Instance().FindItem(slot.Value.Id);
			if (itemData == null)
			{
				itemData = new ItemData(slot.Value);
			}
			else
			{
				itemData.Set(slot.Value);
			}
			itemData.IsEquipments = true;
			_equipItems[slot.Key] = itemData;
		}
		if (this.OnUpdateEquipments != null)
		{
			this.OnUpdateEquipments();
		}
	}

	public void EquipItem(ItemData item)
	{
		if (item != null)
		{
			string stringAttribute = item.GetStringAttribute("slot");
			if (!string.IsNullOrEmpty(stringAttribute))
			{
				EquipItem(stringAttribute, (!item.IsEquipments) ? item : null);
			}
		}
	}

	public void EquipItem(Slot slot, ItemData item)
	{
		EquipItem(slot.ToString().ToLower(), item);
	}

	public void UnequipItem(Slot slot)
	{
		EquipItem(slot.ToString().ToLower(), null);
	}

	public void EquipItem(string slot, ItemData item)
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
				if (Body != null && Body.HasAttribute("slot", "hoody"))
				{
					UIManager.SystemMsg(T._("장비를 착용 할 수 없습니다"));
					return;
				}
			}
			else if (slot == "sub" && Weapon != null && Weapon.HasAttribute("slot", "both"))
			{
				UIManager.SystemMsg(T._("장비를 착용 할 수 없습니다"));
				return;
			}
			RequestEquipMsg(slot, item.Id, "equip");
		}
		else if (_equipItems.ContainsKey(slot))
		{
			RequestEquipMsg(slot, 0uL, "unequip");
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
			RequestEquipMsg(slot, 0uL, "unequip");
		}
		if (this.OnRequestEquip != null)
		{
			this.OnRequestEquip(slot, item != null);
		}
	}

	public PlayerEquipInfo GetEquipInfo()
	{
		ItemData[] weapons = new ItemData[1] { Weapon };
		ItemData[] defenses = new ItemData[4] { Head, Body, Shoes, Gloves };
		return GetEquipInfo(weapons, defenses);
	}

	private PlayerEquipInfo GetEquipInfo(IList<ItemData> weapons, IList<ItemData> defenses)
	{
		PlayerEquipInfo playerEquipInfo = new PlayerEquipInfo(GameSystem<StatisticsSystem>.Instance().DerivedAbilities);
		int count = weapons.Count;
		for (int i = 0; i < count; i++)
		{
			if (weapons[i] != null)
			{
				playerEquipInfo.damages_cut += weapons[i].GetFloatAttribute("damages.cut");
				playerEquipInfo.damages_impact += weapons[i].GetFloatAttribute("damages.impact");
				playerEquipInfo.damages_pierce += weapons[i].GetFloatAttribute("damages.pierce");
				playerEquipInfo.accuracy += weapons[i].GetFloatAttribute("accuracy");
				playerEquipInfo.critical += weapons[i].GetFloatAttribute("critical");
				playerEquipInfo.accuracy_type = weapons[i].GetStringAttribute("accuracy_type");
				playerEquipInfo.range = weapons[i].GetFloatAttribute("range");
			}
		}
		count = defenses.Count;
		for (int j = 0; j < count; j++)
		{
			if (defenses[j] != null)
			{
				playerEquipInfo.defenses_cut += defenses[j].GetFloatAttribute("defenses.cut");
				playerEquipInfo.defenses_impact += defenses[j].GetFloatAttribute("defenses.impact");
				playerEquipInfo.defenses_pierce += defenses[j].GetFloatAttribute("defenses.pierce");
				playerEquipInfo.dodge += defenses[j].GetFloatAttribute("dodge");
			}
		}
		playerEquipInfo.movespeed = KSingleton<PlayerController>.Instance().MoveSpeed;
		return playerEquipInfo;
	}

	private void RequestEquipMsg(string slot, ulong itemId, string action)
	{
		Equip msg = default(Equip);
		msg.SlotName = slot;
		msg.ItemId = itemId;
		msg.Action = action;
		Connections.Frontend.Send(msg);
	}

	public string IsEquipItem(ItemData item)
	{
		if (item != null)
		{
			foreach (KeyValuePair<string, ItemData> equipItem in _equipItems)
			{
				if (item.Id == equipItem.Value.Id)
				{
					return equipItem.Key;
				}
			}
		}
		return null;
	}

	public ItemData FindEquipItem(params Slot[] slots)
	{
		int num = slots.Length;
		ItemData itemData = null;
		for (int i = 0; i < num; i++)
		{
			itemData = FindEquipItem(slots[i].ToString().ToLower());
			if (itemData != null)
			{
				break;
			}
		}
		return itemData;
	}

	public ItemData FindEquipItem(params string[] slots)
	{
		int num = slots.Length;
		for (int i = 0; i < num; i++)
		{
			if (_equipItems.TryGetValue(slots[i], out var value) && value != null)
			{
				return value;
			}
		}
		return null;
	}

	public void InitBarehands(PlayerEntityContainer container)
	{
		PlayerEntity player = container.player;
		ItemData itemData = new ItemData();
		itemData.Id = 0uL;
		itemData.Name = "bare_hands";
		itemData.Icon = "weapon_bare_hands";
		itemData.PrototypeName = "bare_hands";
		itemData.Size = 0;
		TagData item = new TagData("bare_hands", 1, "bare_hands", "icon_empty");
		itemData.Tags.Add(item);
		Barehands bare_hands = player.bare_hands;
		PerformanceData performanceData = new PerformanceData();
		performanceData.id = "weapon";
		performanceData.name = "무기";
		performanceData.icon = "tag_purpose_weapon";
		performanceData.str_attrs.Add("attack_type", bare_hands.attack_type);
		performanceData.str_attrs.Add("accuracy_type", bare_hands.accuracy_type);
		performanceData.str_attrs.Add("weapon_framework", bare_hands.weapon_framework);
		performanceData.num_attrs.Add("attack_cooltime", bare_hands.attack_cooltime);
		performanceData.num_attrs.Add("range", bare_hands.range);
		performanceData.num_attrs.Add("critical", bare_hands.critical);
		performanceData.num_attrs.Add("accuracy_ratio", bare_hands.accuracy_ratio);
		itemData.Performances.Add(performanceData);
		_bodyParts.Clear();
		foreach (KeyValuePair<string, BodyParts> body_part in player.body_parts)
		{
			ItemData itemData2 = new ItemData();
			itemData2.Name = body_part.Key;
			itemData2.Icon = "icon_question";
			itemData2.PrototypeName = string.Empty;
			PerformanceData performanceData2 = new PerformanceData();
			performanceData2.id = "armor";
			performanceData2.name = "방어구";
			performanceData2.icon = "tag_property_equippable";
			foreach (KeyValuePair<string, float> item2 in body_part.Value.defense_ratio)
			{
				performanceData2.num_attrs.Add($"defenses.{item2.Key}", item2.Value);
			}
			performanceData2.num_attrs.Add("dodge_ratio", body_part.Value.dodge_ratio);
			performanceData2.num_attrs.Add("max_hp", body_part.Value.max_hp);
			_bodyParts.Add(body_part.Key, itemData2);
		}
		Barehands = itemData;
	}
}
