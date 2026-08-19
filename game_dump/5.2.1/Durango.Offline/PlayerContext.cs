using System;
using System.Collections.Generic;
using System.IO;
using Durango.Logic.Clusters;
using Durango.Logic.Encyclopedia;
using Durango.UI;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Newtonsoft.Json;
using Shared.Player;
using Shared.Skill;
using UnityEngine;

namespace Durango.Offline;

public class PlayerContext
{
	[JsonProperty("player_slot")]
	public int PlayerSlot;

	[JsonProperty("appear_player")]
	public AppearPlayer AppearPlayer;

	[JsonProperty("player_info")]
	public Durango.Logic.Clusters.PlayerInfo PlayerInfo;

	[JsonProperty("inventory_items")]
	public List<Item> InventoryItems;

	[JsonProperty("equipped_items")]
	public Dictionary<string, string> EquippedItems;

	[JsonProperty("musics")]
	public Dictionary<int, Music> Musics;

	[JsonProperty("storage")]
	public Dictionary<string, byte[]> Storage;

	[JsonProperty("skills")]
	public Dictionary<Category, SkillCategory> Skills;

	[JsonProperty("known_skills")]
	public List<SkillBundle> KnownSkills;

	[JsonProperty("skill_points")]
	public int SkillPoints;

	[JsonProperty("craftest")]
	public Craft Craftest;

	[JsonProperty("active_pet")]
	public AppearPet ActivePet;

	[JsonProperty("position")]
	public WorldPosition Position;

	[JsonProperty("skill_list")]
	public List<Skill> SkillList;

	[JsonProperty("pet_list")]
	public List<Pet> PetList;

	[JsonProperty("pet_inventories")]
	public Dictionary<string, List<Item>> PetInventories;

	[JsonProperty("wallet")]
	public Wallet Wallet;

	[JsonProperty("is_connected_kyllox_server")]
	public bool IsConnectedKylloxServer;

	[JsonIgnore]
	public string Path { get; private set; }

	[JsonIgnore]
	public string EntityId
	{
		get
		{
			if (PlayerInfo != null)
			{
				return PlayerInfo.PlayerEntityId;
			}
			return string.Empty;
		}
	}

	public void Initialize(string path)
	{
		Path = path;
		if (PlayerInfo == null)
		{
			string text = Guid.NewGuid().ToString();
			string titleId = Guid.NewGuid().ToString();
			PlayerInfo = new Durango.Logic.Clusters.PlayerInfo();
			if (GameManager.ClusterMode == Mode.SingleMode)
			{
				PlayerInfo.PlayerLevel = 1;
				PlayerInfo.PlayerEntityId = text;
				PlayerInfo.PlayerName = text.Substring(0, 8);
				AppearPlayer.Name = PlayerInfo.PlayerName;
				AppearPlayer.Level = PlayerInfo.PlayerLevel;
				AppearPlayer.EntityId = text;
				AppearPlayer.IsAlive = true;
				AppearPlayer.Title.EntityId = text;
				AppearPlayer.Title.TitleId = string.Empty;
				AppearPlayer.Title._Title = string.Empty;
				AppearPlayer.Member.EntityId = text;
				AppearPlayer.Member.ClanId = string.Empty;
				AppearPlayer.Member.ClanName = string.Empty;
				AppearPlayer.Member.RoleId = 0;
				AppearPlayer.Move.EntityId = text;
				AppearPlayer.Survival.EntityId = text;
			}
			else
			{
				PlayerInfo.PlayerLevel = 60;
				PlayerInfo.PlayerEntityId = text;
				PlayerInfo.PlayerName = text.Substring(0, 8);
				AppearPlayer.Name = PlayerInfo.PlayerName;
				AppearPlayer.Level = PlayerInfo.PlayerLevel;
				AppearPlayer.EntityId = text;
				AppearPlayer.IsAlive = true;
				AppearPlayer.Title.EntityId = text;
				AppearPlayer.Title.TitleId = titleId;
				AppearPlayer.Title._Title = "영원한 개척자";
			}
			AppearPlayer.Member.EntityId = text;
			AppearPlayer.Member.ClanId = string.Empty;
			AppearPlayer.Member.ClanName = string.Empty;
			AppearPlayer.Member.RoleId = 0;
			AppearPlayer.Move.EntityId = text;
			AppearPlayer.Survival.EntityId = text;
			Gauge life = new Gauge(1125f, 0f, new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = 1125f
				}
			});
			AppearPlayer.Survival.Life = life;
			Gauge value = new Gauge(11250f, 0f, new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = 11250f
				}
			});
			AppearPlayer.Survival.Gauges = new Dictionary<string, Gauge>();
			AppearPlayer.Survival.Gauges.Add("stamina", value);
			Gauge value2 = new Gauge(100f, 0f, new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = 0f
				}
			});
			AppearPlayer.Survival.Gauges.Add("fatigue", value2);
			Job[] array = Enums<Job>.Greater(Job.Invalid);
			Job job = array[UnityEngine.Random.Range(0, array.Length)];
			bool flag = UnityEngine.Random.Range(0, 2) == 1;
			EditPlayerDisplayProxy.FillRandomPlayerDisplayData(flag, job, ref AppearPlayer.Display);
			AppearPlayer.Display.DefaultBody = ((!flag) ? "Models/PC/Female/Body/f_body_nothing.FBX" : "Models/PC/Male/Body/m_body_nothing.FBX");
			AppearPlayer.Display.DefaultInner = ((!flag) ? "Models/PC/Female/Inner/f_inner_basic.FBX" : "Models/PC/Male/Inner/m_inner_basic.FBX");
			AppearPlayer.Display.Body = AppearPlayer.Display.DefaultBody;
			AppearPlayer.Display.EntityId = text;
		}
		if (InventoryItems == null)
		{
			InventoryItems = new List<Item>();
		}
		if (SkillPoints == 0)
		{
			SkillPoints = 777;
		}
		if (KnownSkills == null)
		{
			KnownSkills = new List<SkillBundle>();
		}
		if (Skills == null)
		{
			Skills = new Dictionary<Category, SkillCategory>();
			SkillCategory value3 = default(SkillCategory);
			value3.Exp = 0;
			value3.Level = 60;
			Skills.Add(Category.Armorcrafting, value3);
			Skills.Add(Category.Butchery, value3);
			Skills.Add(Category.Constructing, value3);
			Skills.Add(Category.Cooking, value3);
			Skills.Add(Category.Defense, value3);
			Skills.Add(Category.Farming, value3);
			Skills.Add(Category.Gathering, value3);
			Skills.Add(Category.MeleeCombat, value3);
			Skills.Add(Category.Process, value3);
			Skills.Add(Category.RangedCombat, value3);
			Skills.Add(Category.Survival, value3);
			Skills.Add(Category.Weaponcrafting, value3);
		}
		if (SkillList == null)
		{
			SkillList = new List<Skill>();
		}
		if (EquippedItems == null)
		{
			EquippedItems = new Dictionary<string, string>();
		}
		if (PetList == null)
		{
			PetList = new List<Pet>();
		}
		if (PetInventories == null)
		{
			PetInventories = new Dictionary<string, List<Item>>();
		}
		if (KUtility.GetSize(Storage) != 0)
		{
			return;
		}
		Storage = new Dictionary<string, byte[]>();
		MemoSystem.EncyclopediaStorage data = default(MemoSystem.EncyclopediaStorage);
		data.Memo.Memos = new List<KeyValuePair<MemoType, List<int>>>();
		List<int> list = new List<int>();
		for (int i = 0; i <= 227; i++)
		{
			if (!string.IsNullOrEmpty(MemoSystem.GetMemoText(MemoType.Tooltip, i)))
			{
				list.Add(i);
			}
		}
		data.Memo.Memos.Add(new KeyValuePair<MemoType, List<int>>(MemoType.Tooltip, list));
		list = new List<int>();
		for (int j = 1; j <= 243; j++)
		{
			if (!string.IsNullOrEmpty(MemoSystem.GetMemoText(MemoType.Fiction, j)))
			{
				list.Add(j);
			}
		}
		data.Memo.Memos.Add(new KeyValuePair<MemoType, List<int>>(MemoType.Fiction, list));
		Storage["encyclopedia"] = Json.WriteToBytes(data);
	}

	[CanBeNull]
	public static PlayerContext Load(string path)
	{
		PlayerContext playerContext = null;
		try
		{
			playerContext = Json.Read<PlayerContext>(File.ReadAllBytes(path));
			if (playerContext == null)
			{
				return null;
			}
			playerContext.Initialize(path);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return playerContext;
	}

	public void Save()
	{
		if (string.IsNullOrEmpty(Path))
		{
			return;
		}
		try
		{
			byte[] bytes = Json.WriteToBytes(this, indented: true);
			File.WriteAllBytes(Path, bytes);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static string MakePath(int slot, string clusterKey)
	{
		return global::System.IO.Path.Combine(AppData.CombinePath(WorldContext.GetBasePath(clusterKey)), slot + ".player");
	}
}
