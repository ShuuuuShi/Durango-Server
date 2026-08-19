using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using Building;
using Crafting;
using Durango.Logic;
using Durango.Logic.Clusters;
using Durango.Logic.Combat;
using Durango.Logic.Skill;
using Durango.Logic.Social;
using Durango.Network;
using Durango.Terrain;
using Durango.UI;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using InteractionData;
using L10N;
using Messages;
using Newtonsoft.Json;
using Shared.Ability;
using Shared.Animal;
using Shared.Battle;
using Shared.Building;
using Shared.Display;
using Shared.Economy;
using Shared.Etc;
using Shared.Item;
using Shared.Skill;
using Shared.Teleport;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Offline;

public class Player
{
	public class GenDict
	{
		[JsonProperty("player_slot")]
		public int PlayerSlot;

		[JsonProperty("item_gen_dict")]
		public Dictionary<string, List<Generator>> ItemGenDict;
	}

	private const string EpicCategory = "sunset";

	private readonly Connection _connection;

	public readonly World _world;

	public readonly PlayerContext _context;

	private int _centerX;

	private int _centerY;

	private readonly World.ChunkVisit[,] _chunkVisited;

	private readonly HashSet<string> _artifactSet;

	[CompilerGenerated]
	private static Func<IEnumerable<QuestToDo>, IEnumerable<QuestToDo>, IEnumerable<QuestToDo>> cache0;

	public Collectible _collectable;

	public List<Generator> _generators;

	public GenDict _genDict;

	public Dictionary<string, List<Generator>> _itemGenDict;

	public WorldContext _unstableIslandContext;

	public WorldContext _stableIslandContext;

	private int _attackCount;

	private Damaged _damagedMsg;

	private UseBattleAction _battleActionMsg;

	private int _fastAttackCount;

	private Damage _damageMsg;

	private string _victimId;

	private float _hitRatio = 99f;

	private float _criticalRatio = 20f;

	private float _attackDamage;

	private PerformanceYaml.Weapon _weaponPerformance;

	private global::System.Timers.Timer _sadismTimer;

	private bool _isAlreadySadism;

	private bool _isSadism;

	private bool _isBloodBurst;

	private Messages.StatusEffects _statusEffects;

	private List<Messages.StatusEffect> _statusList = new List<Messages.StatusEffect>();

	private global::System.Timers.Timer _bloodBurstTimer;

	private bool _isAlreadyBloodBurst;

	private List<Messages.Tag> _tagList;

	private bool _isUsingPunchMachine;

	[SerializeField]
	private SoundEventType _insertCoinAudio;

	[SerializeField]
	private SoundEventType _startGameAudio;

	private Artifact _punchMachineTarget;

	private int _firstFastAtk;

	private int _secondFastAtk;

	private int _lastFastAtk;

	private float _swordDamageMin;

	private float _swordDamageMax;

	private float _swordDamageCritical;

	private float _bowDamageMin;

	private float _bowDamageMax;

	private float _bowDamageCritical;

	private bool _isFastAttack;

	private Dictionary<Currency, long> _walletPaidBalances;

	[SerializeField]
	private long _punchingGamePrice = 100L;

	private ParticleType _dodgeEffect;

	private WildAnimalAI _WildAnimalAI;

	public static Player Instance;

	private string _targetEntityId;

	private int _curFastAttackCount;

	private global::System.Timers.Timer _fastAttackTimer;

	private global::System.Timers.Timer _blowTimer;

	public bool _isBlowing;

	private int _sadismLevel;

	private int _bloodBurstLevel;

	public string EntityId { get; private set; }

	public bool IsLocalPlayer { get; private set; }

	public event global::System.Action Closed;

	public event global::System.Action ContextChanged;

	public Player(string entityId, Connection connection, World world, PlayerContext context, bool isLocalPlayer)
	{
		_artifactSet = new HashSet<string>();
		EntityId = entityId;
		_connection = connection;
		_world = world;
		_context = context;
		Instance = this;
		IsLocalPlayer = isLocalPlayer;
		_genDict = new GenDict();
		if (_context.AppearPlayer.Move.Movements == null || !IsLocalPlayer)
		{
			_context.AppearPlayer.Move.Movements = new Movement[1];
			_context.AppearPlayer.Move.Movements[0].Path = new Location[1];
			_context.AppearPlayer.Move.Movements[0].Path[0].Position = GetEntryPosition();
		}
		_centerX = _world.NumChunksX / 2;
		_centerY = _world.NumChunksY / 2;
		_chunkVisited = new World.ChunkVisit[_world.NumChunksX, _world.NumChunksY];
		_world.ArtifactAppeared += World_ArtifactAppeared;
		_world.ArtifactDisappeared += World_ArtifactDisappeared;
		_world.PlayerAppeared += World_PlayerAppeared;
		_world.PlayerDisappeared += World_PlayerDisappeared;
		_world.ArtifactManager.ArtifactDisplayUpdated += delegate(ArtifactDisplay msg)
		{
			Send(msg);
		};
		_world.ArtifactManager.ArtifactStateUpdated += delegate(ArtifactState msg)
		{
			Send(msg);
		};
		_world.NaturalAdded += delegate(Point2 chunk, byte[] bytes)
		{
			Send(new GardenDiff
			{
				Chunk = chunk,
				_GardenDiff = bytes
			});
		};
		_connection.Recv<GetStatistics>(delegate
		{
			SendStatistics();
		});
		_connection.Recv(delegate(GetRoutes msg, PacketHeader header)
		{
			FindSailingRoute(msg);
		});
		_connection.Recv(delegate(LearnSkill msg, PacketHeader header)
		{
			LearnSkill(msg);
			Send(default(OK), header.ReplyOf);
		});
		_connection.Recv(delegate(UntrainSkill msg, PacketHeader header)
		{
			UnlearnSkill(msg);
			Send(default(OK), header.ReplyOf);
		});
		_connection.Recv<GetSkills>(delegate
		{
			SendSkills();
		});
		_connection.Recv(delegate(GetCollectible msg, PacketHeader header)
		{
			_collectable.Generators = _generators.ToArray();
			Send(_collectable, header.Seq);
		});
		_world.NaturalDestroyed += delegate(Point2 tile)
		{
			Send(new DisappearEntityOnTile
			{
				Tile = tile
			});
		};
		_connection.Recv(delegate(UseItem msg, PacketHeader header)
		{
			Send(default(OK), header.ReplyOf);
		});
		_connection.Recv(delegate(PutInItem msg, PacketHeader header)
		{
			ArtPutIn(msg);
			Send(default(OK), header.ReplyOf);
		});
		_connection.Recv(delegate(SetChunk msg, PacketHeader header)
		{
			SetCenterChunks(msg.Chunk.x, msg.Chunk.y);
		});
		_connection.Recv(delegate(GetPetsInfo msg, PacketHeader header)
		{
			PetsInfo(msg, header);
		});
		_connection.Recv(delegate(SpawnPet msg, PacketHeader header)
		{
			SummonPet(msg);
		});
		_connection.Recv(delegate(ReturnPet msg, PacketHeader header)
		{
			DismissPet(msg);
		});
		_connection.Recv<Mount>(delegate
		{
			Mount();
		});
		_connection.Recv<Unmount>(delegate
		{
			Dismount();
		});
		_connection.Recv(delegate(GetPetInventory msg, PacketHeader header)
		{
			GetPetInventories(msg);
		});
		_connection.Recv(delegate(PutInItemsIntoPet msg, PacketHeader header)
		{
			PutIntoPet(msg);
			Send(default(OK), header.ReplyOf);
		});
		_connection.Recv(delegate(TakeOutItemsFromPet msg, PacketHeader header)
		{
			TakeOutFromPet(msg);
			if (_world.ArtifactManager.TakeOutItems(msg.PetId, msg.ItemIds))
			{
				Send(default(OK), header.ReplyOf);
			}
			else
			{
				Send(default(Abort), header.ReplyOf);
			}
		});
		_connection.Recv(delegate(ReleasePet msg, PacketHeader header)
		{
			ReleasePet(msg, header.Seq);
		});
		_connection.Recv(delegate(GrazePets msg, PacketHeader header)
		{
			GrazePets(msg);
		});
		_connection.Recv(delegate(RenamePet msg, PacketHeader header)
		{
			RenamePet(msg, header.Seq);
		});
		_connection.Recv(delegate(Feeding msg, PacketHeader header)
		{
			FeedPet(msg);
		});
		_connection.Recv<MountAirBalloon>(delegate
		{
			MountAirBalloon();
		});
		_connection.Recv<UnmountAirBalloon>(delegate
		{
			DismountAirBalloon();
		});
		_connection.Recv<EstimateCraft>(delegate
		{
			_context.SkillPoints = 2000;
		});
		_connection.Recv(delegate(Craft msg, PacketHeader header)
		{
			CraftItems(msg);
		});
		_connection.Recv(delegate(GetInventory msg, PacketHeader header)
		{
			GetInventories(msg);
		});
		_connection.Recv(delegate(Cheat msg, PacketHeader header)
		{
			HandleCheatMsg(msg._Cheat, header.Seq);
		});
		_connection.Recv(delegate(Messages.Touch msg, PacketHeader header)
		{
			HandleTouchMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(DestructArtifact msg, PacketHeader header)
		{
			HandleDestructMsg(msg);
		});
		_connection.Recv<WarpToPort>(delegate
		{
			WarpToPort();
		});
		_connection.Recv(delegate(RestOn msg, PacketHeader header)
		{
			Send(default(OK), header.Seq);
			OnContextChanged();
		});
		_connection.Recv(delegate(Wash msg, PacketHeader header)
		{
			Send(new Messages.Timer
			{
				Duration = 5f
			}, header.Seq);
			OnContextChanged();
		});
		_connection.Recv(delegate(PlantSeed msg, PacketHeader header)
		{
			HandlePlantSeedMsg(msg);
		});
		_connection.Recv(delegate(ChargeEffect msg, PacketHeader header)
		{
			HandleChargeEffectMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(Scribble msg, PacketHeader header)
		{
			HandleScribbleMsg(msg);
		});
		_connection.Recv(delegate(DumpItems msg, PacketHeader header)
		{
			HandleDumpItemsMsg(msg);
		});
		_connection.Recv(delegate(Move msg, PacketHeader header)
		{
			_world.BroadCast(msg);
			HandleMoveMsg(msg.Movements);
		});
		_connection.Recv(delegate(GetAddOns msg, PacketHeader header)
		{
			HandleGetAddOnsMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(PlaceAddOns msg, PacketHeader header)
		{
			HandlePlaceAddOnsMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(Messages.Display msg, PacketHeader header)
		{
			HandleChangeDecorationMsg(msg);
		});
		_connection.Recv(delegate(Equip msg, PacketHeader header)
		{
			HandleEquipMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(UseBattleAction msg, PacketHeader header)
		{
			OnUseBattleAction(msg);
		});
		_connection.Recv(delegate(ExitBattle msg, PacketHeader header)
		{
			OnExitBattleMsg(msg);
		});
		_connection.Recv(delegate(SearchProducts msg, PacketHeader header)
		{
			HandleSearchProductsMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(GetFavoriteProducts msg, PacketHeader header)
		{
			HandleGetFavoriteProductsMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(BuyProduct msg, PacketHeader header)
		{
			HandleBuyProductMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(GetRecipes msg, PacketHeader header)
		{
			HandleGetRecipesMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(GetArtifactBlueprints msg, PacketHeader header)
		{
			HandleGetArtifactBlueprintsMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(ExtendFloor msg, PacketHeader header)
		{
			HandleExtendFloorMsg(msg);
		});
		_connection.Recv(delegate(GetMusics msg, PacketHeader header)
		{
			HandleGetMusicsMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(SaveMusicToSlot msg, PacketHeader header)
		{
			HandleSaveMusicToSlotMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(RemoveMusicFromSlot msg, PacketHeader header)
		{
			HandleRemoveMusicFromSlotMsg(msg, header.Seq);
		});
		_connection.Recv(delegate(PlayMusic msg, PacketHeader header)
		{
			HandlePlayMusicMsg(msg);
		});
		_connection.Recv(delegate(StopMusic msg, PacketHeader header)
		{
			HandleStopMusicMsg(msg);
		});
		_connection.Recv(delegate(ArtifactDisplay msg, PacketHeader header)
		{
			HandleArtifactDisplayMsg(msg);
		});
		_connection.Recv<GetAvailableEmotions>(delegate
		{
			List<Durango.Logic.Social.Motion> motions = GameSystem<SocialSystem>.Instance().Emotional.Motions;
			AvailableEmotions msg4 = default(AvailableEmotions);
			msg4.Motions = motions.Select((Durango.Logic.Social.Motion motion) => motion.Key).ToArray();
			List<Durango.Logic.Social.Emoticon> emoticons = GameSystem<SocialSystem>.Instance().Emotional.Emoticons;
			msg4.Emoticons = emoticons.Select((Durango.Logic.Social.Emoticon emo) => emo.Key).ToArray();
			Send(msg4);
		});
		_connection.Recv(delegate(SayInExclusiveChannel msg, PacketHeader header)
		{
			Message_ message = msg.Message;
			message.Speaker = new RadioId
			{
				Name = _context.AppearPlayer.Name,
				Freq = _context.AppearPlayer.Freq
			};
			msg.Message = message;
			_world.BroadCast(msg);
		});
		_connection.Recv(delegate(DisappearEntityOnTile msg, PacketHeader header)
		{
			_world.DestroyNatural(msg.Tile);
		});
		_connection.Recv(delegate(GetEstateLicenses msg, PacketHeader header)
		{
			Send(default(EstateLicenses), header.Seq);
		});
		_connection.Recv(delegate(OpenGate msg, PacketHeader header)
		{
			_world.ArtifactManager.OpenGate(new PropKey
			{
				EntityId = msg.EntityId,
				Tile = msg.Tile
			}, open: true);
		});
		_connection.Recv(delegate(CloseGate msg, PacketHeader header)
		{
			_world.ArtifactManager.OpenGate(new PropKey
			{
				EntityId = msg.EntityId,
				Tile = msg.Tile
			}, open: false);
		});
		_connection.Recv(delegate(SetStorageItem msg, PacketHeader header)
		{
			_context.Storage[msg.Key] = msg.Value;
			OnContextChanged();
		});
		_connection.Recv(delegate(TurnOnMusic msg, PacketHeader header)
		{
			_world.ArtifactManager.TurnOnMusic(msg.EntityId);
		});
		_connection.Recv(delegate(TurnOffMusic msg, PacketHeader header)
		{
			_world.ArtifactManager.TurnOffMusic(msg.EntityId);
		});
		_connection.Recv(delegate(ChangeMannequinDisplay msg, PacketHeader header)
		{
			List<Item> inventoryItems = _context.InventoryItems;
			int num = inventoryItems.FindIndex((Item it) => it.Id == msg.ItemId);
			if (num != -1)
			{
				Item item = inventoryItems[num];
				if (_world.ArtifactManager.ChangeMannequin(msg.EntityId, msg.Slot, item))
				{
					Send(default(OK), header.ReplyOf);
					return;
				}
			}
			Send(default(Abort), header.ReplyOf);
		});
		_connection.Recv(delegate(TakeOutItem msg, PacketHeader header)
		{
			ArtTakeout(msg);
			if (_world.ArtifactManager.TakeOutItems(msg.EntityId, msg.ItemIds))
			{
				Send(default(OK), header.ReplyOf);
			}
			else
			{
				Send(default(Abort), header.ReplyOf);
			}
		});
		_connection.Recv(delegate(Collect msg, PacketHeader header)
		{
			CollectNatural(msg, header);
			OnContextChanged();
		});
		_connection.Recv(delegate(GetGrazedPets msg, PacketHeader header)
		{
			Send(new GrazedPets
			{
				Data = _world.GetGrazedPets().ToArray()
			}, header.ReplyOf);
		});
		_connection.Recv(delegate(GetQuests msg, PacketHeader header)
		{
			Chapters chapters = SingletonDict<string, Chapters>.Instance.Get("sunset");
			if (chapters != null)
			{
				Quests msg3 = default(Quests);
				msg3.Category = "sunset";
				IEnumerable<IEnumerable<QuestToDo>> source = chapters.ChapterList.Select((Chapter chapter) => (chapter.Quests == null) ? Enumerable.Empty<QuestToDo>() : chapter.Quests.Select(delegate(string q)
				{
					QuestToDo result = default(QuestToDo);
					result.Finished = true;
					result.Id = q;
					return result;
				}));
				msg3.Todos = source.Aggregate(Enumerable.Concat).ToArray();
				Send(msg3, header.Seq);
			}
		});
		if (_itemGenDict == null)
		{
			_itemGenDict = new Dictionary<string, List<Generator>>();
			GenFileLoader(_context.PlayerSlot, _context.Path);
		}
		SendSkills();
		SendInventory();
		SendEquipments();
		SendDefoggedChunks();
		SendQuestCategories();
		SendActiveActions();
		SendWildAnimals();
		SendMultiSystem();
		WalletUpdated msg2 = new WalletUpdated
		{
			EntityId = _context.AppearPlayer.EntityId,
			Wallet = _context.Wallet
		};
		Send(msg2);
		Send(_context.AppearPlayer);
		_context.PlayerInfo.DisconnectedAt = Times.UnixTimeNow();
		GameSystem<CombatSystem>.Instance().DamagedProcesser.Damaged += OnDamaged;
		TestInteraction();
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.OnePunch, delegate(InteractionObject obj)
		{
			_punchMachineTarget = obj.GetTargetComponent<Artifact>();
			SoundManager.PlayEvent(_insertCoinAudio);
			SoundManager.PrepareEvent(_insertCoinAudio);
			SoundManager.PrepareEvent(_startGameAudio);
			StartPunchMachine();
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ViewPunchRanking, delegate
		{
			GenericSelector genericSelector2 = UIManager.Popup.Tooltip<GenericSelector>();
			genericSelector2.ResetArguments();
			genericSelector2.SetTitle("PunchMachine Ranking");
			List<string> names2 = GetPunchRankingUserInfo("name");
			List<string> damages = GetPunchRankingUserInfo("score");
			List<string> dates = GetPunchRankingUserInfo("date");
			for (int j = 0; j < names2.Count; j++)
			{
				genericSelector2.AddItem(names2[j]);
			}
			genericSelector2.SetSelected(delegate(int index)
			{
				if (index != -1)
				{
					if (LocalizeSystem.LocaleLanguage == "ko")
					{
						UIManager.SystemMsg(string.Concat(new object[6]
						{
							"플레이어 <em>",
							names2[index],
							"</em>님의 기록은 <em>",
							damages[index] + "</em>점입니다.\n기록 경신 날짜: <em>",
							dates[index],
							"</em>"
						}), 5f);
					}
					else
					{
						UIManager.SystemMsg(string.Concat(new object[6]
						{
							"Player <em>",
							names2[index],
							"</em>'s record is <em>",
							damages[index] + "</em>.\nRecord breaking date(UTC): <em>",
							DateTime.Parse(dates[index]).AddHours(-9.0).ToString("yyyy-MM-dd hh:mm:ss"),
							"</em>"
						}), 5f);
					}
				}
			});
			genericSelector2.Show();
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.MountVehicle, delegate(InteractionObject obj)
		{
			Artifact targetComponent = obj.GetTargetComponent<Artifact>();
			if (!(targetComponent == null))
			{
				_context.AppearPlayer.Display.BoardingOn = BoardingOn.Vehicle;
				_context.AppearPlayer.Display.VehicleEntityId = targetComponent.EntityId;
				Send(_context.AppearPlayer.Display);
				OnContextChanged();
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.AddArchitect, delegate(InteractionObject obj)
		{
			GenericSelector genericSelector = UIManager.Popup.Tooltip<GenericSelector>();
			genericSelector.ResetArguments();
			genericSelector.SetTitle("유저 목록");
			List<string> names = _world.GetMultiUserInfo("name");
			List<string> entityIds = _world.GetMultiUserInfo("id");
			for (int i = 0; i < names.Count; i++)
			{
				genericSelector.AddItem(names[i]);
			}
			genericSelector.AddItem("직접 입력");
			genericSelector.SetSelected(delegate(int index)
			{
				if (index != -1)
				{
					if (index == names.Count)
					{
						UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string input)
						{
							_world.AddArchitect(obj.EntityId, input);
						}, T._("추가할 플레이어의 엔티티아이디를 입력하세요."), null, isMultiline: true, null, 0);
					}
					else
					{
						_world.AddArchitect(obj.EntityId, entityIds[index]);
					}
				}
			});
			genericSelector.Show();
		});
		OnContextChanged();
	}

	private WorldPosition GetEntryPosition()
	{
		return new WorldPosition(_world.EntryPoint.x * 200, _world.EntryPoint.y * 200);
	}

	private void World_ArtifactAppeared(AppearArtifact artifact)
	{
		if (IsOverlapped(artifact))
		{
			_artifactSet.Add(artifact.EntityId);
			Send(artifact);
		}
	}

	private void World_ArtifactDisappeared(AppearArtifact artifact)
	{
		if (_artifactSet.Contains(artifact.EntityId))
		{
			_artifactSet.Remove(artifact.EntityId);
			SendDisappear(artifact);
		}
	}

	private void SendDisappear(AppearArtifact artifact)
	{
		DisappearEntity disappearEntity = default(DisappearEntity);
		disappearEntity.EntityId = artifact.EntityId;
		DisappearEntity msg = disappearEntity;
		Send(msg);
	}

	private void World_PlayerAppeared(Player player)
	{
		SendAppear(player);
	}

	public void SendAppear(Player player)
	{
		if (player.EntityId != EntityId)
		{
			Send(player._context.AppearPlayer);
		}
	}

	private void World_PlayerDisappeared(Player player)
	{
		SendDisappear(player);
	}

	private void SendDisappear(Player player)
	{
		if (!(player.EntityId == EntityId))
		{
			Send(new DisappearEntity
			{
				EntityId = player.EntityId
			});
		}
	}

	public void AddItems(IList<Item> items)
	{
		_context.InventoryItems.AddRange(items);
		SendInventory();
		OnContextChanged();
	}

	private void OnContextChanged()
	{
		if (this.ContextChanged != null)
		{
			this.ContextChanged();
		}
	}

	private void SendStatistics()
	{
		Send(new Statistics
		{
			DerivedsAbilities = new Dictionary<Derived, float> { 
			{
				Derived.Swimming,
				100f
			} },
			BasicAbilities = new Dictionary<Basic, int>(),
			Level = _context.AppearPlayer.Level,
			Exp = 0
		});
	}

	private void SetCenterChunks(int x, int y)
	{
		int num = Mathf.Clamp(x, 0, _world.NumChunksX - 1);
		int num2 = Mathf.Clamp(y, 0, _world.NumChunksY - 1);
		ClearVisited(num, num2);
		_centerX = num;
		_centerY = num2;
		MarkVisit();
		List<Chunk> list = _world.CreateChunkMessages(_centerX, _centerY, _chunkVisited);
		for (int i = 0; i < list.Count; i++)
		{
			Send(list[i]);
		}
		foreach (string item in _artifactSet.ToList())
		{
			AppearArtifact? appearArtifact = _world.ArtifactManager.Get(item);
			if (appearArtifact.HasValue && !IsOverlapped(appearArtifact.Value))
			{
				SendDisappear(appearArtifact.Value);
				_artifactSet.Remove(item);
			}
		}
		foreach (AppearArtifact item2 in _world.ArtifactManager.Enumerable((AppearArtifact artifact) => !_artifactSet.Contains(artifact.EntityId) && IsOverlapped(artifact)))
		{
			_artifactSet.Add(item2.EntityId);
			Send(item2);
		}
	}

	private void ClearVisited(int newX, int newY)
	{
		for (int i = _centerX - 1; i <= _centerX + 1; i++)
		{
			for (int j = _centerY - 1; j <= _centerY + 1; j++)
			{
				if (i >= 0 && i < _world.NumChunksX && j >= 0 && j < _world.NumChunksY && (i < newX - 1 || i > newX + 1 || j < newY - 1 || j > newY + 1))
				{
					_chunkVisited[i, j] = World.ChunkVisit.None;
				}
			}
		}
	}

	private void MarkVisit()
	{
		for (int i = _centerX - 1; i <= _centerX + 1; i++)
		{
			for (int j = _centerY - 1; j <= _centerY + 1; j++)
			{
				if (i >= 0 && i < _world.NumChunksX && j >= 0 && j < _world.NumChunksY && _chunkVisited[i, j] != World.ChunkVisit.Sent)
				{
					_chunkVisited[i, j] = World.ChunkVisit.Visit;
				}
			}
		}
	}

	private void HandleMoveMsg(Movement[] movements)
	{
		int num = movements.Length - 1;
		if (num >= 0)
		{
			Movement movement = movements[num];
			_context.AppearPlayer.Move.Movements[0] = movement;
			int num2 = movement.Path.Length - 1;
			if (num2 >= 0)
			{
				_context.AppearPlayer.Move.Movements[0].Path[0].Position = movement.Path[num2].Position;
			}
		}
	}

	private bool IsOverlapped(AppearArtifact artifact)
	{
		int num = (_centerX - 1) * 16;
		int num2 = (_centerX + 2) * 16;
		int num3 = (_centerY - 1) * 16;
		int num4 = (_centerY + 2) * 16;
		bool num5 = num - artifact.Size.x + 1 <= artifact.Tile.x && artifact.Tile.x < num2;
		bool flag = num3 - artifact.Size.y + 1 <= artifact.Tile.y && artifact.Tile.y < num4;
		return num5 && flag;
	}

	private void SendDefoggedChunks()
	{
		DefoggedChunks msg = _world.CreateDefoggedChunks();
		Send(msg);
	}

	private void SendQuestCategories()
	{
		QuestCategory value = default(QuestCategory);
		value.Category = "sunset";
		Send(new QuestCategories
		{
			Epic = value
		});
	}

	private void HandleCheatMsg(string cheat, uint seq)
	{
		cheat = cheat.ToLower();
		string[] array = cheat.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			return;
		}
		string text = array[0];
		AppearArtifact? appearArtifact;
		AddOns? addons;
		switch (PlayerHash.ComputeStringHash(text))
		{
		case 854911780u:
			if (text == "immortal")
			{
				goto IL_08b2;
			}
			goto default;
		case 516436936u:
			if (text == "grazed_to_pet")
			{
				string id4 = array[1];
				List<Messages.Pet> pets = _world.GetPets();
				List<Messages.Pet> grazedPets2 = _world.GetGrazedPets();
				int index2 = grazedPets2.FindIndex((Messages.Pet p) => p.EntityId == id4);
				pets.Add(grazedPets2[index2]);
				_world.BroadCast(new Messages.Pets
				{
					Data = pets.ToArray()
				});
				_world.Save();
				break;
			}
			goto default;
		case 243880259u:
			if (text == "remove_pet")
			{
				string id3 = array[1];
				List<Messages.Pet> pets3 = _world.GetPets();
				int num9 = pets3.FindIndex((Messages.Pet p) => p.EntityId == id3);
				if (num9 != -1)
				{
					pets3.RemoveAt(num9);
					_world.BroadCast(new Messages.Pets
					{
						Data = pets3.ToArray()
					});
					_world.Save();
				}
				break;
			}
			goto default;
		case 1194886160u:
			if (text == "it")
			{
				InventoryUpdated msg2 = default(InventoryUpdated);
				msg2.EntityId = EntityId;
				int level = int.Parse(array[2]);
				int result = 0;
				if (KUtility.GetSize(array) >= 4)
				{
					int.TryParse(array[3], out result);
				}
				if (result == 0)
				{
					result = 1;
				}
				List<Item> list = new List<Item>();
				for (int i = 0; i < result; i++)
				{
					Item? item = Cheats.MakeItem(array[1], level);
					if (item.HasValue)
					{
						list.Add(item.Value);
					}
				}
				msg2.Items = list.ToArray();
				AddItems(list);
				Send(new Info
				{
					Text = $"{list[0].Name} {result}개 획득"
				}, seq);
				Send(msg2);
				break;
			}
			goto default;
		case 1070481844u:
			if (text == "remove_grazing_pet")
			{
				string id2 = array[1];
				List<Messages.Pet> grazedPets = _world.GetGrazedPets();
				int num7 = grazedPets.FindIndex((Messages.Pet p) => p.EntityId == id2);
				if (num7 != -1)
				{
					grazedPets.RemoveAt(num7);
					_world.BroadCast(new GrazedPets
					{
						Data = grazedPets.ToArray()
					});
					_world.Save();
				}
				break;
			}
			goto default;
		case 910182291u:
			if (text == "weather")
			{
				string[] array2 = new string[6] { "sunny", "cloudy", "rainy", "heavy_rainy", "snowy", "heavy_snowy" };
				string text2 = null;
				for (int j = 0; j < 10; j++)
				{
					string text3 = array2[UnityEngine.Random.Range(0, array2.Length)];
					if (text3 != _world.Weather)
					{
						text2 = text3;
						break;
					}
				}
				if (text2 != null)
				{
					_world.ChangeWeather(text2);
				}
				return;
			}
			goto default;
		case 3596982256u:
			if (text == "it_color")
			{
				if (array.Length >= 2)
				{
					string itemId2 = array[1];
					List<Item> inventoryItems3 = _context.InventoryItems;
					int num8 = inventoryItems3.FindIndex((Item it) => it.Id == itemId2);
					if (num8 == -1)
					{
						return;
					}
					Item item2 = inventoryItems3[num8];
					Prototype itemPrototype = PrototypeYaml.GetItemPrototype(item2.Prototype);
					if (itemPrototype == null)
					{
						return;
					}
					int value = UnityEngine.Random.Range(0, int.MaxValue);
					ItemIconTex.TryGetDefaultColor(itemPrototype.ColorR, out var col, value, Color.white);
					ItemIconTex.TryGetDefaultColor(itemPrototype.ColorG, out var col2, value, Color.white);
					ItemIconTex.TryGetDefaultColor(itemPrototype.ColorB, out var col3, value, Color.white);
					item2.ColorR = col.ToHex();
					item2.ColorG = col2.ToHex();
					item2.ColorB = col3.ToHex();
					inventoryItems3[num8] = item2;
					Send(new InventoryUpdated
					{
						EntityId = EntityId,
						Items = new Item[1] { item2 }
					});
					if (_context.EquippedItems.Any((KeyValuePair<string, string> pair) => pair.Value == itemId2))
					{
						UpdateEquipments();
						_world.BroadCast(_context.AppearPlayer.Display);
					}
				}
				break;
			}
			goto default;
		case 1854840628u:
			if (text == "pet_imprint")
			{
				string itemId = array[1];
				List<Item> inventoryItems2 = _context.InventoryItems;
				int num5 = inventoryItems2.FindIndex((Item it) => it.Id == itemId);
				if (num5 == -1)
				{
					UIManager.SystemMsg("Error", "오류가 발생했습니다.");
					return;
				}
				PerformanceYaml.Rein rein = PerformanceYaml.GetRein(inventoryItems2[num5].Prototype);
				if (rein == null)
				{
					UIManager.SystemMsg("Error", "오류가 발생했습니다.");
					return;
				}
				Yaml.Pet pet = SingletonDict<int, Yaml.Pet>.Get(rein.PetEntityType);
				string path = ((pet != null) ? AnimalYaml.GetPrefabPath(pet.VehicleEntityType) : null);
				MessageBox messageBox = UIManager.MessageBox;
				UIWidget modelViewer = messageBox.ModelViewer;
				UIModelViewer componentInChildren = modelViewer.GetComponentInChildren<UIModelViewer>(includeInactive: true);
				componentInChildren.SetPlainModel(path, new UIModelViewer.Arguments
				{
					CameraAngle = 35f,
					Rotation = 140f,
					Loaded = componentInChildren.DefaultAnimalPlay("idle", "stand")
				});
				messageBox.SetCustomWidget(modelViewer, MessageBox.Position.Top);
				messageBox.Show(T._("<em>{0}</em>{0:-을} 귀속하시겠습니까?", rein.PetName), T._("[icon=icon_make_alert] 한 번 귀속한 동물은 귀속해제 전까지 판매하거나 다른 사람에게 양도할 수 없습니다."), delegate(int index)
				{
					if (index == 0)
					{
						float num10 = UnityEngine.Random.Range(15420f, 38219f);
						Gauge gauge = new Gauge(num10, 0f, new GaugeNode[1]
						{
							new GaugeNode
							{
								Time = 0.0,
								Value = num10
							}
						});
						Dictionary<Derived, float> derivedAbilities = new Dictionary<Derived, float>
						{
							{
								Derived.Speed,
								rein.Speed
							},
							{
								Derived.InventoryCapacity,
								rein.Capacity
							},
							{
								Derived.Attack,
								UnityEngine.Random.Range(456f, 1234f)
							},
							{
								Derived.Defense,
								UnityEngine.Random.Range(456f, 1234f)
							},
							{
								Derived.Accuracy,
								UnityEngine.Random.Range(456f, 1234f)
							}
						};
						Messages.Pet pet2 = default(Messages.Pet);
						pet2.EntityId = itemId;
						pet2.EntityType = (ushort)rein.PetEntityType;
						pet2.TamerEntityId = _context.AppearPlayer.EntityId;
						pet2.Name = rein.PetName;
						pet2.Rank = (PetRank)UnityEngine.Random.Range(10, 15);
						pet2.Stat = new PetStats
						{
							PlaybackRate = rein.PlaybackRate,
							Size = rein.Size,
							Life = gauge,
							IsOld = false,
							Hungry = gauge,
							AgingSince = Times.UnixTimeNow(),
							AgingUntil = Times.UnixTimeNow() + 2592000000.0
						};
						pet2.Statistics = new PetStatistics
						{
							Level = inventoryItems2[num5].Level,
							Exp = 0,
							DerivedAbilities = derivedAbilities
						};
						Messages.Pet item3 = pet2;
						List<Messages.Pet> pets4 = _world.GetPets();
						pets4.Add(item3);
						_world.BroadCast(new Messages.Pets
						{
							Data = pets4.ToArray()
						});
						SoundManager.PlayEvent("ui_button_animal_bind");
						UIManager.Alarm.ShowNotify(T._("{0:을} 귀속했습니다!", rein.PetName), "act_domesticate_1", major: true);
						PetGroup petGroup = UIManager.FindScript<PetGroup>();
						if (petGroup == null)
						{
							UIManager.SystemMsg("Error", "오류가 발생했습니다.");
						}
						else
						{
							petGroup.Open(item3.EntityId);
							_context.InventoryItems.RemoveAt(num5);
							Send(new InventoryUpdated
							{
								EntityId = EntityId,
								RemovedItemIds = new string[1] { itemId }
							});
							_world.Save();
						}
					}
				}, new MessageBox.Button
				{
					Text = T._("네"),
					Style = PresetButton.Style.Solid
				}, new MessageBox.Button
				{
					Text = T._("아니오"),
					Style = PresetButton.Style.Border
				});
				break;
			}
			goto default;
		case 1550195791u:
			if (text == "pet_grazing")
			{
				string id = array[1];
				List<Messages.Pet> grazedPets3 = _world.GetGrazedPets();
				List<Messages.Pet> pets2 = _world.GetPets();
				int index3 = pets2.FindIndex((Messages.Pet p) => p.EntityId == id);
				grazedPets3.Add(pets2[index3]);
				_world.BroadCast(new GrazedPets
				{
					Data = grazedPets3.ToArray()
				});
				_world.Save();
				break;
			}
			goto default;
		case 3644723704u:
			if (text == "prop")
			{
				goto IL_08b2;
			}
			goto default;
		case 3893112696u:
			if (text == "m")
			{
				if (array.Length >= 3)
				{
					Teleported msg = default(Teleported);
					msg.Type = TeleportType.Unknown;
					if (int.TryParse(array[1], out msg.Tile.x) && int.TryParse(array[2], out msg.Tile.y))
					{
						Send(msg);
					}
				}
				break;
			}
			goto default;
		case 3753552150u:
			if (text == "natural")
			{
				if (array.Length >= 4)
				{
					int x = array[1].ToInt();
					int y = array[2].ToInt();
					int num6 = array[3].ToInt();
					_world.AddNatural(new Point2(x, y), (ushort)num6);
				}
				break;
			}
			goto default;
		default:
			{
				UIManager.SystemMsg("unknwon msg");
				break;
			}
			IL_08b2:
			appearArtifact = Cheats.MakeAppearArtifact(array, out addons);
			if (!appearArtifact.HasValue)
			{
				return;
			}
			_world.ConstructArtifact(appearArtifact.Value, addons);
			break;
		}
		OnContextChanged();
	}

	public void HandleTouchMsg(Messages.Touch touch, uint seq)
	{
		if (touch.EntityType <= 0)
		{
			return;
		}
		Touched msg = default(Touched);
		msg.EntityId = touch.EntityId;
		bool flag = GameManager.ClusterMode == Mode.Editable || GameManager.ClusterMode == Mode.SingleMode;
		if (touch.EntityType < 10000)
		{
			Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().RecipeContainer.GetBlueprint(touch.EntityType);
			if (blueprint != null)
			{
				msg.EntityName = blueprint.Name;
				List<Interaction> list = new List<Interaction>();
				if (flag)
				{
					AppearArtifact value = _world.ArtifactManager.Get(touch.EntityId).Value;
					if (value.ArchitectEntityIds == null)
					{
						value.ArchitectEntityIds = new List<string> { GameManager.PlayerId }.ToArray();
						_world.ArtifactManager._artifacts[touch.EntityId] = value;
						_world.Save();
					}
					if (value.ArchitectEntityIds.Contains(GameManager.PlayerId))
					{
						list.Add(Interaction.DestructArtifact);
						list.Add(Interaction.AddArchitect);
					}
				}
				if (blueprint.Components.Contains("Washable"))
				{
					list.Add(Interaction.Wash);
				}
				if (blueprint.Components.Contains("Inventory"))
				{
					list.Add(Interaction.Inventory);
					list.Add(Interaction.BrokenInventory);
					list.Add(Interaction.RenameArtifact);
				}
				if (blueprint.Components.Contains("Shelter"))
				{
					list.Add(Interaction.Rest);
				}
				if (blueprint.Components.Contains("Home"))
				{
					list.Add(Interaction.SetAsHome);
				}
				if (blueprint.Components.Contains("Growable"))
				{
					list.Add(Interaction.Plant);
					list.Add(Interaction.Fertilize);
					list.Add(Interaction.Watering);
					list.Add(Interaction.Uproot);
				}
				if (blueprint.Components.Contains("Modular"))
				{
					list.Add(Interaction.AddOnManage);
					list.Add(Interaction.RemodelArtifact);
				}
				if (blueprint.Components.Contains("Scribble"))
				{
					list.Add(Interaction.ScribbleDrawing);
					list.Add(Interaction.ScribbleText);
				}
				if (blueprint.Components.Contains("Sprinklable"))
				{
					list.Add(Interaction.Sprinkle);
					list.Add(Interaction.PutInLiquidFertilizer);
				}
				if (blueprint.Components.Contains("Workbench"))
				{
					list.Add(Interaction.Craft);
				}
				if (blueprint.Components.Contains("Trap"))
				{
					list.Add(Interaction.OpenTrap);
				}
				if (blueprint.Components.Contains("TutorialBoat"))
				{
					list.Add(Interaction.BuildTutorialBoat);
					list.Add(Interaction.ParticipateTutorialBoat);
					list.Add(Interaction.DepartTutorial);
				}
				if (blueprint.Components.Contains("Port"))
				{
					list.Add(Interaction.SailingRoutes);
				}
				if (blueprint.Components.Contains("CargoWarphole"))
				{
					list.Add(Interaction.SetAsBase);
					list.Add(Interaction.WarpCargoToClan);
					list.Add(Interaction.WarpCargoToPrivate);
				}
				if (blueprint.Components.Contains("PersonalRegionWarphole"))
				{
					list.Add(Interaction.ActivatePersonalRegionWarphole);
					list.Add(Interaction.WarpToPersonalRegion);
				}
				if (blueprint.Components.Contains("CargoReceiver"))
				{
					list.Add(Interaction.GetCargoItems);
				}
				if (blueprint.Components.Contains("GrowCage"))
				{
					list.Add(Interaction.Cage);
				}
				if (blueprint.Components.Contains("DomesticCage"))
				{
					list.Add(Interaction.OpenDomesticCage);
				}
				if (blueprint.Components.Contains("PunchMachine"))
				{
					list.Add(Interaction.OnePunch);
					list.Add(Interaction.ViewPunchRanking);
				}
				if (blueprint.Components.Contains("HotAirBalloon"))
				{
					list.Add(Interaction.RideBalloon);
					list.Add(Interaction.MountAirBalloon);
				}
				if (blueprint.Components.Contains("Bandstand"))
				{
					list.Add(Interaction.HostConcert);
					list.Add(Interaction.RegisterConcert);
				}
				if (blueprint.Components.Contains("Arcade"))
				{
					list.Add(Interaction.MiniGameDance);
				}
				if (blueprint.Components.Contains("ReactingProp"))
				{
					list.Add(Interaction.OpenTechSupport);
					list.Add(Interaction.ManagePioneerGrade);
				}
				if (blueprint.Components.Contains("FactionCenter"))
				{
					list.Add(Interaction.AcceptMission);
					list.Add(Interaction.CancelMission);
					list.Add(Interaction.CancelAllMissions);
					list.Add(Interaction.TestInteraction);
				}
				if (blueprint.Components.Contains("FactionDeliveryHouse"))
				{
					list.Add(Interaction.DeliveryChamberOfPioneer);
					list.Add(Interaction.DeliveryChlorophylForum);
					list.Add(Interaction.DeliveryLama);
					list.Add(Interaction.DeliveryTheCommittee);
					list.Add(Interaction.DeliveryTheFirm);
					list.Add(Interaction.DeliveryRescueTf);
				}
				if (blueprint.Components.Contains("ClanWarehouse"))
				{
					list.Add(Interaction.UseWarehouse);
				}
				if (blueprint.Components.Contains("ClanWareHouse"))
				{
					list.Add(Interaction.UseWarehouse);
				}
				if (blueprint.Components.Contains("Catapult"))
				{
					list.Add(Interaction.AddProjectileToVehicle);
					list.Add((!PlayerBehavior.LocalPlayer.IsRiding) ? Interaction.MountVehicle : Interaction.DismountVehicle);
				}
				if (blueprint.Components.Contains("WarpAccelerator"))
				{
					list.Add(Interaction.Accelerate);
				}
				if (blueprint.Components.Contains("Gate"))
				{
					AppearArtifact? appearArtifact = _world.ArtifactManager.Get(touch.EntityId);
					if (appearArtifact.HasValue)
					{
						list.Add((!appearArtifact.Value.States.GateOpened) ? Interaction.OpenGate : Interaction.CloseGate);
					}
				}
				if (blueprint.Components.Contains("Mannequin"))
				{
					list.Add(Interaction.ChangeMannequinHead);
					list.Add(Interaction.ChangeMannequinBody);
				}
				if (KUtility.GetSize(blueprint.Musics) > 0)
				{
					AppearArtifact? appearArtifact2 = _world.ArtifactManager.Get(touch.EntityId);
					if (appearArtifact2.HasValue)
					{
						list.Add((!appearArtifact2.Value.Display.Music.HasValue) ? Interaction.TurnOnMusic : Interaction.TurnOffMusic);
					}
				}
				if (RecipeDict.HasDecoration(blueprint.Id))
				{
					list.Add(Interaction.ChangeDecoration);
				}
				msg.Interactions = list.Select((Interaction o) => (int)o).ToArray();
			}
			msg.Mannequin = _world.ArtifactManager.GetMannequin(touch.EntityId);
		}
		else if (DataHelper.IsNaturalObject(touch.EntityType))
		{
			BiomeSpriteInfo biomeSpriteInfo = DataHelper.GetBiomeSpriteInfo(touch.EntityType);
			if (biomeSpriteInfo != null)
			{
				msg.EntityName = biomeSpriteInfo.Name;
			}
			if (flag)
			{
				List<Interaction> list2 = new List<Interaction>();
				list2.Add(Interaction.Collect);
				list2.Add(Interaction.RemoveNatural);
				msg.Interactions = list2.Select((Interaction o) => (int)o).ToArray();
				msg.Collectible = HandleTouchNatural(touch, biomeSpriteInfo).Collectible;
			}
		}
		Send(msg, seq);
		OnContextChanged();
	}

	private void HandleDestructMsg(DestructArtifact msg)
	{
		if (_world._context.Artifacts.TryGetValue(msg.EntityId, out var value))
		{
			Server.SendLogs("PlayerName: " + PlayerBehavior.LocalPlayer.GetName() + ", MsgType: DestructArtifact, EntityId: " + msg.EntityId + ", EntityType: " + value.EntityType + ", Tile: " + msg.Tile.ToString());
		}
		_world.DestructArtifact(msg.EntityId);
	}

	private void HandleDumpItemsMsg(DumpItems msg)
	{
		_context.InventoryItems.RemoveAll((Item o) => msg.ItemIds.Any((string p) => p == o.Id));
		Send(new InventoryUpdated
		{
			EntityId = EntityId,
			RemovedItemIds = msg.ItemIds
		});
		OnContextChanged();
	}

	private void HandleGetAddOnsMsg(GetAddOns msg, uint seq)
	{
		Send(_world.ArtifactManager.GetAddons(msg.EntityId), seq);
	}

	private void HandlePlaceAddOnsMsg(PlaceAddOns msg, uint seq)
	{
		Dictionary<int, Item> dictionary = new Dictionary<int, Item>();
		AddOns addons = _world.ArtifactManager.GetAddons(msg.EntityId);
		foreach (KeyValuePair<int, string> pair in msg.AddOnPlacements)
		{
			Item? item = null;
			int num = _context.InventoryItems.FindIndex((Item o) => o.Id == pair.Value);
			if (num != -1)
			{
				item = _context.InventoryItems[num];
			}
			else if (addons._AddOns != null)
			{
				foreach (KeyValuePair<int, Item> addOn in addons._AddOns)
				{
					if (pair.Value == addOn.Value.Id)
					{
						item = addOn.Value;
						break;
					}
				}
			}
			if (item.HasValue)
			{
				dictionary.Add(pair.Key, item.Value);
			}
		}
		if (_world.ArtifactManager.PlaceAddOns(msg.EntityId, dictionary).HasValue)
		{
			Send(_world.ArtifactManager.GetAddons(msg.EntityId), seq);
		}
	}

	private void HandlePlantSeedMsg(PlantSeed msg)
	{
		foreach (Item inventoryItem in _context.InventoryItems)
		{
			if (inventoryItem.Id == msg.SeedItemId)
			{
				_world.ArtifactManager.SeedPlant(msg.EntityId, inventoryItem.Prototype);
				break;
			}
		}
	}

	private void HandleChargeEffectMsg(ChargeEffect msg, uint seq)
	{
		_world.ArtifactManager.ChargeEffect(msg.EntityId);
		Send(default(OK), seq);
	}

	private void HandleScribbleMsg(Scribble msg)
	{
		_world.ArtifactManager.Scribble(msg);
	}

	private void HandleChangeDecorationMsg(Messages.Display msg)
	{
		_world.ArtifactManager.ChangeDecoration(msg.EntityId);
	}

	private void HandleEquipMsg(Equip msg, uint headerSeq)
	{
		if (msg.Action == "equip")
		{
			if (_context.InventoryItems.FindIndex((Item x) => x.Id == msg.ItemId) < 0)
			{
				return;
			}
			_context.EquippedItems[msg.SlotName] = msg.ItemId;
		}
		else if (!_context.EquippedItems.Remove(msg.SlotName))
		{
			return;
		}
		SendEquipments(headerSeq);
		_world.BroadCast(_context.AppearPlayer.Display);
		OnContextChanged();
	}

	private void HandleSearchProductsMsg(SearchProducts msg, uint seq)
	{
		Products msg2 = _world.MarketManager.SearchProduct(msg);
		Send(msg2, seq);
	}

	private void HandleGetFavoriteProductsMsg(GetFavoriteProducts msg, uint seq)
	{
		Send(default(Products), seq);
	}

	private void HandleBuyProductMsg(BuyProduct msg, uint seq)
	{
		Item[] array = _world.MarketManager.BuyProduct(msg.ProductId);
		if (array == null)
		{
			Send(default(Messages.Error), seq);
			return;
		}
		InventoryUpdated msg2 = default(InventoryUpdated);
		msg2.EntityId = EntityId;
		msg2.Items = array;
		AddItems(array);
		Send(msg2);
		Send(default(OK), seq);
	}

	private void HandleGetRecipesMsg(GetRecipes msg, uint seq)
	{
		List<Crafting.Recipe> list = new List<Crafting.Recipe>();
		foreach (Crafting.Category category in GameSystem<RecipeSystem>.Instance().RecipeContainer._categoryList)
		{
			list.AddRange(category.Recipes);
		}
		List<string> list2 = new List<string>();
		foreach (Crafting.Recipe item in list)
		{
			list2.Add(item.Id);
		}
		Send(new Recipes
		{
			Ids = list2.ToArray()
		}, seq);
	}

	private void HandleGetArtifactBlueprintsMsg(GetArtifactBlueprints msg, uint seq)
	{
		List<string> list = new List<string>();
		foreach (Building.Blueprint allBlueprint in GameSystem<RecipeSystem>.Instance().RecipeContainer.GetAllBlueprints())
		{
			list.Add(allBlueprint.Id);
		}
		Send(new ArtifactBlueprints
		{
			Ids = list.ToArray()
		}, seq);
	}

	private void HandleExtendFloorMsg(ExtendFloor msg)
	{
		_world.ExtendFloor(msg.EntityId, msg.WithRoof);
	}

	private void HandleGetMusicsMsg(GetMusics msg, uint seq)
	{
		Send(new Musics
		{
			_Musics = _context.Musics
		}, seq);
	}

	private void HandleSaveMusicToSlotMsg(SaveMusicToSlot msg, uint seq)
	{
		Dictionary<int, Music> dictionary = _context.Musics;
		if (dictionary == null)
		{
			dictionary = new Dictionary<int, Music>();
			_context.Musics = dictionary;
		}
		dictionary[msg.Slot] = msg.Music;
		Send(default(OK), seq);
		OnContextChanged();
	}

	private void HandleRemoveMusicFromSlotMsg(RemoveMusicFromSlot msg, uint seq)
	{
		Dictionary<int, Music> musics = _context.Musics;
		if (musics != null && musics.Remove(msg.Slot))
		{
			Send(default(OK), seq);
			OnContextChanged();
		}
		else
		{
			Send(default(Abort), seq);
		}
	}

	private void HandlePlayMusicMsg(PlayMusic msg)
	{
		Messages.Musician musician = default(Messages.Musician);
		musician.EntityId = EntityId;
		Messages.Musician msg2 = musician;
		Dictionary<int, Music> musics = _context.Musics;
		Music value = default(Music);
		if (musics == null || !musics.TryGetValue(msg.Slot, out value))
		{
			return;
		}
		msg2.Music = value;
		int num = _context.InventoryItems.FindIndex((Item item) => item.Id == msg.InstrumentItemId);
		if (num == -1)
		{
			return;
		}
		Item item2 = _context.InventoryItems[num];
		string text = null;
		if (item2.Performance != null)
		{
			Performance performance = item2.Performance.FirstOrDefault((Performance p) => p.Id == "instrument");
			if (performance.Strs != null)
			{
				text = performance.Strs.Get("timbre");
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			msg2.Timbre = text;
			msg2.PlayedAt = Times.UnixTimeNow();
			_world.BroadCast(msg2);
		}
	}

	private void HandleStopMusicMsg(StopMusic msg)
	{
		_world.BroadCast(new Messages.Musician
		{
			EntityId = EntityId
		});
	}

	private void HandleArtifactDisplayMsg(ArtifactDisplay msg)
	{
		_world.ArtifactManager.UpdateArtifactDisplay(msg);
	}

	private void SendInventory()
	{
		Inventory msg = default(Inventory);
		msg.EntityId = EntityId;
		msg.InventoryInfos.EntityId = EntityId;
		msg.InventoryItems.EntityId = EntityId;
		msg.InventoryInfos.MaxSize = 200;
		msg.InventoryItems.Items = _context.InventoryItems.ToArray();
		Send(msg);
	}

	private Equipments UpdateEquipments()
	{
		Equipments result = default(Equipments);
		result.CurrentType = EquipSlotType.Slot1;
		EquipmentSlot value = default(EquipmentSlot);
		value.IsLocked = false;
		PlayerDisplay display = _context.AppearPlayer.Display;
		display.Body = display.DefaultBody;
		display.Head = null;
		display.BodyColor = new string[3] { "FFFFFF", "FFFFFF", "FFFFFF" };
		display.WeaponInfo = default(WeaponDisplayInfo);
		display.Equip = null;
		display.EquipColor = null;
		Dictionary<string, Item> dictionary = new Dictionary<string, Item>();
		foreach (KeyValuePair<string, string> pair in _context.EquippedItems)
		{
			int num = _context.InventoryItems.FindIndex((Item x) => x.Id == pair.Value);
			if (num < 0)
			{
				continue;
			}
			Item value2 = _context.InventoryItems[num];
			dictionary[pair.Key] = value2;
			string[] array = new string[3] { value2.ColorR, value2.ColorG, value2.ColorB };
			_weaponPerformance = PerformanceYaml.GetWeapon(value2.Prototype);
			if (_weaponPerformance != null)
			{
				display.WeaponInfo = new WeaponDisplayInfo
				{
					WeaponFramework = _weaponPerformance.WeaponFramework
				};
				display.Equip = _weaponPerformance.Model;
				display.EquipColor = array;
			}
			bool flag = _context.AppearPlayer.IsMale();
			PerformanceYaml.Armor armor = PerformanceYaml.GetArmor(value2.Prototype);
			if (armor != null)
			{
				if (armor.Slot == "body")
				{
					display.Body = ((!flag) ? armor.FemaleModel : armor.MaleModel);
					display.BodyColor = array;
				}
				else if (armor.Slot == "head")
				{
					display.Head = ((!flag) ? armor.FemaleModel : armor.MaleModel);
					display.HeadColor = array;
				}
			}
		}
		SendActiveActions();
		_context.AppearPlayer.Display = display;
		value.ItemSlots = dictionary;
		value.UnlockSince = null;
		value.UnlockUntil = null;
		value.TitleId = string.Empty;
		result.Presets = new Dictionary<EquipSlotType, EquipmentSlot>();
		result.Presets[EquipSlotType.Slot1] = value;
		return result;
	}

	private void SendEquipments(uint replyOf = 0u)
	{
		Send(UpdateEquipments(), replyOf);
	}

	public void Process()
	{
		_connection.Process();
	}

	public void Stop()
	{
		_connection.Close();
	}

	public void Send<T>(T msg, uint replyOf = 0u)
	{
		_connection.Send(msg, replyOf);
	}

	public void PetsInfo(GetPetsInfo msg, PacketHeader header)
	{
		PetsInfo msg2 = default(PetsInfo);
		msg2.Pets.Data = _context.PetList.ToArray();
		msg2.GrazedPets.Data = _world._context.GrazedPetList.ToArray();
		Send(msg2, header.ReplyOf);
	}

	public void GetInventories(GetInventory get)
	{
		Inventory msg = default(Inventory);
		List<Item> boxItems = _world.ArtifactManager.GetBoxItems(get.Target.Value.EntityId);
		msg.EntityId = get.Target.Value.EntityId;
		msg.InventoryInfos.EntityId = get.Target.Value.EntityId;
		msg.InventoryItems.EntityId = get.Target.Value.EntityId;
		msg.InventoryInfos.MaxSize = 200;
		msg.InventoryItems.Items = boxItems.ToArray();
		Send(msg);
	}

	public void ArtPutIn(PutInItem item)
	{
		_world.ArtifactManager._boxInventories[item.EntityId].AddRange(_context.InventoryItems.FindAll((Item o) => item.ItemIds.Any((string p) => p == o.Id)));
		_context.InventoryItems.RemoveAll((Item o) => item.ItemIds.Any((string p) => p == o.Id));
		Send(new InventoryUpdated
		{
			EntityId = EntityId,
			RemovedItemIds = item.ItemIds
		});
		Send(new InventoryUpdated
		{
			EntityId = item.EntityId,
			Items = _world.ArtifactManager._boxInventories[item.EntityId].ToArray()
		});
		OnContextChanged();
		_world.Save();
	}

	public void ArtTakeout(TakeOutItem take)
	{
		_context.InventoryItems.AddRange(_world.ArtifactManager._boxInventories[take.EntityId].FindAll((Item o) => take.ItemIds.Any((string p) => p == o.Id)));
		_world.ArtifactManager._boxInventories[take.EntityId].RemoveAll((Item o) => take.ItemIds.Any((string p) => p == o.Id));
		Send(new InventoryUpdated
		{
			EntityId = take.EntityId,
			RemovedItemIds = take.ItemIds
		});
		Send(new InventoryUpdated
		{
			EntityId = EntityId,
			Items = _context.InventoryItems.ToArray()
		});
		OnContextChanged();
		_world.Save();
	}

	public void LearnSkill(LearnSkill skill)
	{
		Messages.Skill skill2 = default(Messages.Skill);
		skill2.Level = skill.Level;
		skill2.SkillId = skill.SkillId;
		skill2.SubId = skill.SubId;
		Node node = GameSystem<SkillSystem>.Instance().FindSkill(skill2);
		SkillBundle item = default(SkillBundle);
		item.Category = node.Category;
		item.SkillId = node.Id;
		item.Levels = new Dictionary<string, int>();
		item.Levels.Add(skill.SubId, skill.Level);
		_context.KnownSkills.Add(item);
		_context.SkillList.Add(skill2);
		SendSkills();
		OnContextChanged();
	}

	public void UnlearnSkill(UntrainSkill untrainSkill)
	{
		Messages.Skill skill = default(Messages.Skill);
		skill.Level = untrainSkill.Level;
		skill.SkillId = untrainSkill.SkillId;
		skill.SubId = untrainSkill.SubId;
		Node node = GameSystem<SkillSystem>.Instance().FindSkill(skill);
		SkillBundle skillBundle = default(SkillBundle);
		skillBundle.Category = node.Category;
		skillBundle.SkillId = node.Id;
		skillBundle.Levels = new Dictionary<string, int>();
		SkillBundle item = skillBundle;
		item.Levels.Remove(skill.SubId);
		_context.KnownSkills.Remove(item);
		OnContextChanged();
		SendSkills();
	}

	public void SendSkills()
	{
		Send(new Skills
		{
			Categories = _context.Skills,
			SkillPoint = _context.SkillPoints,
			SkillList = _context.KnownSkills.ToArray(),
			AdvisedSkills = _context.SkillList.ToArray()
		});
	}

	public void CraftItems(Craft msg)
	{
		Craft craftest = default(Craft);
		craftest.RecipeId = msg.RecipeId;
		craftest.Materials = msg.Materials;
		craftest.ToolItemId = msg.ToolItemId;
		craftest.Workbench = msg.Workbench;
		_context.Craftest = craftest;
		_context.SkillPoints = 9999999;
		OnContextChanged();
	}

	public void SummonPet(SpawnPet pet)
	{
		if (_context.ActivePet.PetData.HasValue)
		{
			PetManager.ReturnMyPet(_context.ActivePet.EntityId);
			PetManager.SpawnMyPet(pet.PetId);
			return;
		}
		if (!_context.PetInventories.ContainsKey(pet.PetId))
		{
			List<Item> value = new List<Item>();
			_context.PetInventories.Add(pet.PetId, value);
		}
		Gauge life = new Gauge(15240f, 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = 15240f
			}
		});
		Survival survival = default(Survival);
		survival.Life = life;
		Messages.Pet value2 = default(Messages.Pet);
		foreach (Messages.Pet pet2 in _context.PetList)
		{
			if (pet.PetId == pet2.EntityId)
			{
				value2 = pet2;
			}
		}
		Dictionary<Derived, float> dictionary = new Dictionary<Derived, float>();
		value2.IsSpawned = true;
		dictionary = value2.Statistics.DerivedAbilities;
		value2.Statistics.DerivedAbilities = dictionary;
		AppearPet appearPet = default(AppearPet);
		appearPet.EntityId = value2.EntityId;
		appearPet.IsAlive = true;
		appearPet.PetData = value2;
		appearPet.Move = null;
		appearPet.EntityType = value2.EntityType;
		appearPet.Survival = survival;
		AppearPet appearPet2 = appearPet;
		Send(appearPet2);
		_context.ActivePet = appearPet2;
		OnContextChanged();
	}

	public void Mount()
	{
		_context.AppearPlayer.Display.BoardingOn = BoardingOn.Pet;
		_context.AppearPlayer.Display.VehicleEntityId = _context.ActivePet.EntityId;
		Send(_context.AppearPlayer.Display);
		OnContextChanged();
	}

	public void Dismount()
	{
		_context.AppearPlayer.Display.BoardingOn = BoardingOn.None;
		_context.AppearPlayer.Display.VehicleEntityId = null;
		Send(_context.AppearPlayer.Display);
		OnContextChanged();
	}

	public void DismissPet(ReturnPet msg)
	{
		Send(new DisappearPet
		{
			TamerEntityId = _context.ActivePet.PetData.Value.TamerEntityId,
			EntityId = msg.PetId
		});
		_context.ActivePet = default(AppearPet);
		OnContextChanged();
	}

	public void RenameArtafact(Rename msg)
	{
		AppearArtifact msg2 = _world._context.Artifacts[msg.EntityId];
		msg2.States.ChangedName = msg.Name;
		Send(msg2);
		OnContextChanged();
	}

	public void CollectNatural(Collect msg, PacketHeader header)
	{
		new List<string>();
		List<Item> list = new List<Item>();
		Messages.SkillCategory value = default(Messages.SkillCategory);
		_context.Skills.TryGetValue(Shared.Skill.Category.Gathering, out value);
		CollectibleChanged changed = default(CollectibleChanged);
		changed.EntityId = msg.EntityId;
		_ = value.Level / 4;
		Result result = Result.Invalid;
		int num2 = new global::System.Random().Next(1, 100);
		if (num2 < 5)
		{
			result = Result.BigFailure;
		}
		if (num2 > 5 && num2 < 10)
		{
			result = Result.Failure;
		}
		if (num2 > 10 && num2 < 85)
		{
			result = Result.Success;
		}
		if (num2 > 85)
		{
			result = Result.GreatSuccess;
		}
		Item? item = GenItem(msg.GeneratorId, msg.Level, result);
		if (item.HasValue)
		{
			Item item2 = default(Item);
			item2 = item.Value;
			item2.Name = _generators.Find((Generator o) => o.Id == msg.GeneratorId).Name;
			list.Add(item2);
		}
		Send(new Messages.Timer
		{
			Duration = 2f
		}, header.Seq);
		global::System.Timers.Timer timer = new global::System.Timers.Timer();
		timer.Interval = 2000.0;
		timer.Enabled = true;
		timer.AutoReset = false;
		timer.Elapsed += delegate
		{
			if (num2 > 5)
			{
				AddItems(list);
			}
			Generator generator = default(Generator);
			int index = _generators.FindIndex((Generator o) => o.Id == msg.GeneratorId);
			generator = _generators[index];
			generator.Amount = _generators[index].Amount - 1;
			_generators[index] = generator;
			if (generator.Amount == 0)
			{
				_generators.RemoveAt(index);
			}
			if (!generator.Enabled)
			{
				_world.DestroyNatural(msg.Tile);
			}
			Collectible value2 = _world._context.CollectedFrom[msg.Tile.ToString()];
			value2.Generators = _generators.ToArray();
			_world._context.CollectedFrom[msg.Tile.ToString()] = value2;
			DateTime value3 = DateTime.Now.AddDays(2.0);
			if (!_world._context.ActionTimer.ContainsKey(msg.Tile.ToString()))
			{
				_world._context.ActionTimer.Add(msg.Tile.ToString(), value3);
			}
			else
			{
				_world._context.ActionTimer.Remove(msg.Tile.ToString());
				_world._context.ActionTimer.Add(msg.Tile.ToString(), value3);
			}
			Send(changed);
			SendCollected(list, result, header);
			_world.Save();
		};
	}

	public Item? GenItem(string prototypeId, int level, Result result)
	{
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(prototypeId);
		Item? result2;
		if (itemPrototype == null)
		{
			result2 = null;
		}
		else
		{
			Messages.SkillCategory value = default(Messages.SkillCategory);
			_context.Skills.TryGetValue(Shared.Skill.Category.Gathering, out value);
			int num = value.Level / 10;
			Item item = default(Item);
			item.Id = Guid.NewGuid().ToString();
			item.FounderId = _context.PlayerInfo.PlayerName;
			item.FounderCategory = string.Empty;
			item.Durability = new Gauge(1f, 0f, new GaugeNode[1]
			{
				new GaugeNode(0.0, 1f)
			});
			item.Size = itemPrototype.Size;
			item.Unstable = false;
			item.ModifiableCount = 5 + num;
			item.ModifiedCount = 0;
			Item value2 = item;
			int hashCode = value2.Id.GetHashCode();
			ItemIconTex.TryGetDefaultColor(itemPrototype.ColorR, out var col, hashCode, Color.black);
			ItemIconTex.TryGetDefaultColor(itemPrototype.ColorG, out var col2, hashCode, Color.gray);
			ItemIconTex.TryGetDefaultColor(itemPrototype.ColorB, out col2, hashCode, Color.black);
			value2.ColorR = col.ToHex();
			value2.ColorG = col.ToHex();
			value2.ColorB = col.ToHex();
			value2.Icon = itemPrototype.Icon;
			value2.Prototype = prototypeId;
			value2.Level = value.Level;
			value2.Name = itemPrototype.Name;
			value2.Description = itemPrototype.Description;
			value2.Tags = TagListGenItem(itemPrototype, result).ToArray();
			List<Performance> list = new List<Performance>();
			if (PerformanceYaml.TryGetAddOnModelKey(prototypeId, out var modelKey))
			{
				list.Add(new Performance
				{
					Id = "add_on",
					Strs = new Dictionary<string, string> { { "add_on_model_key", modelKey } }
				});
			}
			PerformanceYaml.Weapon weapon = PerformanceYaml.GetWeapon(prototypeId);
			if (weapon != null)
			{
				list.Add(new Performance
				{
					Id = "weapon",
					Strs = new Dictionary<string, string>
					{
						{ "weapon_framework", weapon.WeaponFramework },
						{ "model", weapon.Model },
						{ "slot", weapon.Slot }
					}
				});
			}
			PerformanceYaml.Armor armor = PerformanceYaml.GetArmor(prototypeId);
			if (armor != null)
			{
				list.Add(new Performance
				{
					Id = "armor",
					Strs = new Dictionary<string, string>
					{
						{ "female_model", armor.FemaleModel },
						{ "male_model", armor.MaleModel },
						{ "slot", armor.Slot }
					}
				});
			}
			PerformanceYaml.Instrument instrument = PerformanceYaml.GetInstrument(prototypeId);
			if (instrument != null)
			{
				list.Add(new Performance
				{
					Id = "instrument",
					Strs = new Dictionary<string, string> { { "timbre", instrument.Timbre } }
				});
			}
			value2.Performance = list.ToArray();
			result2 = value2;
		}
		return result2;
	}

	private void SendCollected(List<Item> list, Result result, PacketHeader header)
	{
		new WaitForSeconds(2f);
		Send(new Collected
		{
			Items = list.ToArray(),
			Result = result
		}, header.Seq);
	}

	public Touched HandleTouchNatural(Messages.Touch touch, BiomeSpriteInfo biomeSpriteInfo)
	{
		List<Generator> list = new List<Generator>();
		Touched result = default(Touched);
		if (_world._context.CollectedFrom.ContainsKey(touch.Tile.ToString()))
		{
			_generators = new List<Generator>();
			result.Collectible = _world._context.CollectedFrom[touch.Tile.ToString()];
			Generator[] generators = _world._context.CollectedFrom[touch.Tile.ToString()].Generators;
			foreach (Generator item in generators)
			{
				_generators.Add(item);
			}
		}
		else
		{
			result.Collectible.EntityId = touch.EntityId;
			result.Collectible.CollectibleId = biomeSpriteInfo.CollectibleId;
			list = Generator(biomeSpriteInfo);
			result.Collectible.Generators = list.ToArray();
			_collectable = result.Collectible;
			_generators = list;
			_world._context.CollectedFrom.Add(touch.Tile.ToString(), result.Collectible);
		}
		_world.Save();
		GenFileMaker(_context.PlayerSlot, _context.Path);
		return result;
	}

	public void ReleasePet(ReleasePet msg, uint seq)
	{
		if (_context.ActivePet.EntityId == msg.PetId)
		{
			PetManager.ReturnMyPet(_context.ActivePet.EntityId);
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("remove_pet");
		stringBuilder.AppendFormat(" {0}", msg.PetId);
		Connections.Frontend.Send(new Cheat
		{
			_Cheat = stringBuilder.ToString().Trim()
		});
		if (_context.PetInventories.ContainsKey(msg.PetId))
		{
			_context.PetInventories.Remove(msg.PetId);
		}
		Send(default(OK), seq);
	}

	public void GetPetInventories(GetPetInventory get)
	{
		PetInventory msg = default(PetInventory);
		Messages.Pet pet = default(Messages.Pet);
		foreach (Messages.Pet pet2 in _context.PetList)
		{
			if (get.EntityId == pet2.EntityId)
			{
				pet = pet2;
			}
		}
		float num = pet.Statistics.DerivedAbilities.Get(Derived.InventoryCapacity, 200f);
		List<Item> petItems = GetPetItems(get.EntityId);
		msg.Inven.EntityId = get.EntityId;
		msg.Inven.InventoryInfos.EntityId = get.EntityId;
		msg.Inven.InventoryItems.EntityId = get.EntityId;
		msg.Inven.InventoryItems.Items = petItems.ToArray();
		msg.Inven.InventoryInfos.MaxSize = (int)num;
		Send(msg);
	}

	public void GrazePets(GrazePets pets)
	{
		if (pets.PetIdsToGraze.Contains(_context.ActivePet.EntityId))
		{
			PetManager.ReturnMyPet(_context.ActivePet.EntityId);
			return;
		}
		string[] petIdsToGraze = pets.PetIdsToGraze;
		foreach (string arg in petIdsToGraze)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("remove_pet");
			stringBuilder.AppendFormat(" {0}", arg);
			Connections.Frontend.Send(new Cheat
			{
				_Cheat = stringBuilder.ToString().Trim()
			});
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append("pet_grazing");
			stringBuilder2.AppendFormat(" {0}", arg);
			Connections.Frontend.Send(new Cheat
			{
				_Cheat = stringBuilder2.ToString().Trim()
			});
		}
	}

	public void PutIntoPet(PutInItemsIntoPet item)
	{
		_context.PetInventories[item.PetId].AddRange(_context.InventoryItems.FindAll((Item o) => item.ItemIds.Any((string p) => p == o.Id)));
		_context.InventoryItems.RemoveAll((Item o) => item.ItemIds.Any((string p) => p == o.Id));
		Send(new InventoryUpdated
		{
			EntityId = _context.AppearPlayer.EntityId,
			RemovedItemIds = item.ItemIds
		});
		Send(new InventoryUpdated
		{
			EntityId = item.PetId,
			Items = _context.PetInventories[item.PetId].ToArray()
		});
		PetStats petStats = default(PetStats);
		petStats.InventoryUsage = _context.PetInventories[item.PetId].Count;
		PetStats msg = petStats;
		Send(msg);
		int index = _context.PetList.FindIndex((Messages.Pet it) => it.EntityId == item.PetId);
		Messages.Pet value = _context.PetList[index];
		value.Stat.InventoryUsage = msg.InventoryUsage;
		_context.PetList[index] = value;
		OnContextChanged();
		_context.Save();
	}

	public void FeedPet(Feeding msg)
	{
	}

	public void RenamePet(RenamePet msg, uint seq)
	{
		int index = _context.PetList.FindIndex((Messages.Pet it) => it.EntityId == msg.PetId);
		Messages.Pet msg2 = _context.PetList[index];
		msg2.Name = msg.Name;
		Send(msg2);
		Send(default(OK), seq);
		OnContextChanged();
	}

	public void MountAirBalloon()
	{
		_context.AppearPlayer.Display.BoardingOn = BoardingOn.AirBalloon;
		Send(_context.AppearPlayer.Display);
		OnContextChanged();
	}

	public void DismountAirBalloon()
	{
		_context.AppearPlayer.Display.BoardingOn = BoardingOn.None;
		Send(_context.AppearPlayer.Display);
		OnContextChanged();
	}

	public void TakeOutFromPet(TakeOutItemsFromPet take)
	{
		_context.InventoryItems.AddRange(_context.PetInventories[take.PetId].FindAll((Item o) => take.ItemIds.Any((string p) => p == o.Id)));
		_context.PetInventories[take.PetId].RemoveAll((Item o) => take.ItemIds.Any((string p) => p == o.Id));
		Send(new InventoryUpdated
		{
			EntityId = take.PetId,
			RemovedItemIds = take.ItemIds
		});
		Send(new InventoryUpdated
		{
			EntityId = _context.AppearPlayer.EntityId,
			Items = _context.InventoryItems.ToArray()
		});
		PetStats petStats = default(PetStats);
		petStats.InventoryUsage = _context.PetInventories[take.PetId].Count;
		PetStats msg = petStats;
		Send(msg);
		int index = _context.PetList.FindIndex((Messages.Pet it) => it.EntityId == take.PetId);
		Messages.Pet value = _context.PetList[index];
		value.Stat.InventoryUsage = msg.InventoryUsage;
		_context.PetList[index] = value;
		OnContextChanged();
		_context.Save();
	}

	public List<Generator> Generator(BiomeSpriteInfo biomeSpriteInfo)
	{
		Messages.SkillCategory value = default(Messages.SkillCategory);
		_context.Skills.TryGetValue(Shared.Skill.Category.Gathering, out value);
		int num = value.Level / 10;
		List<Generator> list = new List<Generator>();
		if (_itemGenDict.Keys.Contains(biomeSpriteInfo.CollectibleId))
		{
			List<Generator> value2 = new List<Generator>();
			_itemGenDict.TryGetValue(biomeSpriteInfo.CollectibleId, out value2);
			{
				foreach (Generator item in value2)
				{
					Generator current = item;
					current.Level = value.Level;
					list.Add(current);
				}
				return list;
			}
		}
		List<string> list2 = new List<string>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		dictionary.Add("bare_hands", 1);
		foreach (Item inventoryItem in _context.InventoryItems)
		{
			if (list2.Count < 5)
			{
				list2.Add(inventoryItem.Prototype);
			}
		}
		foreach (string item2 in list2)
		{
			Prototype itemPrototype = PrototypeYaml.GetItemPrototype(item2);
			list.Add(new Generator
			{
				Id = item2,
				Icon = itemPrototype.Icon,
				Name = itemPrototype.Name,
				Amount = new global::System.Random().Next(1, 10),
				Level = new global::System.Random().Next(10, 60),
				Duration = 2f,
				Effort = 20 - num,
				Enabled = true,
				ToolRequirements = dictionary
			});
		}
		_itemGenDict.Add(biomeSpriteInfo.CollectibleId, list);
		return list;
	}

	public void GenFileMaker(int slot, string path)
	{
		try
		{
			_genDict.ItemGenDict = _itemGenDict;
			_genDict.PlayerSlot = slot;
			byte[] bytes = Json.WriteToBytes(_genDict, indented: true);
			File.WriteAllBytes(Path.GetDirectoryName(path) + "\\" + slot + ".gen", bytes);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void GenFileLoader(int slot, string path)
	{
		try
		{
			if (new FileInfo(Path.GetDirectoryName(path) + "\\" + slot + ".gen").Exists)
			{
				_genDict = Json.Read<GenDict>(File.ReadAllBytes(Path.GetDirectoryName(path) + "\\" + slot + ".gen"));
				_itemGenDict = _genDict.ItemGenDict;
			}
			else
			{
				GenFileMaker(slot, Path.GetDirectoryName(path) + "\\" + slot + ".gen");
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void BackToStableIsland(int slot, string id)
	{
		StableIslandLoader(slot, id, _world._context.Path);
		new Cluster
		{
			OnRequestAccount = delegate(Action<Account> action)
			{
				Account account = new Account();
				account.MaxPlayerSlotCount = 7;
				account.PlayerSlotCount = 1;
				account.Players = new List<Durango.Logic.Clusters.PlayerInfo>();
				account.Players.Add(_context.PlayerInfo);
				action?.Invoke(account);
			},
			GatewayUrlRoot = "http://127.0.0.1:" + Server.GetIslandPort(),
			LocalPlayer = Json.Write(_context)
		};
		GameManager.Emigrated = GameManager.EmigratedType.Explore;
		Server.BeginServer(_world._context, _context);
		Durango.Utils.Singleton<GameManager>.Instance().MoveToTitle();
	}

	public void StableIslandLoader(int slot, string id, string path)
	{
		try
		{
			if (File.Exists(string.Concat(Path.GetDirectoryName(path), "\\", slot, "." + id)))
			{
				byte[] bytes = File.ReadAllBytes(string.Concat(Path.GetDirectoryName(path), "\\", slot, "." + id));
				File.WriteAllBytes(path, bytes);
			}
			else
			{
				UIManager.SystemMsg("섬 파일이 존재하지 않습니다.");
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void OnDamaged(Damaged msg)
	{
		if (_isUsingPunchMachine)
		{
			bool isFastAtk = false;
			if (_isFastAttack)
			{
				isFastAtk = true;
				if (_curFastAttackCount == 3)
				{
					_isFastAttack = false;
					FinishPunchMachine(msg.Damage, isFastAtk);
				}
			}
			else
			{
				FinishPunchMachine(msg.Damage, isFastAtk);
			}
			return;
		}
		if (msg.VictimId == _context.AppearPlayer.EntityId)
		{
			bool isDead = false;
			if ((float)msg.Damage.Value >= PlayerBehavior.LocalPlayer.Life.Get())
			{
				isDead = true;
				OnTakeDamage(msg.Damage, isDead);
			}
			else
			{
				OnTakeDamage(msg.Damage, isDead);
			}
			return;
		}
		bool isDead2 = false;
		CharacterBehavior characterBehavior = Durango.Utils.Singleton<ObjectManager>.Instance().FindCharacter(msg.VictimId);
		if (characterBehavior == null)
		{
			UIManager.SystemMsg("Error", "타겟이 존재하지 않습니다.");
			return;
		}
		WildAnimalAI component = characterBehavior.GetComponent<WildAnimalAI>();
		if (component != null)
		{
			if ((float)msg.Damage.Value >= component.TargetAnimal.Life.Get())
			{
				isDead2 = true;
				component.OnTakeDamage(msg.Damage, isDead2);
			}
			else
			{
				component.OnTakeDamage(msg.Damage, isDead2);
			}
		}
	}

	public void OnExitBattleMsg(ExitBattle msg)
	{
		if (_isUsingPunchMachine)
		{
			Connections.Frontend.PushPacket(new BattleEnded
			{
				EntityId = _context.AppearPlayer.EntityId
			});
			_isUsingPunchMachine = false;
			UIManager.SystemMsg("펀치머신 이용이 종료되었습니다.");
		}
		else
		{
			if (!GameSystem<CombatSystem>.Instance().CombatMode)
			{
				return;
			}
			CharacterBehavior characterBehavior = Durango.Utils.Singleton<ObjectManager>.Instance().FindCharacter(_targetEntityId);
			if (characterBehavior == null)
			{
				UIManager.SystemMsg("Error", "타겟이 존재하지 않습니다.");
				return;
			}
			WildAnimalAI component = characterBehavior.GetComponent<WildAnimalAI>();
			if (component == null)
			{
				UIManager.SystemMsg("Error", "오류가 발생하였습니다.");
				return;
			}
			GameSystem<CombatSystem>.Instance().ClearTarget();
			Connections.Frontend.PushPacket(new BattleEnded
			{
				EntityId = _context.AppearPlayer.EntityId
			});
			component.RemoveActivatedAi();
		}
	}

	public void OnTakeDamage(Damage damage, bool isDead)
	{
		if (damage.Value <= 0)
		{
			return;
		}
		if (isDead)
		{
			Gauge life = new Gauge(PlayerBehavior.LocalPlayer.Life.Max(), 0f, new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = PlayerBehavior.LocalPlayer.Life.Get() - (float)damage.Value
				}
			});
			PlayerBehavior.LocalPlayer.SetSurvivalGauge(life, _context.AppearPlayer.Survival.Gauges);
			EventDead();
			return;
		}
		Gauge life2 = new Gauge(PlayerBehavior.LocalPlayer.Life.Max(), 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = PlayerBehavior.LocalPlayer.Life.Get() - (float)damage.Value
			}
		});
		PlayerBehavior.LocalPlayer.SetSurvivalGauge(life2, _context.AppearPlayer.Survival.Gauges);
		if (damage.Effects == DamageEffects.Blow)
		{
			EventBlow();
		}
		if (damage.Result == DamageResult.Missed)
		{
			EventFlinch();
		}
	}

	private void EventDead()
	{
		Gauge life = new Gauge(PlayerBehavior.LocalPlayer.Life.Max(), 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = 0f
			}
		});
		PlayerBehavior.LocalPlayer.SetSurvivalGauge(life, _context.AppearPlayer.Survival.Gauges);
	}

	private void EventBlow()
	{
		_fastAttackTimer.Stop();
		_isBlowing = true;
		_blowTimer = new global::System.Timers.Timer(2000.0);
		_blowTimer.Enabled = true;
		_blowTimer.AutoReset = false;
		_blowTimer.Elapsed += delegate
		{
			_isBlowing = false;
		};
	}

	private void EventFlinch()
	{
		_fastAttackTimer.Stop();
	}

	public Vector3 AngleToDirection(Vector3 vStart, Vector3 vEnd)
	{
		Vector3 forward = vEnd - vStart;
		Vector3 forward2 = _WildAnimalAI.TargetAnimal.transform.forward;
		return Quaternion.LookRotation(forward) * forward2;
	}

	public List<Messages.Tag> TagListGenItem(Prototype itemPrototype, Result result)
	{
		List<Messages.Tag> list = new List<Messages.Tag>();
		foreach (KeyValuePair<string, string> tag in itemPrototype.Tags)
		{
			list.Add(new Messages.Tag
			{
				Level = _context.PlayerInfo.PlayerLevel,
				Id = tag.Key
			});
		}
		new global::System.Random().Next(1, 100);
		if (itemPrototype.Tags.ContainsKey("fruit") && result == Result.Success)
		{
			list.Add(new Messages.Tag
			{
				Level = _context.PlayerInfo.PlayerLevel,
				Id = "fresh"
			});
		}
		return list;
	}

	private void OnSadismTimedEvent(object p0, ElapsedEventArgs p1)
	{
		if (!_isAlreadySadism)
		{
			_isAlreadySadism = true;
			Messages.StatusEffect statusEffect = default(Messages.StatusEffect);
			statusEffect.Id = _context.AppearPlayer.EntityId;
			statusEffect.EffectId = "sadism";
			statusEffect.Level = _sadismLevel;
			statusEffect.NameGettext = "가학성";
			statusEffect.DurationHidden = false;
			statusEffect.Since = Connections.Frontend.GetPredictedServerTime();
			statusEffect.Until = Connections.Frontend.GetPredictedServerTime() + 10.0;
			Messages.StatusEffect effect = statusEffect;
			AddPlayerStatusEffect(effect);
		}
		if (PlayerBehavior.LocalPlayer.Life.Get() < PlayerBehavior.LocalPlayer.Life.Max())
		{
			Gauge life = new Gauge(PlayerBehavior.LocalPlayer.Life.Max(), 0f, new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = PlayerBehavior.LocalPlayer.Life.Get() + (float)_sadismLevel
				}
			});
			PlayerBehavior.LocalPlayer.SetSurvivalGauge(life, _context.AppearPlayer.Survival.Gauges);
		}
	}

	private void UpdateSurvival()
	{
		_context.AppearPlayer.Survival.Life = PlayerBehavior.LocalPlayer.Life;
		Survival survival = default(Survival);
		survival.EntityId = _context.AppearPlayer.EntityId;
		survival.Life = _context.AppearPlayer.Survival.Life;
		_context.Save();
	}

	public void AddPlayerStatusEffect(Messages.StatusEffect effect)
	{
		_statusList.Add(effect);
		_statusEffects = new Messages.StatusEffects
		{
			EntityId = effect.Id,
			_StatusEffects = _statusList.ToArray()
		};
		GameSystem<StatusEffectSystem>.Instance().GetStatusEffects().SetStatusEffects(_statusEffects);
	}

	public void RemovePlayerStatusEffect(string entityId, string effectId)
	{
		_statusList.RemoveAll((Messages.StatusEffect ef) => ef.EffectId == effectId);
		_statusEffects.EntityId = entityId;
		_statusEffects._StatusEffects = _statusList.ToArray();
		GameSystem<StatusEffectSystem>.Instance().GetStatusEffects().SetStatusEffects(_statusEffects);
	}

	private void OnBloodBurstTimedEvent(object p0, ElapsedEventArgs p1)
	{
		if (_WildAnimalAI.TargetAnimal.Life.Get() > _WildAnimalAI.TargetAnimal.Life.Min())
		{
			if (!_isAlreadyBloodBurst)
			{
				_isAlreadyBloodBurst = true;
				Messages.StatusEffect statusEffect = default(Messages.StatusEffect);
				statusEffect.Id = _WildAnimalAI.TargetAnimal.EntityId;
				statusEffect.EffectId = "life_decr";
				statusEffect.Level = _bloodBurstLevel;
				statusEffect.NameGettext = "출혈";
				statusEffect.DurationHidden = false;
				statusEffect.Since = Connections.Frontend.GetPredictedServerTime();
				statusEffect.Until = Connections.Frontend.GetPredictedServerTime() + 8.0;
				Messages.StatusEffect effect = statusEffect;
				AddTargetStatusEffect(effect);
			}
			Gauge life = new Gauge(_WildAnimalAI.TargetAnimal.Life.Max(), 0f, new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = _WildAnimalAI.TargetAnimal.Life.Get() - (float)_bloodBurstLevel
				}
			});
			_WildAnimalAI.TargetAnimal.SetSurvivalGauge(life, null);
		}
		else if (_WildAnimalAI.TargetAnimal.Life.Get() <= 0f)
		{
			_bloodBurstTimer.Stop();
			_isAlreadyBloodBurst = true;
			RemoveTargetStatusEffect(_WildAnimalAI.TargetAnimal.EntityId, "life_decr");
			_WildAnimalAI.EventDead();
		}
	}

	public void AddTargetStatusEffect(Messages.StatusEffect effect)
	{
		_statusList.Add(effect);
		_statusEffects = new Messages.StatusEffects
		{
			EntityId = effect.Id,
			_StatusEffects = _statusList.ToArray()
		};
		GameSystem<StatusEffectSystem>.Instance().GetStatusEffects(effect.Id).SetStatusEffects(_statusEffects);
	}

	public void RemoveTargetStatusEffect(string entityId, string effectId)
	{
		_statusList.RemoveAll((Messages.StatusEffect ef) => ef.EffectId == effectId);
		_statusEffects.EntityId = entityId;
		_statusEffects._StatusEffects = _statusList.ToArray();
		GameSystem<StatusEffectSystem>.Instance().GetStatusEffects(entityId).SetStatusEffects(_statusEffects);
	}

	private void SendActiveActions()
	{
		if (_weaponPerformance == null)
		{
			GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[8]
			{
				new BattleAction(SingletonDict<string, PlayerAction>.Get("barehand_default_a"))
				{
					Motion = "Barehand_AttackPunch"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("barehand_default_b"))
				{
					Motion = "Barehand_Attack_B"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("barehand_kick_a"))
				{
					Motion = "Barehand_Attack_Kick_A"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("barehand_kick_b"))
				{
					Motion = "Barehand_Attack_Kick_B"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("barehand_smash"))
				{
					Motion = "Barehand_AttackStrong"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("barehand_combination"))
				{
					Motion = "Barehand_Attack_TriplePunch"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("barehand_dodge"))
				{
					Motion = "Onehand_Dodge"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("melee_tackle"))
				{
					Motion = "Barehand_Attack_Pursuit"
				}
			});
		}
		else if (_weaponPerformance.WeaponFramework == "onehand" && _weaponPerformance.AttackType == "sword")
		{
			GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[8]
			{
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_default_a"))
				{
					Motion = "Onehand_Attack_A"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_default_b"))
				{
					Motion = "Onehand_Attack_B"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_default_c"))
				{
					Motion = "Onehand_Attack_C"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_smash"))
				{
					Motion = "Onehand_AttackStrong"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_flurry"))
				{
					Motion = "Onehand_Attack_TripleSwing"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_stab"))
				{
					Motion = "Onehand_Attack_Stab"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_dodge"))
				{
					Motion = "Onehand_Dodge"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("melee_tackle"))
				{
					Motion = "Barehand_Attack_Pursuit"
				}
			});
		}
		else if (_weaponPerformance.WeaponFramework == "twohand" && _weaponPerformance.AttackType == "sword")
		{
			GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[8]
			{
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_default_a"))
				{
					Motion = "Twohand_Attack_A"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_default_b"))
				{
					Motion = "Twohand_Attack_B"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_default_c"))
				{
					Motion = "Twohand_Attack_C"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_smash"))
				{
					Motion = "Twohand_AttackDash"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_sweeping"))
				{
					Motion = "Twohand_AttackStrong"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_strike"))
				{
					Motion = "Twohand_AttackSwing_Lower"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_dodge"))
				{
					Motion = "Twohand_Dodge"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("melee_tackle"))
				{
					Motion = "Barehand_Attack_Pursuit"
				}
			});
		}
		else if (_weaponPerformance.WeaponFramework == "onehand" && _weaponPerformance.AttackType == "axe")
		{
			GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[8]
			{
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_default_axe_a"))
				{
					Motion = "Onehand_Attack_A"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_default_axe_b"))
				{
					Motion = "Onehand_Attack_B"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_default_axe_c"))
				{
					Motion = "Onehand_Attack_C"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_smash_axe"))
				{
					Motion = "Onehand_AttackStrong"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_flurry_axe"))
				{
					Motion = "Onehand_Attack_TripleSwing"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_stab_axe"))
				{
					Motion = "Onehand_Attack_Stab"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_dodge"))
				{
					Motion = "Onehand_Dodge"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("melee_tackle"))
				{
					Motion = "Barehand_Attack_Pursuit"
				}
			});
		}
		else if (_weaponPerformance.WeaponFramework == "twohand" && _weaponPerformance.AttackType == "axe")
		{
			GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[8]
			{
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_default_axe_a"))
				{
					Motion = "Twohand_Attack_A"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_default_axe_b"))
				{
					Motion = "Twohand_Attack_B"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_default_axe_c"))
				{
					Motion = "Twohand_Attack_C"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_smash_axe"))
				{
					Motion = "Twohand_AttackDash"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_sweeping_axe"))
				{
					Motion = "Twohand_AttackStrong"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_strike_axe"))
				{
					Motion = "Twohand_AttackSwing_Lower"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_dodge"))
				{
					Motion = "Twohand_Dodge"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("melee_tackle"))
				{
					Motion = "Barehand_Attack_Pursuit"
				}
			});
		}
		else if (_weaponPerformance.WeaponFramework == "onehand" && _weaponPerformance.AttackType == "blunt")
		{
			GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[8]
			{
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_default_blunt_a"))
				{
					Motion = "Onehand_Attack_A"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_default_blunt_b"))
				{
					Motion = "Onehand_Attack_B"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_default_blunt_c"))
				{
					Motion = "Onehand_Attack_C"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_smash_blunt"))
				{
					Motion = "Onehand_AttackStrong"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_flurry_blunt"))
				{
					Motion = "Onehand_Attack_TripleSwing"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_stab_blunt"))
				{
					Motion = "Onehand_Attack_Stab"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_dodge"))
				{
					Motion = "Onehand_Dodge"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("melee_tackle"))
				{
					Motion = "Barehand_Attack_Pursuit"
				}
			});
		}
		else if (_weaponPerformance.WeaponFramework == "twohand" && _weaponPerformance.AttackType == "blunt")
		{
			GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[8]
			{
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_default_blunt_a"))
				{
					Motion = "Twohand_Attack_A"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_default_blunt_b"))
				{
					Motion = "Twohand_Attack_B"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_default_blunt_c"))
				{
					Motion = "Twohand_Attack_C"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_smash_blunt"))
				{
					Motion = "Twohand_AttackDash"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_sweeping_blunt"))
				{
					Motion = "Twohand_AttackStrong"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_strike_blunt"))
				{
					Motion = "Twohand_AttackSwing_Lower"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_dodge"))
				{
					Motion = "Twohand_Dodge"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("melee_tackle"))
				{
					Motion = "Barehand_Attack_Pursuit"
				}
			});
		}
		else if (_weaponPerformance.WeaponFramework == "lance")
		{
			GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[7]
			{
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_lance_default_a"))
				{
					Motion = "Lance_Attack_Small"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_lance_default_b"))
				{
					Motion = "Lance_Attack_B"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_lance_default_c"))
				{
					Motion = "Lance_Attack_C"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_lance_strike"))
				{
					Motion = "Lance_Attack_Deep"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_lance_dash"))
				{
					Motion = "Lance_Attack_Dash"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_strike_blunt"))
				{
					Motion = "Twohand_AttackSwing_Lower"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("melee_tackle"))
				{
					Motion = "Barehand_Attack_Pursuit"
				}
			});
		}
		else if (_weaponPerformance.WeaponFramework == "bow")
		{
			GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[6]
			{
				new BattleAction(SingletonDict<string, PlayerAction>.Get("ranged_bow_default_a"))
				{
					Motion = "Bow_Attack_Shoot"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("ranged_bow_default_b"))
				{
					Motion = "Bow_Attack_Shoot_B"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("ranged_bow_default_c"))
				{
					Motion = "Bow_Attack_Shoot_C"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("ranged_bow_quickshot"))
				{
					Motion = "Bow_Attack_Fast"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("ranged_bow_aimedshot"))
				{
					Motion = "Bow_Attack_AimedShot"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("melee_tackle"))
				{
					Motion = "Barehand_Attack_Pursuit"
				}
			});
		}
		else if (_weaponPerformance.WeaponFramework == "crossbow")
		{
			GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[4]
			{
				new BattleAction(SingletonDict<string, PlayerAction>.Get("ranged_crossbow_default"))
				{
					Motion = "Crossbow_Attack_Shoot"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("ranged_crossbow_quickshot"))
				{
					Motion = "CrossBow_Attack_Fast"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("ranged_crossbow_aimedshot"))
				{
					Motion = "Crossbow_Attack_AimedShot"
				},
				new BattleAction(SingletonDict<string, PlayerAction>.Get("melee_tackle"))
				{
					Motion = "Barehand_Attack_Pursuit"
				}
			});
		}
	}

	public void OnUseBattleAction(UseBattleAction msg)
	{
		BattleAction battleAction = GameSystem<CombatSystem>.Instance().GetBattleAction(msg.ActionId);
		if (battleAction == null)
		{
			UIManager.SystemMsg("Error", "존재하지 않는 전투액션입니다.");
			return;
		}
		_battleActionMsg = msg;
		PlayerActionAttackInfo playerActionAttackInfo = ((battleAction.Data.AttackInfo != null) ? battleAction.Data.AttackInfo.FirstOrDefault() : null);
		if (playerActionAttackInfo == null)
		{
			UIManager.SystemMsg("Error", "플레이어 액션에 오류가 발생하였습니다.");
			return;
		}
		Gauge value = new Gauge(PlayerBehavior.LocalPlayer.Stamina.Max(), 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = PlayerBehavior.LocalPlayer.Stamina.Get() - battleAction.Stamina
			}
		});
		_context.AppearPlayer.Survival.Gauges["stamina"] = value;
		OnContextChanged();
		PlayerBehavior.LocalPlayer.SetSurvivalGauge(_context.AppearPlayer.Survival.Life, _context.AppearPlayer.Survival.Gauges);
		if (_isUsingPunchMachine)
		{
			_damagedMsg = new Damaged
			{
				AttackerId = _context.AppearPlayer.EntityId,
				VictimId = msg.TargetEntityId,
				EventAt = msg.StartAt + (double)playerActionAttackInfo.AttackTime
			};
			CheckAndMakeDamageToPunchMachine(_damagedMsg);
			return;
		}
		if (!GameSystem<CombatSystem>.Instance().CombatMode)
		{
			CharacterBehavior characterBehavior = Durango.Utils.Singleton<ObjectManager>.Instance().FindCharacter(msg.TargetEntityId);
			if (characterBehavior == null)
			{
				return;
			}
			WildAnimalAI component = characterBehavior.GetComponent<WildAnimalAI>();
			if (component == null)
			{
				return;
			}
			characterBehavior.GetComponent<WildAnimalAI>().RemoveActivatedAi();
			component.SetAiActivated();
			GameSystem<CombatSystem>.Instance().SelectTarget(msg.TargetEntityId);
			Connections.Frontend.PushPacket(new BattleBegun
			{
				EntityId = _context.AppearPlayer.EntityId,
				EnemyId = msg.TargetEntityId
			});
		}
		_targetEntityId = msg.TargetEntityId;
		_damagedMsg = new Damaged
		{
			AttackerId = _context.AppearPlayer.EntityId,
			VictimId = msg.TargetEntityId,
			EventAt = msg.StartAt + (double)playerActionAttackInfo.AttackTime
		};
		CheckAndMakeDamageToAnimal(_damagedMsg);
	}

	public void ButcheryAnimal(Collect msg, PacketHeader header)
	{
		new List<string>();
		List<Item> list = new List<Item>();
		Messages.SkillCategory value = default(Messages.SkillCategory);
		_context.Skills.TryGetValue(Shared.Skill.Category.Butchery, out value);
		CollectibleChanged changed = default(CollectibleChanged);
		changed.EntityId = msg.EntityId;
		_ = value.Level / 4;
		Result result = Result.Invalid;
		int num2 = new global::System.Random().Next(1, 100);
		if (num2 < 5)
		{
			result = Result.BigFailure;
		}
		if (num2 > 5 && num2 < 10)
		{
			result = Result.Failure;
		}
		if (num2 > 10 && num2 < 85)
		{
			result = Result.Success;
		}
		if (num2 > 85)
		{
			result = Result.GreatSuccess;
		}
		Item? item = GenItemButchery(msg.GeneratorId, msg.Level, result);
		if (item.HasValue)
		{
			Item item2 = default(Item);
			item2 = item.Value;
			item2.Name = _generators.Find((Generator o) => o.Id == msg.GeneratorId).Name;
			list.Add(item2);
		}
		Send(new Messages.Timer
		{
			Duration = 2f
		}, header.Seq);
		global::System.Timers.Timer timer = new global::System.Timers.Timer();
		timer.Interval = 2000.0;
		timer.Enabled = true;
		timer.AutoReset = false;
		timer.Elapsed += delegate
		{
			if (num2 > 5)
			{
				AddItems(list);
			}
			Generator generator = default(Generator);
			int index = _generators.FindIndex((Generator o) => o.Id == msg.GeneratorId);
			generator = _generators[index];
			generator.Amount = _generators[index].Amount - 1;
			_generators[index] = generator;
			if (generator.Amount == 0)
			{
				_generators.RemoveAt(index);
			}
			Collectible value2 = _world._context.CollectedFrom[msg.EntityId.ToString()];
			value2.Generators = _generators.ToArray();
			_world._context.CollectedFrom[msg.EntityId.ToString()] = value2;
			DateTime value3 = DateTime.Now.AddDays(2.0);
			if (!_world._context.ActionTimer.ContainsKey(msg.EntityId.ToString()))
			{
				_world._context.ActionTimer.Add(msg.EntityId.ToString(), value3);
			}
			else
			{
				_world._context.ActionTimer.Remove(msg.EntityId.ToString());
				_world._context.ActionTimer.Add(msg.EntityId.ToString(), value3);
			}
			Send(changed);
			SendCollected(list, result, header);
			_world.Save();
		};
	}

	public Item? GenItemButchery(string prototypeId, int level, Result result)
	{
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(prototypeId);
		Item? result2;
		if (itemPrototype == null)
		{
			result2 = null;
		}
		else
		{
			Messages.SkillCategory value = default(Messages.SkillCategory);
			_context.Skills.TryGetValue(Shared.Skill.Category.Butchery, out value);
			int num = value.Level / 10;
			Item item = default(Item);
			item.Id = Guid.NewGuid().ToString();
			item.FounderId = _context.PlayerInfo.PlayerName;
			item.FounderCategory = string.Empty;
			item.Durability = new Gauge(1f, 0f, new GaugeNode[1]
			{
				new GaugeNode(0.0, 1f)
			});
			item.Size = itemPrototype.Size;
			item.Unstable = false;
			item.ModifiableCount = 5 + num;
			item.ModifiedCount = 0;
			Item value2 = item;
			int hashCode = value2.Id.GetHashCode();
			ItemIconTex.TryGetDefaultColor(itemPrototype.ColorR, out var col, hashCode, Color.black);
			ItemIconTex.TryGetDefaultColor(itemPrototype.ColorG, out var col2, hashCode, Color.gray);
			ItemIconTex.TryGetDefaultColor(itemPrototype.ColorB, out col2, hashCode, Color.black);
			value2.ColorR = col.ToHex();
			value2.ColorG = col.ToHex();
			value2.ColorB = col.ToHex();
			value2.Icon = itemPrototype.Icon;
			value2.Prototype = prototypeId;
			value2.Level = value.Level;
			value2.Name = itemPrototype.Name;
			value2.Description = itemPrototype.Description;
			value2.Tags = TagListGenItem(itemPrototype, result).ToArray();
			List<Performance> list = new List<Performance>();
			if (PerformanceYaml.TryGetAddOnModelKey(prototypeId, out var modelKey))
			{
				list.Add(new Performance
				{
					Id = "add_on",
					Strs = new Dictionary<string, string> { { "add_on_model_key", modelKey } }
				});
			}
			PerformanceYaml.Weapon weapon = PerformanceYaml.GetWeapon(prototypeId);
			if (weapon != null)
			{
				list.Add(new Performance
				{
					Id = "weapon",
					Strs = new Dictionary<string, string>
					{
						{ "weapon_framework", weapon.WeaponFramework },
						{ "model", weapon.Model },
						{ "slot", weapon.Slot }
					}
				});
			}
			PerformanceYaml.Armor armor = PerformanceYaml.GetArmor(prototypeId);
			if (armor != null)
			{
				list.Add(new Performance
				{
					Id = "armor",
					Strs = new Dictionary<string, string>
					{
						{ "female_model", armor.FemaleModel },
						{ "male_model", armor.MaleModel },
						{ "slot", armor.Slot }
					}
				});
			}
			PerformanceYaml.Instrument instrument = PerformanceYaml.GetInstrument(prototypeId);
			if (instrument != null)
			{
				list.Add(new Performance
				{
					Id = "instrument",
					Strs = new Dictionary<string, string> { { "timbre", instrument.Timbre } }
				});
			}
			value2.Performance = list.ToArray();
			result2 = value2;
		}
		return result2;
	}

	public List<Generator> ButcheryGenerator(AnimalBehavior animal)
	{
		Messages.SkillCategory value = default(Messages.SkillCategory);
		_context.Skills.TryGetValue(Shared.Skill.Category.Butchery, out value);
		int num = value.Level / 10;
		List<Generator> list = new List<Generator>();
		if (_itemGenDict.Keys.Contains(animal.EntityTypeId.ToString()))
		{
			List<Generator> value2 = new List<Generator>();
			_itemGenDict.TryGetValue(animal.EntityTypeId.ToString(), out value2);
			{
				foreach (Generator item in value2)
				{
					Generator current = item;
					current.Level = value.Level;
					list.Add(current);
				}
				return list;
			}
		}
		List<string> list2 = new List<string>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		dictionary.Add("bare_hands", 1);
		foreach (Item inventoryItem in _context.InventoryItems)
		{
			list2.Add(inventoryItem.Prototype);
		}
		foreach (string item2 in list2)
		{
			Prototype itemPrototype = PrototypeYaml.GetItemPrototype(item2);
			list.Add(new Generator
			{
				Id = item2,
				Icon = itemPrototype.Icon,
				Name = itemPrototype.Name,
				Amount = new global::System.Random().Next(1, 10),
				Level = new global::System.Random().Next(10, 60),
				Duration = 2f,
				Effort = 20 - num,
				Enabled = true,
				ToolRequirements = dictionary
			});
		}
		_itemGenDict.Add(animal.EntityId, list);
		return list;
	}

	public Touched HandleTouchAnimal(Messages.Touch touch, AnimalBehavior animal)
	{
		List<Generator> list = new List<Generator>();
		Touched result = default(Touched);
		if (_world._context.CollectedFrom.ContainsKey(touch.EntityId.ToString()))
		{
			_generators = new List<Generator>();
			result.Collectible = _world._context.CollectedFrom[touch.EntityId.ToString()];
			Generator[] generators = _world._context.CollectedFrom[touch.EntityId.ToString()].Generators;
			foreach (Generator item in generators)
			{
				_generators.Add(item);
			}
		}
		else
		{
			result.Collectible.EntityId = touch.EntityId;
			result.Collectible.CollectibleId = null;
			list = ButcheryGenerator(animal);
			result.Collectible.Generators = list.ToArray();
			_collectable = result.Collectible;
			_generators = list;
			_world._context.CollectedFrom.Add(touch.EntityId.ToString(), result.Collectible);
		}
		_world.Save();
		GenFileMaker(_context.PlayerSlot, _context.Path);
		return result;
	}

	public List<Item> GetPetItems(string entityId)
	{
		if (_context.PetInventories.TryGetValue(entityId, out var value))
		{
			return new List<Item>(value);
		}
		return null;
	}

	private void StartPunchMachine()
	{
		if (_punchMachineTarget == null)
		{
			return;
		}
		UIManager.MessageBox.Show(T._("펀치 머신에 도전 하시겠습니까?"), T._("\"펀치 머신도 한 대 맞기 전까진 계획이 있었다.\""), delegate(int index)
		{
			if (index == 0)
			{
				if (InventorySystem.Wallet.GetBalance(Currency.TStone) >= _punchingGamePrice)
				{
					_isUsingPunchMachine = true;
					SoundManager.PlayEvent(_startGameAudio);
					Artifact punchMachineTarget = _punchMachineTarget;
					GameSystem<CombatSystem>.Instance().CombatMode = true;
					GameSystem<CombatSystem>.Instance().SelectTarget(new ArtifactDamageableEntity(punchMachineTarget));
				}
				else
				{
					UIManager.SystemMsg("이용료가 부족합니다.");
				}
			}
		}, new MessageBox.Button
		{
			Text = T._("도전 <t_stone> {0}", _punchingGamePrice),
			Style = PresetButton.Style.Solid
		}, new MessageBox.Button
		{
			Text = T._("취소"),
			Style = PresetButton.Style.Border
		});
	}

	public void FinishPunchMachine(Damage damage, bool isFastAtk)
	{
		GameSystem<CombatSystem>.Instance().ClearTarget();
		if (GameSystem<CombatSystem>.Instance().CombatMode)
		{
			Connections.Frontend.PushPacket(new BattleEnded
			{
				EntityId = _context.AppearPlayer.EntityId
			});
		}
		if (isFastAtk)
		{
			UIManager.SystemMsg("플레이어 <em>" + PlayerBehavior.LocalPlayer.PlayerName + "</em>님의 펀치머신 점수는 <em>" + (_firstFastAtk + _secondFastAtk + _lastFastAtk) + "</em>점 입니다.");
			SendPunchRankingUserInfo(_firstFastAtk + _secondFastAtk + _lastFastAtk);
		}
		else
		{
			UIManager.SystemMsg("플레이어 <em>" + PlayerBehavior.LocalPlayer.PlayerName + "</em>님의 펀치머신 점수는 <em>" + damage.Value + "</em>점 입니다.");
			SendPunchRankingUserInfo(damage.Value);
		}
		_isUsingPunchMachine = false;
	}

	public void SendWildAnimals()
	{
	}

	public void FindSailingRoute(GetRoutes routes)
	{
		GenericSelector genericSelector = UIManager.Popup.Tooltip<GenericSelector>();
		genericSelector.ResetArguments();
		genericSelector.SetTitle("항해");
		if (_world._context.IsSailingUnstable)
		{
			genericSelector.AddItem("개인섬");
		}
		genericSelector.AddItem("사바나 해역");
		genericSelector.AddItem("열대 해역");
		genericSelector.AddItem("온대 해역");
		genericSelector.AddItem("툰드라 해역");
		genericSelector.AddItem("사막 해역");
		genericSelector.AddItem("늪 해역");
		genericSelector.AddItem("설원 해역");
		genericSelector.AddItem("화산 해역");
		genericSelector.SetSelected(delegate(int index)
		{
			if (_world._context.IsSailingUnstable)
			{
				switch (index)
				{
				case 0:
					UIManager.MessageBox.Show(T._("개인섬으로 돌아가기"), string.Format("<alert_icon/> {0}", T._("현재 테스트 중인 버전입니다. 파일 손상의 우려가 있으니 반드시 AppData 폴더를 백업 후 이용해주세요.")), delegate(int index2)
					{
						if (index2 == 0)
						{
							BackToStableIsland(_context.PlayerSlot, _world._context.StableTerrainId);
						}
					}, new MessageBox.Button(T._("돌아가기")), T._("취소"));
					break;
				case 1:
					UIManager.MessageBox.Show(T._("사바나 해역 항해"), string.Format("<alert_icon/> {0}", T._("현재 테스트 중인 버전입니다. 파일 손상의 우려가 있으니 반드시 AppData 폴더를 백업 후 이용해주세요.")), delegate(int index2)
					{
						switch (index2)
						{
						case 0:
							SailUnstableIsland(_context.PlayerSlot, "ri45sa", 15, routes.Tile);
							break;
						case 1:
							SailUnstableIsland(_context.PlayerSlot, "ri45sa", 45, routes.Tile);
							break;
						}
					}, new MessageBox.Button(T._("Lv. 15 불안정 사바나 섬 항해")), new MessageBox.Button(T._("Lv. 45 불안정 사바나 섬 항해")), T._("취소"));
					break;
				case 2:
					UIManager.MessageBox.Show(T._("열대 해역 항해"), string.Format("<alert_icon/> {0}", T._("현재 테스트 중인 버전입니다. 파일 손상의 우려가 있으니 반드시 AppData 폴더를 백업 후 이용해주세요.")), delegate(int index2)
					{
						switch (index2)
						{
						case 0:
							SailUnstableIsland(_context.PlayerSlot, "ri40tr", 18, routes.Tile);
							break;
						case 1:
							SailUnstableIsland(_context.PlayerSlot, "ri40tr", 25, routes.Tile);
							break;
						case 2:
							SailUnstableIsland(_context.PlayerSlot, "ri40tr", 40, routes.Tile);
							break;
						case 3:
							SailUnstableIsland(_context.PlayerSlot, "ri40tr", 55, routes.Tile);
							break;
						case 4:
							SailUnstableIsland(_context.PlayerSlot, "ri40tr", 60, routes.Tile);
							break;
						}
					}, new MessageBox.Button(T._("Lv. 18 불안정 열대 섬 항해")), new MessageBox.Button(T._("Lv. 25 불안정 열대 섬 항해")), new MessageBox.Button(T._("Lv. 40 불안정 열대 섬 항해")), new MessageBox.Button(T._("Lv. 55 불안정 푸른열대 섬 항해")), new MessageBox.Button(T._("Lv. 60 불안정 열대 섬 항해")), T._("취소"));
					break;
				case 3:
					UIManager.MessageBox.Show(T._("온대 해역 항해"), string.Format("<alert_icon/> {0}", T._("현재 테스트 중인 버전입니다. 파일 손상의 우려가 있으니 반드시 AppData 폴더를 백업 후 이용해주세요.")), delegate(int index2)
					{
						switch (index2)
						{
						case 0:
							SailUnstableIsland(_context.PlayerSlot, "ri35te", 20, routes.Tile);
							break;
						case 1:
							SailUnstableIsland(_context.PlayerSlot, "ri35te", 35, routes.Tile);
							break;
						}
					}, new MessageBox.Button(T._("Lv. 20 불안정 온대 섬 항해")), new MessageBox.Button(T._("Lv. 35 불안정 온대 섬 항해")), T._("취소"));
					break;
				}
			}
			else
			{
				if (index == 0)
				{
					UIManager.MessageBox.Show(T._("사바나 해역 항해"), string.Format("<alert_icon/> {0}", T._("현재 테스트 중인 버전입니다. 파일 손상의 우려가 있으니 반드시 AppData 폴더를 백업 후 이용해주세요.")), delegate(int index2)
					{
						switch (index2)
						{
						case 0:
							SailUnstableIsland(_context.PlayerSlot, "ri45sa", 15, routes.Tile);
							break;
						case 1:
							SailUnstableIsland(_context.PlayerSlot, "ri45sa", 45, routes.Tile);
							break;
						}
					}, new MessageBox.Button(T._("Lv. 15 불안정 사바나 섬 항해")), new MessageBox.Button(T._("Lv. 45 불안정 사바나 섬 항해")), T._("취소"));
				}
				switch (index)
				{
				case 1:
					UIManager.MessageBox.Show(T._("열대 해역 항해"), string.Format("<alert_icon/> {0}", T._("현재 테스트 중인 버전입니다. 파일 손상의 우려가 있으니 반드시 AppData 폴더를 백업 후 이용해주세요.")), delegate(int index2)
					{
						switch (index2)
						{
						case 0:
							SailUnstableIsland(_context.PlayerSlot, "ri40tr", 18, routes.Tile);
							break;
						case 1:
							SailUnstableIsland(_context.PlayerSlot, "ri40tr", 25, routes.Tile);
							break;
						case 2:
							SailUnstableIsland(_context.PlayerSlot, "ri40tr", 40, routes.Tile);
							break;
						case 3:
							SailUnstableIsland(_context.PlayerSlot, "ri40tr", 55, routes.Tile);
							break;
						case 4:
							SailUnstableIsland(_context.PlayerSlot, "ri40tr", 60, routes.Tile);
							break;
						}
					}, new MessageBox.Button(T._("Lv. 18 불안정 열대 섬 항해")), new MessageBox.Button(T._("Lv. 25 불안정 열대 섬 항해")), new MessageBox.Button(T._("Lv. 40 불안정 열대 섬 항해")), new MessageBox.Button(T._("Lv. 55 불안정 푸른열대 섬 항해")), new MessageBox.Button(T._("Lv. 60 불안정 열대 섬 항해")), T._("취소"));
					break;
				case 2:
					UIManager.MessageBox.Show(T._("온대 해역 항해"), string.Format("<alert_icon/> {0}", T._("현재 테스트 중인 버전입니다. 파일 손상의 우려가 있으니 반드시 AppData 폴더를 백업 후 이용해주세요.")), delegate(int index2)
					{
						switch (index2)
						{
						case 0:
							SailUnstableIsland(_context.PlayerSlot, "ri35te", 20, routes.Tile);
							break;
						case 1:
							SailUnstableIsland(_context.PlayerSlot, "ri35te", 35, routes.Tile);
							break;
						}
					}, new MessageBox.Button(T._("Lv. 20 불안정 온대 섬 항해")), new MessageBox.Button(T._("Lv. 35 불안정 온대 섬 항해")), T._("취소"));
					break;
				}
			}
		});
		genericSelector.Show();
	}

	public void SailUnstableIsland(int slot, string id, int level, Point2 portTile)
	{
		UnstableIslandLoader(slot, id, level, _world._context.Path, portTile);
		new Cluster
		{
			OnRequestAccount = delegate(Action<Account> action)
			{
				Account account = new Account();
				account.MaxPlayerSlotCount = 7;
				account.PlayerSlotCount = 1;
				account.Players = new List<Durango.Logic.Clusters.PlayerInfo>();
				account.Players.Add(_context.PlayerInfo);
				action?.Invoke(account);
			},
			GatewayUrlRoot = "http://127.0.0.1:" + Server.GetIslandPort(),
			LocalPlayer = Json.Write(_context)
		};
		GameManager.Emigrated = GameManager.EmigratedType.Explore;
		Server.BeginServer(_world._context, _context);
		Durango.Utils.Singleton<GameManager>.Instance().MoveToTitle();
	}

	public void UnstableIslandInit(int slot, string id, int level, Point2 portTile)
	{
		GameManager.Region.Template.Level = level;
		_unstableIslandContext.PlayerSlot = slot;
		_unstableIslandContext.TerrainId = id;
		_unstableIslandContext.StableTerrainId = _stableIslandContext.TerrainId;
		_unstableIslandContext.IsSailingUnstable = true;
		if (_unstableIslandContext.Artifacts == null)
		{
			_unstableIslandContext.Artifacts = new Dictionary<string, AppearArtifact>();
		}
		if (_unstableIslandContext.BoxInventories == null)
		{
			_unstableIslandContext.BoxInventories = new Dictionary<string, List<Item>>();
		}
		if (_unstableIslandContext.ArtifactAddOns == null)
		{
			_unstableIslandContext.ArtifactAddOns = new Dictionary<string, AddOns>();
		}
		if (_unstableIslandContext.ArtifactMannequins == null)
		{
			_unstableIslandContext.ArtifactMannequins = new Dictionary<string, Messages.Mannequin>();
		}
		if (_unstableIslandContext.AddedNatural == null)
		{
			_unstableIslandContext.AddedNatural = new List<NaturalInfo>();
		}
		if (_unstableIslandContext.RemovedNatural == null)
		{
			_unstableIslandContext.RemovedNatural = new List<Point2>();
		}
		if (_unstableIslandContext.GrazedPetList == null)
		{
			_unstableIslandContext.GrazedPetList = new List<Messages.Pet>();
		}
		if (_unstableIslandContext.CollectedFrom == null)
		{
			_unstableIslandContext.CollectedFrom = new Dictionary<string, Collectible>();
		}
		if (_unstableIslandContext.ActionTimer == null)
		{
			_unstableIslandContext.ActionTimer = new Dictionary<string, DateTime>();
		}
		if (_unstableIslandContext.WildAnimalList == null)
		{
			_unstableIslandContext.WildAnimalList = new List<AppearAnimal>();
		}
		string entityId = Guid.NewGuid().ToString();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("common", "dock_01_wood");
		AppearArtifact appearArtifact = default(AppearArtifact);
		appearArtifact.EntityId = entityId;
		appearArtifact.EntityType = 7001;
		appearArtifact.IsAlive = false;
		appearArtifact.Tile = portTile;
		appearArtifact.Size = new Point2
		{
			x = 3,
			y = 3
		};
		appearArtifact.Rotation = Rotation.Quarter;
		appearArtifact.Display = new ArtifactDisplay
		{
			EntityId = entityId,
			Parts = dictionary
		};
		appearArtifact.States = new ArtifactState
		{
			BuildingState = BuildingState.Completed
		};
		AppearArtifact value = appearArtifact;
		_unstableIslandContext.Artifacts.Add(value.EntityId, value);
		string contents = Json.Write(_unstableIslandContext, indented: true);
		File.WriteAllText(string.Concat(Path.GetDirectoryName(_world._context.Path), "\\", slot, "." + id), contents);
		File.WriteAllText(_world._context.Path, contents);
	}

	public void UnstableIslandMaker(int slot, string id, int level, string path, Point2 portTile)
	{
		try
		{
			_stableIslandContext = _world._context;
			byte[] bytes = Json.WriteToBytes(_stableIslandContext, indented: true);
			File.WriteAllBytes(string.Concat(Path.GetDirectoryName(path), "\\", slot, "." + _world._context.TerrainId), bytes);
			_unstableIslandContext = new WorldContext();
			UnstableIslandInit(slot, id, level, portTile);
			byte[] bytes2 = Json.WriteToBytes(_unstableIslandContext, indented: true);
			File.WriteAllBytes(string.Concat(Path.GetDirectoryName(path), "\\", slot, "." + id), bytes2);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void UnstableIslandLoader(int slot, string id, int level, string path, Point2 portTile)
	{
		try
		{
			_stableIslandContext = _world._context;
			byte[] bytes = Json.WriteToBytes(_stableIslandContext, indented: true);
			File.WriteAllBytes(string.Concat(Path.GetDirectoryName(path), "\\", slot, "." + _world._context.TerrainId), bytes);
			if (File.Exists(string.Concat(Path.GetDirectoryName(path), "\\", slot, "." + id)))
			{
				_unstableIslandContext = new WorldContext();
				UnstableIslandInit(slot, id, level, portTile);
				string contents = Json.Write(_unstableIslandContext, indented: true);
				File.WriteAllText(_world._context.Path, contents);
			}
			else
			{
				UnstableIslandMaker(slot, id, level, path, portTile);
				UIManager.SystemMsg("불안정섬 파일이 존재하지 않아 파일을 새로 생성했습니다.");
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void WarpToPort()
	{
		Send(new Messages.Timer
		{
			Duration = 2f
		});
		global::System.Timers.Timer timer = new global::System.Timers.Timer();
		timer.Interval = 2000.0;
		timer.Enabled = true;
		timer.AutoReset = false;
		timer.Elapsed += delegate
		{
			List<AppearArtifact> list = _world._context.Artifacts.Values.ToList().FindAll((AppearArtifact p) => p.EntityType == 7001);
			if (list.Count != 0)
			{
				if (list.Count >= 2)
				{
					int x = PlayerBehavior.LocalPlayer.CurrentTile.x;
					int y = PlayerBehavior.LocalPlayer.CurrentTile.y;
					List<double> distanceList = new List<double>();
					foreach (AppearArtifact item in list)
					{
						int num = item.Tile.x - x;
						int num2 = item.Tile.y - y;
						distanceList.Add(Math.Sqrt(Math.Pow(num, 2.0) + Math.Pow(num2, 2.0)));
					}
					distanceList.Sort();
					int index = list.FindIndex((AppearArtifact p) => Math.Sqrt(Math.Pow((double)p.Tile.x - (double)x, 2.0) + Math.Pow((double)p.Tile.y - (double)y, 2.0)) == distanceList[0]);
					Connections.Frontend.Send(new Cheat
					{
						_Cheat = $"m {list[index].Tile.x} {list[index].Tile.y}"
					});
				}
				else
				{
					int index2 = list.FindIndex((AppearArtifact p) => p.EntityType == 7001);
					Connections.Frontend.Send(new Cheat
					{
						_Cheat = $"m {list[index2].Tile.x} {list[index2].Tile.y}"
					});
				}
			}
		};
	}

	private void TestInteraction()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.TestInteraction, delegate
		{
			UIManager.SystemMsg(Server._isOpenedKylloxServer.ToString());
			UIManager.SystemMsg(Server._isConnectingKylloxServer.ToString());
			UIManager.SystemMsg(Server._localPlayer.IsConnectedKylloxServer.ToString());
			UIManager.SystemMsg(_context.IsConnectedKylloxServer.ToString());
		});
	}

	public void SendMultiSystem()
	{
		if (Server._isConnectingKylloxServer)
		{
			_context.IsConnectedKylloxServer = true;
		}
	}

	public void CheckAndMakeDamageToAnimal(Damaged msg)
	{
		CharacterBehavior characterBehavior = Durango.Utils.Singleton<ObjectManager>.Instance().FindCharacter(msg.VictimId);
		if (characterBehavior == null)
		{
			UIManager.SystemMsg("Error", "타겟이 존재하지 않습니다.");
			return;
		}
		WildAnimalAI component = characterBehavior.GetComponent<WildAnimalAI>();
		if (component == null)
		{
			UIManager.SystemMsg("Error", "오류가 발생하였습니다.");
			return;
		}
		_WildAnimalAI = component;
		BattleAction battleAction = GameSystem<CombatSystem>.Instance().GetBattleAction(_battleActionMsg.ActionId);
		if (battleAction == null)
		{
			UIManager.SystemMsg("Error", "존재하지 않는 전투액션입니다.");
			return;
		}
		string id = battleAction.Data.Id;
		if (id == "twohand_dodge" || id == "barehand_dodge")
		{
			return;
		}
		if (((battleAction.Data.AttackInfo != null) ? battleAction.Data.AttackInfo.FirstOrDefault() : null) == null)
		{
			UIManager.SystemMsg("Error", "플레이어 액션에 오류가 발생하였습니다.");
			return;
		}
		_damageMsg = CalcDamageResult(msg.VictimId);
		_victimId = msg.VictimId;
		switch (_weaponPerformance.WeaponFramework)
		{
		case "barehand":
			SendBareHandsAttack();
			break;
		case "onehand":
			SendOneHandWeaponAttack();
			break;
		case "twohand":
			SendTwoHandWeaponAttack();
			break;
		case "lance":
			SendSpearWeaponAttack();
			break;
		case "bow":
			SendBowWeaponAttack();
			break;
		case "crossbow":
			SendBowWeaponAttack();
			break;
		}
	}

	private float CalcWeaponDamage(string attackType)
	{
		if (!GameSystem<CombatSystem>.Instance().CombatMode)
		{
			return 0f;
		}
		BattleAction battleAction = GameSystem<CombatSystem>.Instance().GetBattleAction(_battleActionMsg.ActionId);
		if (battleAction == null)
		{
			UIManager.SystemMsg("Error", "존재하지 않는 전투액션입니다.");
			return 0f;
		}
		if (((battleAction.Data.AttackInfo != null) ? battleAction.Data.AttackInfo.FirstOrDefault() : null) == null)
		{
			UIManager.SystemMsg("Error", "플레이어 액션에 오류가 발생하였습니다.");
			return 0f;
		}
		float num = 1f;
		if (!_isUsingPunchMachine)
		{
			CharacterBehavior characterBehavior = Durango.Utils.Singleton<ObjectManager>.Instance().FindCharacter(_targetEntityId);
			if (characterBehavior == null)
			{
				UIManager.SystemMsg("Error", "타겟이 존재하지 않습니다.");
				return 0f;
			}
			WildAnimalAI component = characterBehavior.GetComponent<WildAnimalAI>();
			if (component == null)
			{
				UIManager.SystemMsg("Error", "오류가 발생하였습니다.");
				return 0f;
			}
			num = component._defenseValue;
		}
		Item equippedWeaponInfo = GetEquippedWeaponInfo();
		List<Messages.Tag> list = new List<Messages.Tag>();
		list.AddRange(equippedWeaponInfo.Tags);
		if (_damageMsg.Effects == DamageEffects.Critical)
		{
			int num2 = list.FindIndex((Messages.Tag x) => x.Id == "sadism");
			if (num2 != -1)
			{
				_sadismLevel = list[num2].Level;
				_isSadism = true;
			}
			int num3 = list.FindIndex((Messages.Tag x) => x.Id == "blood_burst");
			if (num3 != -1)
			{
				_bloodBurstLevel = list[num3].Level;
				_isBloodBurst = true;
			}
		}
		float[] array = CalcWeaponAttack();
		switch (attackType)
		{
		case "normal":
			if (_damageMsg.Effects == DamageEffects.Blow)
			{
				_attackDamage = UnityEngine.Random.Range(array[0], array[1]);
			}
			else if (_damageMsg.Effects == DamageEffects.Critical)
			{
				_attackDamage = UnityEngine.Random.Range(array[1], array[2]);
			}
			else if (_damageMsg.Effects == DamageEffects.CrossCounter)
			{
				_attackDamage = UnityEngine.Random.Range(array[1] * 0.7f, array[2] * 0.7f);
			}
			else
			{
				_attackDamage = UnityEngine.Random.Range(array[0] * 0.5f, array[1] * 0.5f);
			}
			break;
		case "fast":
			if (_curFastAttackCount == 0)
			{
				if (_damageMsg.Effects == DamageEffects.Blow)
				{
					_attackDamage = UnityEngine.Random.Range(array[0] * 0.7f, array[1] * 0.7f);
				}
				else if (_damageMsg.Effects == DamageEffects.Critical)
				{
					_attackDamage = UnityEngine.Random.Range(array[1] * 0.7f, array[2] * 0.7f);
				}
				else if (_damageMsg.Effects == DamageEffects.CrossCounter)
				{
					_attackDamage = UnityEngine.Random.Range(array[1] * 0.7f, array[2] * 0.7f);
				}
				else
				{
					_attackDamage = UnityEngine.Random.Range(array[0] * 0.7f * 0.5f, array[1] * 0.7f * 0.5f);
				}
				_firstFastAtk = (int)_attackDamage;
			}
			else if (_curFastAttackCount == 1)
			{
				if (_damageMsg.Effects == DamageEffects.Blow)
				{
					_attackDamage = UnityEngine.Random.Range(array[0] * 0.78f, array[1] * 0.78f);
				}
				else if (_damageMsg.Effects == DamageEffects.Critical)
				{
					_attackDamage = UnityEngine.Random.Range(array[1] * 0.78f, array[2] * 0.78f);
				}
				else if (_damageMsg.Effects == DamageEffects.CrossCounter)
				{
					_attackDamage = UnityEngine.Random.Range(array[1] * 0.78f * 0.7f, array[2] * 0.78f * 0.7f);
				}
				else
				{
					_attackDamage = UnityEngine.Random.Range(array[0] * 0.78f * 0.5f, array[1] * 1.08f * 0.78f);
				}
				_secondFastAtk = (int)_attackDamage;
			}
			else if (_curFastAttackCount == 2)
			{
				if (_damageMsg.Effects == DamageEffects.Blow)
				{
					_attackDamage = UnityEngine.Random.Range(array[0] * 0.94f, array[1] * 0.94f);
				}
				else if (_damageMsg.Effects == DamageEffects.Critical)
				{
					_attackDamage = UnityEngine.Random.Range(array[1] * 0.94f, array[2] * 0.94f);
				}
				else if (_damageMsg.Effects == DamageEffects.CrossCounter)
				{
					_attackDamage = UnityEngine.Random.Range(array[1] * 0.94f * 0.7f, array[2] * 0.94f * 0.7f);
				}
				else
				{
					_attackDamage = UnityEngine.Random.Range(array[0] * 0.94f * 0.5f, array[1] * 0.94f * 0.5f);
				}
				_lastFastAtk = (int)_attackDamage;
			}
			break;
		case "strong":
			if (_damageMsg.Effects == DamageEffects.Blow)
			{
				_attackDamage = UnityEngine.Random.Range(array[0] * 2.2f, array[1] * 2.2f);
			}
			else if (_damageMsg.Effects == DamageEffects.Critical)
			{
				_attackDamage = UnityEngine.Random.Range(array[1] * 2.2f, array[2] * 2.2f);
			}
			else if (_damageMsg.Effects == DamageEffects.CrossCounter)
			{
				_attackDamage = UnityEngine.Random.Range(array[1] * 2.2f * 0.7f, array[2] * 2.2f * 0.7f);
			}
			else
			{
				_attackDamage = UnityEngine.Random.Range(array[0] * 2.2f * 0.5f, array[1] * 2.2f * 0.5f);
			}
			break;
		case "stab":
			if (_damageMsg.Effects == DamageEffects.Blow)
			{
				_attackDamage = UnityEngine.Random.Range(array[0] * 2.8f, array[1] * 2.8f);
			}
			else if (_damageMsg.Effects == DamageEffects.Critical)
			{
				_attackDamage = UnityEngine.Random.Range(array[1] * 2.8f, array[2] * 2.8f);
			}
			else if (_damageMsg.Effects == DamageEffects.CrossCounter)
			{
				_attackDamage = UnityEngine.Random.Range(array[1] * 2.8f * 0.7f, array[2] * 2.8f * 0.7f);
			}
			else
			{
				_attackDamage = UnityEngine.Random.Range(array[0] * 2.8f * 0.5f, array[1] * 2.8f * 0.5f);
			}
			break;
		case "tackle":
			if (_damageMsg.Effects == DamageEffects.Blow)
			{
				_attackDamage = UnityEngine.Random.Range(array[0] * 0.2f, array[0] * 0.5f);
			}
			else if (_damageMsg.Effects == DamageEffects.Critical)
			{
				_attackDamage = UnityEngine.Random.Range(array[0] * 0.3f, array[0] * 0.5f);
			}
			else
			{
				_attackDamage = UnityEngine.Random.Range(array[0] * 0.1f, array[0] * 0.2f);
			}
			break;
		}
		if (_isSadism)
		{
			global::System.Timers.Timer timer = new global::System.Timers.Timer();
			timer.Interval = 500.0;
			timer.Enabled = true;
			timer.AutoReset = false;
			timer.Elapsed += delegate
			{
				if (!_isAlreadySadism)
				{
					_sadismTimer = new global::System.Timers.Timer(125.0);
					_sadismTimer.Enabled = true;
					_sadismTimer.AutoReset = true;
					_sadismTimer.Elapsed += OnSadismTimedEvent;
					Send(new Messages.Timer
					{
						Duration = 8f
					});
					global::System.Timers.Timer timer2 = new global::System.Timers.Timer();
					timer2.Interval = 8000.0;
					timer2.Enabled = true;
					timer2.AutoReset = false;
					timer2.Elapsed += delegate
					{
						_sadismTimer.Stop();
						_isSadism = false;
						_isAlreadySadism = false;
						RemovePlayerStatusEffect(_context.AppearPlayer.EntityId, "sadism");
						UpdateSurvival();
					};
				}
			};
		}
		return _attackDamage * num;
	}

	public void CheckAndMakeDamageToPunchMachine(Damaged msg)
	{
		BattleAction battleAction = GameSystem<CombatSystem>.Instance().GetBattleAction(_battleActionMsg.ActionId);
		if (battleAction == null)
		{
			UIManager.SystemMsg("Error", "존재하지 않는 전투액션입니다.");
		}
		else if (((battleAction.Data.AttackInfo != null) ? battleAction.Data.AttackInfo.FirstOrDefault() : null) == null)
		{
			UIManager.SystemMsg("Error", "플레이어 액션에 오류가 발생하였습니다.");
		}
		else if (battleAction.Data.Id.IndexOf("dodge") == -1)
		{
			_damageMsg = CalcDamageResult(msg.VictimId);
			_victimId = msg.VictimId;
			switch (_weaponPerformance.WeaponFramework)
			{
			case "barehand":
				SendBareHandsAttack();
				break;
			case "onehand":
				SendOneHandWeaponAttack();
				break;
			case "twohand":
				SendTwoHandWeaponAttack();
				break;
			case "lance":
				SendSpearWeaponAttack();
				break;
			case "bow":
				SendBowWeaponAttack();
				break;
			case "crossbow":
				SendBowWeaponAttack();
				break;
			}
		}
	}

	private float[] CalcWeaponAttack()
	{
		Item equippedWeaponInfo = GetEquippedWeaponInfo();
		MathParser mathParser = new MathParser();
		string text = _weaponPerformance.Attack.Trim().Replace("level", equippedWeaponInfo.Level.ToString());
		int num = text.Count((char f) => f == '(');
		if (num == 1)
		{
			text = text.Replace("(", "").Replace(")", "");
		}
		else
		{
			int num2 = text.IndexOf("(", 1);
			if (num == 2 && num2 != -1)
			{
				text = text.Remove(num2, 1).Replace("))", ")");
			}
		}
		string text2 = _weaponPerformance.AttackRating.Trim().Replace("level", equippedWeaponInfo.Level.ToString());
		if (text2.Count((char f) => f == '(') == 1)
		{
			text2 = text2.Replace("(", "").Replace(")", "");
		}
		else
		{
			int num3 = text2.IndexOf("(", 1);
			if (num == 2 && num3 != -1)
			{
				text2 = text2.Remove(num3, 1).Replace("))", ")");
			}
		}
		List<Messages.Tag> list = new List<Messages.Tag>();
		list.AddRange(equippedWeaponInfo.Tags);
		int num4 = 0;
		int num5 = 0;
		int num6 = list.FindIndex((Messages.Tag x) => x.Id == "attack_incr");
		if (num6 != -1)
		{
			num4 = list[num6].Level;
		}
		int num7 = list.FindIndex((Messages.Tag x) => x.Id == "reform_attack_incr");
		if (num7 != -1)
		{
			num5 = list[num7].Level;
		}
		float num8 = (float)num4 + (float)num5 / 7f;
		float num9 = ((num8 > 1f) ? num8 : 1f);
		float num10 = ((float)mathParser.Calculate(text) + (float)mathParser.Calculate(text2)) * num9;
		float num11 = num10 * 1.4f;
		float num12 = num10 * 2f;
		return new float[3] { num10, num11, num12 };
	}

	private void SendBareHandsAttack()
	{
		BattleAction battleAction = GameSystem<CombatSystem>.Instance().GetBattleAction(_battleActionMsg.ActionId);
		if (battleAction == null)
		{
			UIManager.SystemMsg("Error", "존재하지 않는 전투액션입니다.");
			return;
		}
		if (((battleAction.Data.AttackInfo != null) ? battleAction.Data.AttackInfo.FirstOrDefault() : null) == null)
		{
			UIManager.SystemMsg("Error", "플레이어 액션에 오류가 발생하였습니다.");
			return;
		}
		switch (battleAction.Data.Id)
		{
		case "barehand_default_a":
			_damageMsg.Value = (int)CalcWeaponDamage("normal");
			break;
		case "barehand_default_b":
			_damageMsg.Value = (int)CalcWeaponDamage("normal");
			break;
		case "barehand_default_c":
			_damageMsg.Value = (int)CalcWeaponDamage("normal");
			break;
		case "barehand_kick_a":
			_damageMsg.Value = (int)CalcWeaponDamage("strong");
			break;
		case "barehand_kick_b":
			_damageMsg.Value = (int)CalcWeaponDamage("strong");
			break;
		case "barehand_smash":
			_damageMsg.Value = (int)CalcWeaponDamage("stab");
			break;
		case "barehand_combination":
			OnUseFastAttack(3);
			return;
		case "melee_tackle":
			_damageMsg.Value = (int)CalcWeaponDamage("tackle");
			if (_damageMsg.Effects == DamageEffects.Blow && UnityEngine.Random.value <= 0.25f)
			{
				_damageMsg.Effects = DamageEffects.KnockBack;
			}
			else if (_damageMsg.Effects == DamageEffects.Critical)
			{
				_damageMsg.Effects = DamageEffects.KnockBack;
			}
			break;
		}
		Connections.Frontend.PushPacket(new Damaged
		{
			AttackerId = _context.AppearPlayer.EntityId,
			Damage = _damageMsg,
			VictimId = _victimId,
			EventAt = _damagedMsg.EventAt
		});
	}

	private void SendOneHandWeaponAttack()
	{
		BattleAction battleAction = GameSystem<CombatSystem>.Instance().GetBattleAction(_battleActionMsg.ActionId);
		if (battleAction == null)
		{
			UIManager.SystemMsg("Error", "존재하지 않는 전투액션입니다.");
			return;
		}
		if (((battleAction.Data.AttackInfo != null) ? battleAction.Data.AttackInfo.FirstOrDefault() : null) == null)
		{
			UIManager.SystemMsg("Error", "플레이어 액션에 오류가 발생하였습니다.");
			return;
		}
		switch (battleAction.Data.Id)
		{
		case "onehand_default_a":
			_damageMsg.Value = (int)CalcWeaponDamage("normal");
			break;
		case "onehand_default_b":
			_damageMsg.Value = (int)CalcWeaponDamage("normal");
			break;
		case "onehand_default_c":
			_damageMsg.Value = (int)CalcWeaponDamage("normal");
			break;
		case "onehand_smash":
			_damageMsg.Value = (int)CalcWeaponDamage("strong");
			break;
		case "onehand_flurry":
			OnUseFastAttack(3);
			return;
		case "onehand_stab":
			_damageMsg.Value = (int)CalcWeaponDamage("stab");
			break;
		case "melee_tackle":
			_damageMsg.Value = (int)CalcWeaponDamage("tackle");
			if (_damageMsg.Effects == DamageEffects.Blow && UnityEngine.Random.value <= 0.5f)
			{
				_damageMsg.Effects = DamageEffects.KnockBack;
			}
			else if (_damageMsg.Effects == DamageEffects.Critical)
			{
				_damageMsg.Effects = DamageEffects.KnockBack;
			}
			break;
		}
		Connections.Frontend.PushPacket(new Damaged
		{
			AttackerId = _context.AppearPlayer.EntityId,
			Damage = _damageMsg,
			VictimId = _victimId,
			EventAt = _damagedMsg.EventAt
		});
	}

	private void SendTwoHandWeaponAttack()
	{
		BattleAction battleAction = GameSystem<CombatSystem>.Instance().GetBattleAction(_battleActionMsg.ActionId);
		if (battleAction == null)
		{
			UIManager.SystemMsg("Error", "존재하지 않는 전투액션입니다.");
			return;
		}
		if (((battleAction.Data.AttackInfo != null) ? battleAction.Data.AttackInfo.FirstOrDefault() : null) == null)
		{
			UIManager.SystemMsg("Error", "플레이어 액션에 오류가 발생하였습니다.");
			return;
		}
		switch (battleAction.Data.Id)
		{
		case "twohand_default_a":
			_damageMsg.Value = (int)CalcWeaponDamage("normal");
			break;
		case "twohand_default_b":
			_damageMsg.Value = (int)CalcWeaponDamage("normal");
			break;
		case "twohand_default_c":
			_damageMsg.Value = (int)CalcWeaponDamage("normal");
			break;
		case "twohand_smash":
			_damageMsg.Value = (int)CalcWeaponDamage("strong");
			break;
		case "twohand_sweeping":
			OnUseFastAttack(2);
			return;
		case "twohand_strike":
			_damageMsg.Value = (int)CalcWeaponDamage("stab");
			break;
		case "melee_tackle":
			_damageMsg.Value = (int)CalcWeaponDamage("tackle");
			if (_damageMsg.Effects == DamageEffects.Blow && UnityEngine.Random.value <= 0.25f)
			{
				_damageMsg.Effects = DamageEffects.KnockBack;
			}
			else if (_damageMsg.Effects == DamageEffects.Critical)
			{
				_damageMsg.Effects = DamageEffects.KnockBack;
			}
			break;
		}
		Connections.Frontend.PushPacket(new Damaged
		{
			AttackerId = _context.AppearPlayer.EntityId,
			Damage = _damageMsg,
			VictimId = _victimId,
			EventAt = _damagedMsg.EventAt
		});
	}

	private void SendSpearWeaponAttack()
	{
	}

	private void SendBowWeaponAttack()
	{
		BattleAction battleAction = GameSystem<CombatSystem>.Instance().GetBattleAction(_battleActionMsg.ActionId);
		if (battleAction == null)
		{
			UIManager.SystemMsg("Error", "존재하지 않는 전투액션입니다.");
			return;
		}
		if (((battleAction.Data.AttackInfo != null) ? battleAction.Data.AttackInfo.FirstOrDefault() : null) == null)
		{
			UIManager.SystemMsg("Error", "플레이어 액션에 오류가 발생하였습니다.");
			return;
		}
		string id = battleAction.Data.Id;
		if (id != null)
		{
			int length = id.Length;
			if (length <= 20)
			{
				if (length != 12)
				{
					if (length == 20)
					{
						switch (id[19])
						{
						case 'a':
							if (id == "ranged_bow_default_a")
							{
								_damageMsg.Value = (int)CalcWeaponDamage("normal");
							}
							goto IL_028c;
						case 'b':
							if (id == "ranged_bow_default_b")
							{
								_damageMsg.Value = (int)CalcWeaponDamage("normal");
							}
							goto IL_028c;
						case 'c':
							if (id == "ranged_bow_default_c")
							{
								_damageMsg.Value = (int)CalcWeaponDamage("normal");
							}
							goto IL_028c;
						case 't':
							break;
						default:
							goto IL_028c;
						}
						if (id == "ranged_bow_aimedshot")
						{
							goto IL_0212;
						}
						if (id == "ranged_bow_quickshot")
						{
							goto IL_022b;
						}
					}
				}
				else if (id == "melee_tackle")
				{
					_damageMsg.Value = (int)CalcWeaponDamage("tackle");
					if (_damageMsg.Effects == DamageEffects.Blow && UnityEngine.Random.value <= 0.25f)
					{
						_damageMsg.Effects = DamageEffects.KnockBack;
					}
					else if (_damageMsg.Effects == DamageEffects.Critical)
					{
						_damageMsg.Effects = DamageEffects.KnockBack;
					}
				}
			}
			else if (length != 23)
			{
				if (length == 25)
				{
					char c = id[16];
					if (c != 'a')
					{
						if (c == 'q' && id == "ranged_crossbow_quickshot")
						{
							goto IL_022b;
						}
					}
					else if (id == "ranged_crossbow_aimedshot")
					{
						goto IL_0212;
					}
				}
			}
			else if (id == "ranged_crossbow_default")
			{
				_damageMsg.Value = (int)CalcWeaponDamage("normal");
			}
		}
		goto IL_028c;
		IL_028c:
		Connections.Frontend.PushPacket(new Damaged
		{
			AttackerId = _context.AppearPlayer.EntityId,
			Damage = _damageMsg,
			VictimId = _victimId,
			EventAt = _damagedMsg.EventAt
		});
		return;
		IL_0212:
		_damageMsg.Value = (int)CalcWeaponDamage("strong");
		goto IL_028c;
		IL_022b:
		OnUseFastAttack(3);
	}

	private Damage CalcDamageResult(string targetId)
	{
		if (!GameSystem<CombatSystem>.Instance().CombatMode)
		{
			return default(Damage);
		}
		float magnitude;
		if (_isUsingPunchMachine)
		{
			magnitude = Maths.Make2D(_punchMachineTarget.transform.position - PlayerBehavior.LocalPlayer.transform.position).magnitude;
		}
		else
		{
			CharacterBehavior characterBehavior = Durango.Utils.Singleton<ObjectManager>.Instance().FindCharacter(targetId);
			if (characterBehavior == null)
			{
				UIManager.SystemMsg("Error", "타겟이 존재하지 않습니다.");
				return default(Damage);
			}
			WildAnimalAI component = characterBehavior.GetComponent<WildAnimalAI>();
			if (component == null)
			{
				UIManager.SystemMsg("Error", "오류가 발생하였습니다.");
				return default(Damage);
			}
			magnitude = Maths.Make2D(component.TargetAnimal.transform.position - PlayerBehavior.LocalPlayer.transform.position).magnitude;
		}
		Damage result = default(Damage);
		string weaponFramework = _weaponPerformance.WeaponFramework;
		string attackType = _weaponPerformance.AttackType;
		float num = CalcWeaponAccuracy() / 1000f;
		float value = UnityEngine.Random.value;
		float num2 = 0.8f + num;
		switch (weaponFramework)
		{
		case "barehand":
			if (magnitude < 400f && value <= num2)
			{
				result.Result = DamageResult.Hit;
			}
			else
			{
				result.Result = DamageResult.Missed;
			}
			break;
		case "onehand":
			if (magnitude < 450f && value <= num2)
			{
				result.Result = DamageResult.Hit;
			}
			else
			{
				result.Result = DamageResult.Missed;
			}
			break;
		case "twohand":
			if (magnitude < 500f && value <= num2)
			{
				result.Result = DamageResult.Hit;
			}
			else
			{
				result.Result = DamageResult.Missed;
			}
			break;
		case "lance":
			if (magnitude < 650f && value <= num2)
			{
				result.Result = DamageResult.Hit;
			}
			else
			{
				result.Result = DamageResult.Missed;
			}
			break;
		case "bow":
			if (magnitude < 1000f && value <= num2)
			{
				result.Result = DamageResult.Hit;
			}
			else
			{
				result.Result = DamageResult.Missed;
			}
			break;
		case "crossbow":
			if (magnitude < 850f && value <= num2)
			{
				result.Result = DamageResult.Hit;
			}
			else
			{
				result.Result = DamageResult.Missed;
			}
			break;
		}
		if (_isBlowing)
		{
			result.Result = DamageResult.Countered;
		}
		switch (attackType)
		{
		case "bare_hands":
			result.AttackType = AttackType.BareHands;
			break;
		case "sword":
			result.AttackType = AttackType.Sword;
			break;
		case "axe":
			result.AttackType = AttackType.Axe;
			break;
		case "blunt":
			result.AttackType = AttackType.Blunt;
			break;
		case "spear":
			result.AttackType = AttackType.Spear;
			break;
		case "arrow":
			result.AttackType = AttackType.Arrow;
			break;
		}
		result.Direction = DamageDirection.Front;
		result.Part = BodyPart.Auto;
		if (result.Result == DamageResult.Missed)
		{
			result.Effects = DamageEffects.None;
		}
		else if (result.Result == DamageResult.Hit)
		{
			float value2 = UnityEngine.Random.value;
			float num3 = 0.1f + num;
			result.Effects = ((value2 <= num3) ? DamageEffects.Critical : DamageEffects.Blow);
		}
		else if (result.Result == DamageResult.Countered)
		{
			result.Effects = DamageEffects.CrossCounter;
		}
		return result;
	}

	private void OnFastAttackTimedEvent(object p0, ElapsedEventArgs p1)
	{
		BattleAction battleAction = GameSystem<CombatSystem>.Instance().GetBattleAction(_battleActionMsg.ActionId);
		if (battleAction == null)
		{
			UIManager.SystemMsg("Error", "존재하지 않는 전투액션입니다.");
		}
		else if (((battleAction.Data.AttackInfo != null) ? battleAction.Data.AttackInfo.FirstOrDefault() : null) == null)
		{
			UIManager.SystemMsg("Error", "플레이어 액션에 오류가 발생하였습니다.");
		}
		else
		{
			if (!GameSystem<CombatSystem>.Instance().CombatMode)
			{
				return;
			}
			if (!_isUsingPunchMachine)
			{
				CharacterBehavior characterBehavior = Durango.Utils.Singleton<ObjectManager>.Instance().FindCharacter(_victimId);
				if (characterBehavior == null)
				{
					UIManager.SystemMsg("Error", "타겟이 존재하지 않습니다.");
					return;
				}
				WildAnimalAI component = characterBehavior.GetComponent<WildAnimalAI>();
				if (component == null)
				{
					UIManager.SystemMsg("Error", "오류가 발생하였습니다.");
					return;
				}
				if (!component.TargetAnimal.IsAlive)
				{
					_fastAttackTimer.Stop();
					_isFastAttack = false;
					return;
				}
			}
			if (_curFastAttackCount == _fastAttackCount)
			{
				_fastAttackTimer.Stop();
				_isFastAttack = false;
				_curFastAttackCount = 0;
				return;
			}
			_isFastAttack = true;
			if (_curFastAttackCount < _fastAttackCount)
			{
				_damageMsg = CalcDamageResult(_victimId);
				_damageMsg.Value = (int)CalcWeaponDamage("fast");
				Connections.Frontend.PushPacket(new Damaged
				{
					AttackerId = _context.AppearPlayer.EntityId,
					Damage = _damageMsg,
					VictimId = _victimId,
					EventAt = _battleActionMsg.StartAt + _fastAttackTimer.Interval / 1000.0
				});
				_curFastAttackCount++;
			}
		}
	}

	private void OnUseFastAttack(int attackCount)
	{
		BattleAction battleAction = GameSystem<CombatSystem>.Instance().GetBattleAction(_battleActionMsg.ActionId);
		if (battleAction == null)
		{
			UIManager.SystemMsg("Error", "존재하지 않는 전투액션입니다.");
			return;
		}
		PlayerActionAttackInfo playerActionAttackInfo = ((battleAction.Data.AttackInfo != null) ? battleAction.Data.AttackInfo.FirstOrDefault() : null);
		if (playerActionAttackInfo == null)
		{
			UIManager.SystemMsg("Error", "플레이어 액션에 오류가 발생하였습니다.");
			return;
		}
		_curFastAttackCount = 0;
		_fastAttackCount = attackCount;
		_fastAttackTimer = new global::System.Timers.Timer((double)playerActionAttackInfo.AttackTime * 1000.0);
		_fastAttackTimer.Enabled = true;
		_fastAttackTimer.AutoReset = true;
		_fastAttackTimer.Elapsed += OnFastAttackTimedEvent;
	}

	public void SendPunchRankingUserInfo(int score)
	{
		UIManager.ShowLoadingIcon(show: true);
		WebClient webClient = new WebClient();
		NameValueCollection nameValueCollection = new NameValueCollection();
		nameValueCollection["name"] = PlayerBehavior.LocalPlayer.PlayerName + "#" + PlayerBehavior.LocalPlayer.Freq;
		nameValueCollection["id"] = PlayerBehavior.LocalPlayer.EntityId;
		nameValueCollection["score"] = score.ToString();
		Item equippedWeaponInfo = GetEquippedWeaponInfo();
		string text = "";
		for (int i = 0; i < equippedWeaponInfo.Tags.Length; i++)
		{
			string[] obj = new string[5]
			{
				text,
				equippedWeaponInfo.Tags[i].Id.ToString(),
				":",
				null,
				null
			};
			int num = 3;
			int level = equippedWeaponInfo.Tags[i].Level;
			obj[num] = level.ToString();
			obj[4] = ((i < equippedWeaponInfo.Tags.Length - 1) ? " " : "");
			text = string.Concat(obj);
		}
		nameValueCollection["weapon"] = equippedWeaponInfo.Name + "," + equippedWeaponInfo.Description + "," + equippedWeaponInfo.Level + "," + equippedWeaponInfo.ColorR + "," + equippedWeaponInfo.ColorG + "," + equippedWeaponInfo.ColorB + "," + text;
		webClient.UploadValuesAsync(new Uri("http://db.kyllox.pe.kr/durango/user_infos/ranking/upload_info.php"), nameValueCollection);
		global::System.Timers.Timer timer = new global::System.Timers.Timer(1000.0);
		timer.Enabled = true;
		timer.AutoReset = false;
		timer.Elapsed += delegate
		{
			NameValueCollection nameValueCollection2 = new NameValueCollection();
			List<string> punchRankingUserInfo = GetPunchRankingUserInfo("all");
			string text2 = null;
			for (int j = 0; j < punchRankingUserInfo.Count; j++)
			{
				text2 = text2 + punchRankingUserInfo[j] + "\n";
			}
			nameValueCollection2["data"] = text2;
			webClient.UploadValuesAsync(new Uri("http://db.kyllox.pe.kr/durango/ranking/punch/infos/upload_info.php"), nameValueCollection2);
		};
		UIManager.ShowLoadingIcon(show: false);
	}

	public List<string> GetPunchRankingUserInfo(string info)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		List<int> list4 = new List<int>();
		List<string> list5 = new List<string>();
		try
		{
			UIManager.ShowLoadingIcon(show: true);
			using (StreamReader streamReader = new StreamReader(new WebClient().OpenRead("http://db.kyllox.pe.kr/durango/user_infos/ranking/users.txt"), Encoding.Default))
			{
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					string[] array = text.Split(new string[1] { "::" }, StringSplitOptions.None);
					list.Add(text);
					list2.Add(array[0]);
					list3.Add(array[1]);
					list4.Add(int.Parse(array[2]));
					list5.Add(array[3]);
				}
			}
			UIManager.ShowLoadingIcon(show: false);
		}
		catch (Exception ex)
		{
			UIManager.SystemMsg(ex.ToString());
		}
		List<int> list6 = new List<int>();
		for (int i = 0; i < list4.Count; i++)
		{
			list6.Add(list4[i]);
		}
		list6.Sort((int n1, int n2) => n2.CompareTo(n1));
		List<string> list7 = new List<string>();
		List<string> list8 = new List<string>();
		List<string> list9 = new List<string>();
		List<string> list10 = new List<string>();
		List<string> list11 = new List<string>();
		List<int> list12 = new List<int>();
		for (int j = 0; j < list6.Count; j++)
		{
			list12.Add(list6[j]);
		}
		int num = 0;
		for (int k = 0; k < list4.Count; k++)
		{
			int num2 = list4[k];
			for (int l = 0; l < list12.Count; l++)
			{
				int num3 = list12[l];
				if (num2 == num3)
				{
					num++;
				}
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			if (num >= 2)
			{
				int num4 = list4.IndexOf(list6[m]);
				while (num4 != -1 && list7.Count < list.Count)
				{
					list7.Add(list[num4]);
					list8.Add(list2[num4]);
					list9.Add(list3[num4]);
					list10.Add(list4[num4].ToString());
					list11.Add(list5[num4]);
					num4 = list4.IndexOf(list6[m], num4 + 1);
				}
			}
			else
			{
				int index = list4.IndexOf(list6[m]);
				list7.Add(list[index]);
				list8.Add(list2[index]);
				list9.Add(list3[index]);
				list10.Add(list4[index].ToString());
				list11.Add(list5[index]);
			}
		}
		return info switch
		{
			"all" => list7, 
			"name" => list8, 
			"id" => list9, 
			"score" => list10, 
			"date" => list11, 
			_ => null, 
		};
	}

	public void SendWalletUpdated(Dictionary<Currency, long> walletInfos)
	{
		Dictionary<Currency, long> unpaidBalances = new Dictionary<Currency, long>();
		VoucherInfo[] vouchers = null;
		Wallet wallet = default(Wallet);
		wallet.PaidBalances = walletInfos;
		wallet.UnpaidBalances = unpaidBalances;
		wallet.Vouchers = vouchers;
		Wallet wallet2 = wallet;
		WalletUpdated walletUpdated = default(WalletUpdated);
		walletUpdated.EntityId = _context.AppearPlayer.EntityId;
		walletUpdated.Wallet = wallet2;
		WalletUpdated msg = walletUpdated;
		Send(msg);
		_context.Wallet = wallet2;
		OnContextChanged();
	}

	private Item GetEquippedWeaponInfo()
	{
		List<Item> inventoryItems = _context.InventoryItems;
		int num = -1;
		string value2;
		if (_context.EquippedItems.TryGetValue("main", out var value))
		{
			num = inventoryItems.FindIndex((Item it) => it.Id == value);
			if (num == -1)
			{
				return default(Item);
			}
		}
		else if (_context.EquippedItems.TryGetValue("both", out value2))
		{
			num = inventoryItems.FindIndex((Item it) => it.Id == value2);
			if (num == -1)
			{
				return default(Item);
			}
		}
		_ = inventoryItems[num];
		return inventoryItems[num];
	}

	private float CalcWeaponAccuracy()
	{
		Item equippedWeaponInfo = GetEquippedWeaponInfo();
		MathParser mathParser = new MathParser();
		string text = _weaponPerformance.Accuracy.Trim().Replace("level", equippedWeaponInfo.Level.ToString());
		int num = text.Count((char f) => f == '(');
		if (num == 1)
		{
			text = text.Replace("(", "").Replace(")", "");
		}
		else
		{
			int num2 = text.IndexOf("(", 1);
			if (num == 2 && num2 != -1)
			{
				text = text.Remove(num2, 1).Replace("))", ")");
			}
		}
		List<Messages.Tag> list = new List<Messages.Tag>();
		list.AddRange(equippedWeaponInfo.Tags);
		int num3 = 0;
		int num4 = list.FindIndex((Messages.Tag x) => x.Id == "accuracy_incr");
		if (num4 != -1)
		{
			num3 = list[num4].Level;
		}
		return ((float)mathParser.Calculate(text) + (float)num3) / 2f;
	}
}
