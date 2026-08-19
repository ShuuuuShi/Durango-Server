using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Model;
using Durango.Network;
using Durango.Player;
using Durango.System;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Player;
using Shared.Teleport;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class PlayerManager : Durango.Utils.Singleton<PlayerManager>
{
	private readonly Dictionary<string, PlayerBehavior> _players = new Dictionary<string, PlayerBehavior>();

	private bool _hideOtherPlayer;

	public static bool ShowDrawLine { get; set; }

	public event Action<PlayerBehavior> PlayerAppeared;

	public event Action<PlayerBehavior> PlayerDisappeared;

	public event Action<PlayerBehavior> PlayerTitleChanged;

	public event Action<PlayerBehavior> PlayerClanChanged;

	public event Action<TeleportType> Teleported;

	public event Action<PlayerBehavior> DisplayUpdated;

	[CanBeNull]
	public PlayerBehavior GetPlayer(string id)
	{
		return _players.Get(id);
	}

	[CanBeNull]
	public PlayerBehavior GetPlayer([NotNull] Predicate<PlayerBehavior> predicate)
	{
		foreach (PlayerBehavior value in _players.Values)
		{
			if (predicate(value))
			{
				return value;
			}
		}
		return null;
	}

	[CanBeNull]
	public PlayerBehavior GetPlayerIncludeLocalPlayer(string id)
	{
		return (!(id == PlayerBehavior.LocalPlayer.EntityId)) ? GetPlayer(id) : PlayerBehavior.LocalPlayer;
	}

	[CanBeNull]
	public PlayerBehavior GetPlayerIncludeLocalPlayer([NotNull] Predicate<PlayerBehavior> predicate)
	{
		if (predicate(PlayerBehavior.LocalPlayer))
		{
			return PlayerBehavior.LocalPlayer;
		}
		foreach (PlayerBehavior value in _players.Values)
		{
			if (predicate(value))
			{
				return value;
			}
		}
		return null;
	}

	public IEnumerable<PlayerBehavior> GetPlayers()
	{
		return _players.Values;
	}

	[NotNull]
	public PlayerBehavior MakePlayerObject(bool male, Vector3? worldPosition, string id, string motionName = "Barehand_Stand", bool loadClips = true)
	{
		GameObject original = ((!male) ? Durango.Utils.Singleton<PlatformResources>.Instance().FemaleReference : Durango.Utils.Singleton<PlatformResources>.Instance().MaleReference);
		GameObject gameObject = UnityEngine.Object.Instantiate(original);
		PlayerBehavior component = gameObject.GetComponent<PlayerBehavior>();
		component.CurrentPosition = ((!worldPosition.HasValue) ? Vector3.zero : Util.WorldPositionToClientPosition(worldPosition.Value));
		component.EntityId = id;
		if (loadClips)
		{
			LoadPlayerClips(male, gameObject);
			component.PlayMotionForcely((!string.IsNullOrEmpty(motionName)) ? motionName : "Barehand_Stand", 1f, immediately: true);
		}
		return component;
	}

	public PlayerBehavior MakePreview(bool male, PlayerDisplay? display = null, float yaw = 180f, bool loadClips = true)
	{
		bool male2 = male;
		Vector3? worldPosition = null;
		string empty = string.Empty;
		bool loadClips2 = loadClips;
		PlayerBehavior player = MakePlayerObject(male2, worldPosition, empty, "Barehand_Stand", loadClips2);
		player.MotionConditionChanged += delegate
		{
			player.PlayStateForcely((!player.IsAlive) ? "Die" : "Stand", 1f, immediately: true);
		};
		player.IsPreview = true;
		player.LookAtController.enabled = false;
		if (display.HasValue)
		{
			SetDisplay(player, display.Value);
		}
		player.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
		return player;
	}

	private static void LoadPlayerClips(bool male, GameObject target)
	{
		Animation componentInChildren = target.GetComponentInChildren<Animation>();
		foreach (KeyValuePair<string, AnimationClip> playerClip in GetPlayerClips(male))
		{
			componentInChildren.AddClip(playerClip.Value, playerClip.Key);
		}
	}

	public static IEnumerable<KeyValuePair<string, AnimationClip>> GetPlayerClips(bool male)
	{
		AnimationClipResource resource = Durango.Utils.Singleton<AssetBundleManager>.Instance().GetPlayerClip(male);
		if (resource == null || resource.Clips == null)
		{
			Debug.LogError("Cannot load player AnimationClipResource");
			yield break;
		}
		foreach (KeyValuePair<string, AnimationClip> playerClip in GetPlayerClips(resource.Clips))
		{
			yield return playerClip;
		}
	}

	public static IEnumerable<KeyValuePair<string, AnimationClip>> GetPlayerClips(List<AnimationClip> clips)
	{
		int i = 0;
		for (int count = clips.Count; i < count; i++)
		{
			AnimationClip clip = clips[i];
			if (!(clip == null))
			{
				string clipName = ((!clip.name.EndsWith("@hq", StringComparison.InvariantCultureIgnoreCase)) ? clip.name : clip.name.Substring(0, clip.name.Length - 3));
				yield return new KeyValuePair<string, AnimationClip>(clipName, clip);
			}
		}
	}

	public bool HandleMoveMsg(Move msg)
	{
		bool flag = msg.EntityId == PlayerBehavior.LocalPlayer.EntityId;
		PlayerBehavior player = GetPlayer(msg.EntityId);
		if (player != null)
		{
			player.HandleMoveMsg(msg);
		}
		return flag || player != null;
	}

	public bool HandleDisappearMsg(DisappearEntity msg)
	{
		PlayerBehavior player = GetPlayer(msg.EntityId);
		if (player != null)
		{
			OnDisappearPlayer(player);
			UnityEngine.Object.Destroy(player.gameObject);
			_players.Remove(msg.EntityId);
			return true;
		}
		return false;
	}

	private static void SetCostumeColors(PlayerBehavior player, PlayerDisplay msg)
	{
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Body, new ItemColor(msg.BodyColor));
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Head, new ItemColor(msg.HeadColor));
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Skin, new ItemColor(msg.SkinColor));
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Hair, new ItemColor(msg.HairColor));
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Eye, new ItemColor(msg.EyeColor));
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Lip, new ItemColor(msg.LipColor));
		player.ChangeEquipmentColor(new ItemColor(msg.EquipColor));
	}

	public static void SetDisplay(PlayerBehavior player, PlayerDisplay msg, bool hideOtherPlayer = false, bool fromAppear = false, bool handleBoarding = false)
	{
		player.Display = msg;
		player.ChangeCostume(CharacterCostume.CostumeType.Body, msg.Body);
		player.ChangeCostume(CharacterCostume.CostumeType.Head, msg.Head);
		player.ChangeCostume(CharacterCostume.CostumeType.Beard, msg.Beard);
		player.ChangeCostume(CharacterCostume.CostumeType.Hair, msg.Hair);
		player.ChangeEquipment(msg.Equip);
		player.ChangeAccessory("Attachment_Flag", ((!string.IsNullOrEmpty(msg.Accessory)) ? SingletonDict<string, Accessory>.Get(msg.Accessory) : null)?.Model);
		SetCostumeColors(player, msg);
		player.ChangeBodySize(msg.BodySize);
		player.VoiceSoundSwitch = GetVoiceSoundSwitch(player.IsMale, msg.VoiceType);
		player.ChangePortraitType(msg.Portrait, msg.PortraitBg, msg.PortraitBgColor.ToColor());
		bool flag = (!player.IsLocalPlayer && hideOtherPlayer) || msg.Invisible;
		if (player.IsLocalPlayer && msg.Invisible)
		{
			flag = false;
		}
		handleBoarding = true;
		if (handleBoarding)
		{
			player.SetBoardingOn(msg.BoardingOn, msg.VehicleEntityId, fromAppear);
		}
		player.SetVisible(!flag);
		player.SetWeaponData(msg.WeaponInfo);
	}

	public static SoundSwitch GetVoiceSoundSwitch(bool isMale, int voiceType)
	{
		voiceType = Math.Max(voiceType, 1);
		return SoundSwitch.Set("PlayerVoice", string.Format("{0}{1:00}", (!isMale) ? "Female" : "Male", voiceType));
	}

	public void HideOtherPlayers(bool hide)
	{
		_hideOtherPlayer = hide;
		foreach (PlayerBehavior value in _players.Values)
		{
			value.SetVisible(!hide);
		}
	}

	[ExposedInEditor(null)]
	public void MakePlayers(int count, int radius)
	{
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = Util.ClientPositionToWorldPosition(PlayerBehavior.LocalPlayer.CurrentPosition);
			Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
			Vector3 worldPos = new Vector3(vector.x + insideUnitCircle.x * (float)radius, 0f, vector.z + insideUnitCircle.y * (float)radius);
			string text = (UnityEngine.Random.value * 100000f).ToString();
			AppearPlayer appearPlayer = CreateAppearPlayer(text, i % 2 == 0, text, worldPos);
			Connections.Frontend.Handle(90u, appearPlayer, default(PacketHeader));
		}
	}

	private AppearPlayer CreateAppearPlayer(string entityId, bool male, string playerName, Vector3 worldPos)
	{
		AppearPlayer result = default(AppearPlayer);
		result.EntityId = entityId;
		result.EntityType = (ushort)((!male) ? 1001 : 1000);
		result.IsAlive = true;
		result.Name = playerName;
		result.Title.EntityId = entityId;
		result.Title._Title = "Generated";
		result.Member.EntityId = entityId;
		result.Member.ClanId = string.Empty;
		result.Member.ClanName = string.Empty;
		result.Member.RoleId = -1;
		Shared.Player.Job[] array = Enums<Shared.Player.Job>.Greater(Shared.Player.Job.Invalid);
		Shared.Player.Job job = array[UnityEngine.Random.Range(0, array.Length)];
		bool isMale = UnityEngine.Random.value > 0.5f;
		EditPlayerDisplayProxy.FillRandomPlayerDisplayData(isMale, job, ref result.Display);
		result.Display.Body = ResourceSingleton<PlayerCostumeTable>.Instance().GetPlayerDefaultBodyModelAssetBundlePath(isMale, (int)job, PlayerCostumeTable.ClothState.Normal);
		result.Display.EntityId = entityId;
		result.Move.EntityId = entityId;
		result.Move.Movements = new Movement[1];
		result.Move.Movements[0].Path = new Location[1];
		result.Move.Movements[0].Path[0].Position = new WorldPosition(worldPos.x, worldPos.z);
		result.Survival.EntityId = entityId;
		result.Survival.Life = new Gauge(100f, 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = 100f
			}
		});
		result.Survival.Gauges = new Dictionary<string, Gauge>();
		return result;
	}

	private IEnumerator Start()
	{
		Durango.Utils.Singleton<GameManager>.Instance().PreReconnect += GameManager_PreReconnect;
		Connections.Frontend.On(delegate(AppearPlayer msg, PacketHeader header)
		{
			bool flag = msg.EntityId == PlayerBehavior.LocalPlayer.EntityId;
			double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
			Location location = PathMovable.GetLocation(msg.Move, bufferedServerTime);
			Vector3 value = location.Position.ToVector3();
			byte floor = location.Floor;
			float yaw = location.Yaw;
			if (flag)
			{
				Durango.Utils.Singleton<TerrainBase>.Instance().SetCorrectionPostion(location.Position.ToVector2());
			}
			PlayerBehavior playerBehavior = null;
			if (flag)
			{
				bool male = msg.IsMale();
				string lastMotionName = PathMovable.GetLastMotionName(msg.Move);
				playerBehavior = MakePlayerObject(male, value, msg.EntityId, lastMotionName);
				playerBehavior.gameObject.name = "Player";
				playerBehavior.RescueRequested = msg.RescueRequested;
				UnityEngine.Object.Destroy(PlayerBehavior.LocalPlayer.gameObject);
				PlayerBehavior.LocalPlayer = playerBehavior;
				SetPlayer(playerBehavior, yaw, floor, msg);
				Durango.Utils.Singleton<PlayerController>.Instance().UpdateLastSentTransform(location.Position.ToClientPosition(), location.Height, yaw, location.Floor);
				PlayerController.MotionUpdater.Motion(lastMotionName, 0f, 1f, forceTransition: true);
				PlayerController.MotionUpdater.ForceUpdate();
				Vector3 pos2 = location.Position.ToClientPosition();
				pos2.y = location.Height;
				Durango.Utils.Singleton<PlayerController>.Instance().Teleport(pos2, TeleportType.Unknown, instance: true);
				OnAppearPlayer(playerBehavior);
			}
			else
			{
				playerBehavior = GetPlayer(msg.EntityId);
				if (playerBehavior == null)
				{
					bool male2 = msg.IsMale();
					string appearMotionName = PathMovable.GetAppearMotionName(msg.Move, bufferedServerTime);
					playerBehavior = MakePlayerObject(male2, value, msg.EntityId, appearMotionName);
					playerBehavior.PathMovable.HandleMoveMsg(msg.Move);
					playerBehavior.RescueRequested = msg.RescueRequested;
					SetPlayer(playerBehavior, yaw, floor, msg);
					_players[msg.EntityId] = playerBehavior;
					OnAppearPlayer(playerBehavior);
				}
			}
			Connections.Frontend.Handle(182u, msg.Survival, header);
		});
		Connections.Frontend.On(delegate(Teleported msg, PacketHeader _)
		{
			Vector3 pos = Util.TilePositionToClientPosition(msg.Tile, tileCenter: true);
			Durango.Utils.Singleton<PlayerController>.Instance().Teleport(pos, msg.Type);
			if (this.Teleported != null)
			{
				this.Teleported(msg.Type);
			}
		});
		Connections.Frontend.On(delegate(Member msg, PacketHeader header)
		{
			PlayerBehavior playerIncludeLocalPlayer5 = GetPlayerIncludeLocalPlayer(msg.EntityId);
			if (playerIncludeLocalPlayer5 != null)
			{
				SetClan(playerIncludeLocalPlayer5, msg);
			}
		});
		Connections.Frontend.On(delegate(Messages.Title msg, PacketHeader header)
		{
			string entityId = msg.EntityId;
			PlayerBehavior playerIncludeLocalPlayer4 = GetPlayerIncludeLocalPlayer(entityId);
			if (playerIncludeLocalPlayer4 != null)
			{
				SetTitle(playerIncludeLocalPlayer4, msg);
			}
		});
		Connections.Frontend.On(delegate(PlayerDisplay msg, PacketHeader header)
		{
			PlayerBehavior playerIncludeLocalPlayer3 = GetPlayerIncludeLocalPlayer(msg.EntityId);
			if (playerIncludeLocalPlayer3 != null)
			{
				SetDisplay(playerIncludeLocalPlayer3, msg, _hideOtherPlayer, fromAppear: false, handleBoarding: true);
				if (this.DisplayUpdated != null)
				{
					this.DisplayUpdated(playerIncludeLocalPlayer3);
				}
			}
		});
		Connections.Frontend.On(delegate(VisualEffects msg, PacketHeader header)
		{
			PlayerBehavior playerIncludeLocalPlayer2 = GetPlayerIncludeLocalPlayer(msg.EntityId);
			if (playerIncludeLocalPlayer2 != null)
			{
				playerIncludeLocalPlayer2.SetParticleEffects(msg.Effects);
				playerIncludeLocalPlayer2.SkinEffect = msg.SkinEffect;
			}
		});
		Connections.Frontend.On(delegate(PlayerDrawLine msg, PacketHeader header)
		{
			if (ShowDrawLine)
			{
				PlayerBehavior player2 = GetPlayer(msg.PlayerId);
				if (!(player2 == null) && player2.GetVisible() && !GameSystem<SocialSystem>.Instance().IsBlocked(player2.EntityId) && !CombatSystem.IsHostilePlayer(player2))
				{
					player2.AddDrawLineBuffer(msg.DrawCommands);
				}
			}
		});
		Connections.Frontend.On(delegate(PlayerVoice msg, PacketHeader header)
		{
			PlayerBehavior player = GetPlayer(msg.PlayerId);
			if (player != null && player.GetVisible())
			{
				player.OnVoiceMsg(Convert.FromBase64String(msg.VoiceData));
			}
		});
		Connections.Frontend.On(delegate(SetBaseMoveSpeed msg, PacketHeader header)
		{
			if (!(msg.EntityId != GameManager.PlayerId))
			{
				Durango.Utils.Singleton<PlayerController>.Instance().SetBaseMoveSpeed(msg);
			}
		});
		Connections.Frontend.On(delegate(Messages.Musician msg, PacketHeader header)
		{
			PlayerBehavior playerIncludeLocalPlayer = GetPlayerIncludeLocalPlayer(msg.EntityId);
			if (playerIncludeLocalPlayer != null)
			{
				playerIncludeLocalPlayer.SetMusician(msg);
			}
		});
		do
		{
			yield return null;
		}
		while (!Connections.Frontend.Connected());
		Durango.Utils.Singleton<GameManager>.Instance().SendReady();
	}

	private void GameManager_PreReconnect()
	{
		foreach (PlayerBehavior value in _players.Values)
		{
			UnityEngine.Object.Destroy(value.gameObject);
		}
		_players.Clear();
	}

	private void SetPlayer(PlayerBehavior player, float yaw, byte floor, AppearPlayer msg)
	{
		player.SetAlive(msg.IsAlive, fromInit: true);
		player.Floor.Value = floor;
		player.TurnToYaw(yaw, bSnap: true);
		player.PlayerName = msg.Name;
		player.Freq = msg.Freq;
		player.Level = msg.Level;
		player.EntityTypeId = msg.EntityType;
		SetDisplay(player, msg.Display, _hideOtherPlayer, fromAppear: true, handleBoarding: true);
		SetTitle(player, msg.Title);
		SetClan(player, msg.Member);
		player.SetMusician(msg.Musician);
	}

	private void SetTitle(PlayerBehavior player, Messages.Title msg)
	{
		player.Title = msg;
		if (this.PlayerTitleChanged != null)
		{
			this.PlayerTitleChanged(player);
		}
	}

	private void SetClan(PlayerBehavior player, Member msg)
	{
		if (!GameManager.Region.IsPvpIsland())
		{
			player.Clan = msg;
			if (this.PlayerClanChanged != null)
			{
				this.PlayerClanChanged(player);
			}
		}
	}

	private void OnAppearPlayer(PlayerBehavior player)
	{
		if (this.PlayerAppeared != null)
		{
			this.PlayerAppeared(player);
		}
	}

	private void OnDisappearPlayer(PlayerBehavior player)
	{
		player.StopMusic();
		if (this.PlayerDisappeared != null)
		{
			this.PlayerDisappeared(player);
		}
		if (player.Driver.IsRiding)
		{
			player.Driver.Unmount(null, immediately: true);
		}
	}
}
