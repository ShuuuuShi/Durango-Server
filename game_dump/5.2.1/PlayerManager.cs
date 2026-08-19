using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
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
	[CompilerGenerated]
	private sealed class _003CGetPlayerClips_003Ed__32 : IEnumerable<KeyValuePair<string, AnimationClip>>, IEnumerable, IEnumerator<KeyValuePair<string, AnimationClip>>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private KeyValuePair<string, AnimationClip> _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private bool male;

		public bool _003C_003E3__male;

		private IEnumerator<KeyValuePair<string, AnimationClip>> _003C_003E7__wrap1;

		KeyValuePair<string, AnimationClip> IEnumerator<KeyValuePair<string, AnimationClip>>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetPlayerClips_003Ed__32(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					AnimationClipResource playerClip = Durango.Utils.Singleton<AssetBundleManager>.Instance().GetPlayerClip(male);
					if (playerClip == null || playerClip.Clips == null)
					{
						Debug.LogError("Cannot load player AnimationClipResource");
						return false;
					}
					_003C_003E7__wrap1 = GetPlayerClips(playerClip.Clips).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				}
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					KeyValuePair<string, AnimationClip> current = _003C_003E7__wrap1.Current;
					_003C_003E2__current = current;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<KeyValuePair<string, AnimationClip>> IEnumerable<KeyValuePair<string, AnimationClip>>.GetEnumerator()
		{
			_003CGetPlayerClips_003Ed__32 _003CGetPlayerClips_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetPlayerClips_003Ed__ = this;
			}
			else
			{
				_003CGetPlayerClips_003Ed__ = new _003CGetPlayerClips_003Ed__32(0);
			}
			_003CGetPlayerClips_003Ed__.male = _003C_003E3__male;
			return _003CGetPlayerClips_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, AnimationClip>>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetPlayerClips_003Ed__33 : IEnumerable<KeyValuePair<string, AnimationClip>>, IEnumerable, IEnumerator<KeyValuePair<string, AnimationClip>>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private KeyValuePair<string, AnimationClip> _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private List<AnimationClip> clips;

		public List<AnimationClip> _003C_003E3__clips;

		private int _003Ci_003E5__2;

		private int _003Ccount_003E5__3;

		KeyValuePair<string, AnimationClip> IEnumerator<KeyValuePair<string, AnimationClip>>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetPlayerClips_003Ed__33(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_00a1;
			}
			_003C_003E1__state = -1;
			_003Ci_003E5__2 = 0;
			_003Ccount_003E5__3 = clips.Count;
			goto IL_00b1;
			IL_00a1:
			_003Ci_003E5__2++;
			goto IL_00b1;
			IL_00b1:
			if (_003Ci_003E5__2 < _003Ccount_003E5__3)
			{
				AnimationClip animationClip = clips[_003Ci_003E5__2];
				if (!(animationClip == null))
				{
					string key = ((!animationClip.name.EndsWith("@hq", StringComparison.InvariantCultureIgnoreCase)) ? animationClip.name : animationClip.name.Substring(0, animationClip.name.Length - 3));
					_003C_003E2__current = new KeyValuePair<string, AnimationClip>(key, animationClip);
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_00a1;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<KeyValuePair<string, AnimationClip>> IEnumerable<KeyValuePair<string, AnimationClip>>.GetEnumerator()
		{
			_003CGetPlayerClips_003Ed__33 _003CGetPlayerClips_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetPlayerClips_003Ed__ = this;
			}
			else
			{
				_003CGetPlayerClips_003Ed__ = new _003CGetPlayerClips_003Ed__33(0);
			}
			_003CGetPlayerClips_003Ed__.clips = _003C_003E3__clips;
			return _003CGetPlayerClips_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, AnimationClip>>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CStart_003Ed__42 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PlayerManager _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CStart_003Ed__42(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PlayerManager CS_0024_003C_003E8__locals0 = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				if (Connections.Frontend.Connected())
				{
					Durango.Utils.Singleton<GameManager>.Instance().SendReady();
					return false;
				}
			}
			else
			{
				_003C_003E1__state = -1;
				Durango.Utils.Singleton<GameManager>.Instance().PreReconnect += CS_0024_003C_003E8__locals0.GameManager_PreReconnect;
				Connections.Frontend.On(delegate(AppearPlayer msg, PacketHeader header)
				{
					bool num2 = msg.EntityId == PlayerBehavior.LocalPlayer.EntityId;
					double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
					Location location = PathMovable.GetLocation(msg.Move, bufferedServerTime);
					Vector3 value = location.Position.ToVector3();
					byte floor = location.Floor;
					float yaw = location.Yaw;
					if (num2)
					{
						Durango.Utils.Singleton<TerrainBase>.Instance().SetCorrectionPostion(location.Position.ToVector2());
					}
					PlayerBehavior playerBehavior = null;
					if (num2)
					{
						bool male = msg.IsMale();
						string lastMotionName = PathMovable.GetLastMotionName(msg.Move);
						playerBehavior = CS_0024_003C_003E8__locals0.MakePlayerObject(male, value, msg.EntityId, lastMotionName);
						playerBehavior.gameObject.name = "Player";
						playerBehavior.RescueRequested = msg.RescueRequested;
						UnityEngine.Object.Destroy(PlayerBehavior.LocalPlayer.gameObject);
						PlayerBehavior.LocalPlayer = playerBehavior;
						CS_0024_003C_003E8__locals0.SetPlayer(playerBehavior, yaw, floor, msg);
						Durango.Utils.Singleton<PlayerController>.Instance().UpdateLastSentTransform(location.Position.ToClientPosition(), location.Height, yaw, location.Floor);
						PlayerController.MotionUpdater.Motion(lastMotionName, 0f, 1f, forceTransition: true);
						PlayerController.MotionUpdater.ForceUpdate();
						Vector3 pos2 = location.Position.ToClientPosition();
						pos2.y = location.Height;
						Durango.Utils.Singleton<PlayerController>.Instance().Teleport(pos2, TeleportType.Unknown, instance: true);
						CS_0024_003C_003E8__locals0.OnAppearPlayer(playerBehavior);
					}
					else
					{
						playerBehavior = CS_0024_003C_003E8__locals0.GetPlayer(msg.EntityId);
						if (playerBehavior == null)
						{
							bool male2 = msg.IsMale();
							string appearMotionName = PathMovable.GetAppearMotionName(msg.Move, bufferedServerTime);
							playerBehavior = CS_0024_003C_003E8__locals0.MakePlayerObject(male2, value, msg.EntityId, appearMotionName);
							playerBehavior.PathMovable.HandleMoveMsg(msg.Move);
							playerBehavior.RescueRequested = msg.RescueRequested;
							CS_0024_003C_003E8__locals0.SetPlayer(playerBehavior, yaw, floor, msg);
							CS_0024_003C_003E8__locals0._players[msg.EntityId] = playerBehavior;
							CS_0024_003C_003E8__locals0.OnAppearPlayer(playerBehavior);
						}
					}
					Connections.Frontend.Handle(182u, msg.Survival, header);
				});
				Connections.Frontend.On(delegate(Teleported msg, PacketHeader _)
				{
					Vector3 pos = Util.TilePositionToClientPosition(msg.Tile, tileCenter: true);
					Durango.Utils.Singleton<PlayerController>.Instance().Teleport(pos, msg.Type);
					if (CS_0024_003C_003E8__locals0.Teleported != null)
					{
						CS_0024_003C_003E8__locals0.Teleported(msg.Type);
					}
				});
				Connections.Frontend.On(delegate(Member msg, PacketHeader header)
				{
					PlayerBehavior playerIncludeLocalPlayer5 = CS_0024_003C_003E8__locals0.GetPlayerIncludeLocalPlayer(msg.EntityId);
					if (playerIncludeLocalPlayer5 != null)
					{
						CS_0024_003C_003E8__locals0.SetClan(playerIncludeLocalPlayer5, msg);
					}
				});
				Connections.Frontend.On(delegate(Messages.Title msg, PacketHeader header)
				{
					string entityId = msg.EntityId;
					PlayerBehavior playerIncludeLocalPlayer4 = CS_0024_003C_003E8__locals0.GetPlayerIncludeLocalPlayer(entityId);
					if (playerIncludeLocalPlayer4 != null)
					{
						CS_0024_003C_003E8__locals0.SetTitle(playerIncludeLocalPlayer4, msg);
					}
				});
				Connections.Frontend.On(delegate(PlayerDisplay msg, PacketHeader header)
				{
					PlayerBehavior playerIncludeLocalPlayer3 = CS_0024_003C_003E8__locals0.GetPlayerIncludeLocalPlayer(msg.EntityId);
					if (playerIncludeLocalPlayer3 != null)
					{
						SetDisplay(playerIncludeLocalPlayer3, msg, CS_0024_003C_003E8__locals0._hideOtherPlayer, fromAppear: false, handleBoarding: true);
						if (CS_0024_003C_003E8__locals0.DisplayUpdated != null)
						{
							CS_0024_003C_003E8__locals0.DisplayUpdated(playerIncludeLocalPlayer3);
						}
					}
				});
				Connections.Frontend.On(delegate(VisualEffects msg, PacketHeader header)
				{
					PlayerBehavior playerIncludeLocalPlayer2 = CS_0024_003C_003E8__locals0.GetPlayerIncludeLocalPlayer(msg.EntityId);
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
						PlayerBehavior player2 = CS_0024_003C_003E8__locals0.GetPlayer(msg.PlayerId);
						if (!(player2 == null) && player2.GetVisible() && !GameSystem<SocialSystem>.Instance().IsBlocked(player2.EntityId) && !CombatSystem.IsHostilePlayer(player2))
						{
							player2.AddDrawLineBuffer(msg.DrawCommands);
						}
					}
				});
				Connections.Frontend.On(delegate(PlayerVoice msg, PacketHeader header)
				{
					PlayerBehavior player = CS_0024_003C_003E8__locals0.GetPlayer(msg.PlayerId);
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
					PlayerBehavior playerIncludeLocalPlayer = CS_0024_003C_003E8__locals0.GetPlayerIncludeLocalPlayer(msg.EntityId);
					if (playerIncludeLocalPlayer != null)
					{
						playerIncludeLocalPlayer.SetMusician(msg);
					}
				});
			}
			_003C_003E2__current = null;
			_003C_003E1__state = 1;
			return true;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

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
		if (id == PlayerBehavior.LocalPlayer.EntityId)
		{
			return PlayerBehavior.LocalPlayer;
		}
		return GetPlayer(id);
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
		GameObject gameObject = UnityEngine.Object.Instantiate((!male) ? Durango.Utils.Singleton<PlatformResources>.Instance().FemaleReference : Durango.Utils.Singleton<PlatformResources>.Instance().MaleReference);
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetPlayerClips_003Ed__32(-2)
		{
			_003C_003E3__male = male
		};
	}

	public static IEnumerable<KeyValuePair<string, AnimationClip>> GetPlayerClips(List<AnimationClip> clips)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetPlayerClips_003Ed__33(-2)
		{
			_003C_003E3__clips = clips
		};
	}

	public bool HandleMoveMsg(Move msg)
	{
		bool num = msg.EntityId == PlayerBehavior.LocalPlayer.EntityId;
		PlayerBehavior player = GetPlayer(msg.EntityId);
		if (player != null)
		{
			player.HandleMoveMsg(msg);
		}
		if (!num)
		{
			return player != null;
		}
		return true;
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
		result.EntityType = (ushort)((!male) ? 1001u : 1000u);
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStart_003Ed__42(0)
		{
			_003C_003E4__this = this
		};
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
