using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using K1Network;
using Messages;
using MsgPack;
using MusicData;
using Player;
using Sanford.Multimedia.Midi;
using Shared.Teleport;
using UnityEngine;

public class PlayerManager : KSingleton<PlayerManager>
{
	[SerializeField]
	private GameObject _malePrefab;

	[SerializeField]
	private GameObject _femalePrefab;

	private readonly List<PlayerBehavior> _players = new List<PlayerBehavior>();

	private float _playerDisappearCheckTime;

	private bool _hideOtherPlayer;

	public List<PlayerBehavior> Players => _players;

	public event Action<PlayerBehavior> PlayerAppeared;

	public event Action<PlayerBehavior> PlayerDisappeared;

	public event Action<PlayerBehavior> PlayerTitleChanged;

	public event Action<PlayerBehavior> PlayerClanChanged;

	public event Action<TeleportType> Teleported;

	public Animation GetPlayerAnimation(bool isMale)
	{
		GameObject val = ((!isMale) ? _femalePrefab : _malePrefab);
		if ((Object)(object)val == (Object)null || val.transform.childCount == 0)
		{
			return null;
		}
		Transform child = val.transform.GetChild(0);
		return (!((Object)(object)child != (Object)null)) ? null : ((Component)child).GetComponent<Animation>();
	}

	private int PlayerIndexOf(ulong id)
	{
		int i = 0;
		for (int count = _players.Count; i < count; i++)
		{
			if (_players[i].EntityId == id)
			{
				return i;
			}
		}
		return -1;
	}

	[CanBeNull]
	public PlayerBehavior GetPlayer(ulong id)
	{
		int num = PlayerIndexOf(id);
		return (num != -1) ? _players[num] : null;
	}

	[CanBeNull]
	public PlayerBehavior GetPlayerIncludeLocalPlayer(ulong id)
	{
		return (id != PlayerBehavior.LocalPlayer.EntityId) ? GetPlayer(id) : PlayerBehavior.LocalPlayer;
	}

	[NotNull]
	public PlayerBehavior MakePlayerObject(bool male, Vector3 worldPosition, ulong id, bool isPreview = false)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>((!male) ? _femalePrefab : _malePrefab);
		PlayerBehavior component = val.GetComponent<PlayerBehavior>();
		component.ExpireAt = -1f;
		component.CurrentPosition = ((!isPreview) ? TerrainA6.WorldPositionToClientPosition(worldPosition) : Vector3.zero);
		component.EntityId = id;
		component.PlayAnimation("Stand");
		component.LateMotionUpdate();
		if (isPreview)
		{
			component.IsPreview = true;
			((Behaviour)component.LookAtController).enabled = false;
			Object.Destroy((Object)(object)((Component)component).GetComponent<AnimationEventController>());
		}
		return component;
	}

	public bool HandleMoveMsg(Move msg)
	{
		bool flag = msg.EntityId == PlayerBehavior.LocalPlayer.EntityId;
		if (flag)
		{
			PlayerBehavior.LocalPlayer.HandleMoveMsg(msg);
		}
		PlayerBehavior player = GetPlayer(msg.EntityId);
		if ((Object)(object)player != (Object)null)
		{
			player.HandleMoveMsg(msg);
		}
		return flag || (Object)(object)player != (Object)null;
	}

	public bool HandleDisappearMsg(DisappearEntity msg)
	{
		PlayerBehavior player = GetPlayer(msg.EntityId);
		if ((Object)(object)player != (Object)null)
		{
			OnDisappearPlayer(player);
			Object.Destroy((Object)(object)((Component)player).gameObject);
			int num = PlayerIndexOf(msg.EntityId);
			if (num != -1)
			{
				_players.RemoveAt(num);
			}
			return true;
		}
		return false;
	}

	private static ItemColor ToThreeColors(string[] strings)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (strings == null || strings.Length != 3)
		{
			return default(ItemColor);
		}
		Color val = KUtility.ToColor(strings[0]);
		Color val2 = KUtility.ToColor(strings[1]);
		Color val3 = KUtility.ToColor(strings[2]);
		return new ItemColor(val, val2, val3);
	}

	private static ItemColor ToItemColor(string colorString)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return (!string.IsNullOrEmpty(colorString)) ? new ItemColor(KUtility.ToColor(colorString)) : default(ItemColor);
	}

	private static void SetCostumeColors(PlayerBehavior player, PlayerDisplay msg)
	{
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Body, ToThreeColors(msg.BodyColor));
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Head, ToThreeColors(msg.HeadColor));
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Skin, ToItemColor(msg.SkinColor));
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Hair, ToItemColor(msg.HairColor));
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Eye, ToItemColor(msg.EyeColor));
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Lip, ToItemColor(msg.LipColor));
		player.ChangeCostumeColor(CharacterCostume.CostumeType.Equipment, ToThreeColors(msg.EquipColor));
	}

	private static void SetCostumeColorsFromDict(PlayerBehavior player, ref PlayerDisplay display, MessagePackObjectDictionary dict)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		string[] array = new string[3];
		MessagePackObject val = default(MessagePackObject);
		for (int i = 0; i < 3; i++)
		{
			string text = "body_color_" + i;
			if (dict.TryGetValue(MessagePackObject.op_Implicit(text), ref val))
			{
				array[i] = ((MessagePackObject)(ref val)).AsString();
			}
			else
			{
				array[i] = "000000";
			}
		}
		display.BodyColor = array;
		array = new string[3];
		for (int j = 0; j < 3; j++)
		{
			string text2 = "head_color_" + j;
			if (dict.TryGetValue(MessagePackObject.op_Implicit(text2), ref val))
			{
				array[j] = ((MessagePackObject)(ref val)).AsString();
			}
			else
			{
				array[j] = "000000";
			}
		}
		display.HeadColor = array;
		display.SkinColor = ((!dict.TryGetValue(MessagePackObject.op_Implicit("skin_color"), ref val)) ? "000000" : ((MessagePackObject)(ref val)).AsString());
		display.HairColor = ((!dict.TryGetValue(MessagePackObject.op_Implicit("hair_color"), ref val)) ? "000000" : ((MessagePackObject)(ref val)).AsString());
		display.EyeColor = ((!dict.TryGetValue(MessagePackObject.op_Implicit("eye_color"), ref val)) ? "000000" : ((MessagePackObject)(ref val)).AsString());
		string lipColor;
		if (dict.TryGetValue(MessagePackObject.op_Implicit("lip_color"), ref val))
		{
			lipColor = ((MessagePackObject)(ref val)).AsString();
		}
		else
		{
			Color randomLipColor = ColorTableLoader.GetRandomLipColor(player.IsMale);
			lipColor = ((Color)(ref randomLipColor)).ToString();
		}
		display.LipColor = lipColor;
	}

	public static void SetCostumeFromDict(PlayerBehavior player, MessagePackObjectDictionary dict)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		PlayerDisplay display = default(PlayerDisplay);
		MessagePackObject val = default(MessagePackObject);
		display.DefaultBody = ((!dict.TryGetValue(MessagePackObject.op_Implicit("default_body"), ref val)) ? string.Empty : ((MessagePackObject)(ref val)).AsString());
		display.DefaultInner = ((!dict.TryGetValue(MessagePackObject.op_Implicit("default_inner"), ref val)) ? string.Empty : ((MessagePackObject)(ref val)).AsString());
		display.Body = ((!dict.TryGetValue(MessagePackObject.op_Implicit("body"), ref val)) ? string.Empty : ((MessagePackObject)(ref val)).AsString());
		display.Head = ((!dict.TryGetValue(MessagePackObject.op_Implicit("head"), ref val)) ? string.Empty : ((MessagePackObject)(ref val)).AsString());
		display.Beard = ((!dict.TryGetValue(MessagePackObject.op_Implicit("beard"), ref val)) ? string.Empty : ((MessagePackObject)(ref val)).AsString());
		display.Hair = ((!dict.TryGetValue(MessagePackObject.op_Implicit("hair"), ref val)) ? string.Empty : ((MessagePackObject)(ref val)).AsString());
		display.Equip = ((!dict.TryGetValue(MessagePackObject.op_Implicit("equip"), ref val)) ? string.Empty : ((MessagePackObject)(ref val)).AsString());
		SetCostumeColorsFromDict(player, ref display, dict);
		display.BodySize = 0f;
		MessagePackObject val2 = default(MessagePackObject);
		if (dict.TryGetValue(MessagePackObject.op_Implicit("body_size"), ref val2))
		{
			display.BodySize = (((object)((MessagePackObject)(ref val2)).UnderlyingType != typeof(string)) ? ((MessagePackObject)(ref val2)).AsSingle() : float.Parse(((MessagePackObject)(ref val2)).AsString()));
		}
		display.VoiceType = 0;
		if (dict.TryGetValue(MessagePackObject.op_Implicit("voice_type"), ref val2))
		{
			display.VoiceType = (((object)((MessagePackObject)(ref val2)).UnderlyingType != typeof(string)) ? ((MessagePackObject)(ref val2)).AsInt32() : int.Parse(((MessagePackObject)(ref val2)).AsString()));
		}
		display.Portrait = 0;
		if (dict.TryGetValue(MessagePackObject.op_Implicit("portrait"), ref val2))
		{
			display.Portrait = (((object)((MessagePackObject)(ref val2)).UnderlyingType != typeof(string)) ? ((MessagePackObject)(ref val2)).AsInt32() : int.Parse(((MessagePackObject)(ref val2)).AsString()));
		}
		display.PortraitBg = -1;
		if (dict.TryGetValue(MessagePackObject.op_Implicit("portrait_bg"), ref val2))
		{
			display.PortraitBg = (((object)((MessagePackObject)(ref val2)).UnderlyingType != typeof(string)) ? ((MessagePackObject)(ref val2)).AsInt32() : int.Parse(((MessagePackObject)(ref val2)).AsString()));
		}
		string portraitBgColor;
		if (dict.TryGetValue(MessagePackObject.op_Implicit("portrait_bg_color"), ref val2))
		{
			portraitBgColor = ((MessagePackObject)(ref val2)).ToString();
		}
		else
		{
			Color clear = Color.clear;
			portraitBgColor = ((Color)(ref clear)).ToString();
		}
		display.PortraitBgColor = portraitBgColor;
		if (dict.TryGetValue(MessagePackObject.op_Implicit("effects"), ref val2))
		{
			IList<MessagePackObject> list = ((MessagePackObject)(ref val2)).AsList();
			KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[list.Count];
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				MessagePackObject val3 = list[i];
				IList<MessagePackObject> list2 = ((MessagePackObject)(ref val3)).AsList();
				ref KeyValuePair<string, string> reference = ref array[i];
				MessagePackObject val4 = list2[0];
				string key = ((MessagePackObject)(ref val4)).AsString();
				MessagePackObject val5 = list2[1];
				reference = new KeyValuePair<string, string>(key, ((MessagePackObject)(ref val5)).AsString());
			}
			display.Effects = array;
		}
		else
		{
			display.Effects = new KeyValuePair<string, string>[0];
		}
		display.Invisible = false;
		if (dict.TryGetValue(MessagePackObject.op_Implicit("invisible"), ref val2))
		{
			display.Invisible = ((MessagePackObject)(ref val2)).AsBoolean();
		}
		SetCostume(player, display);
	}

	public static void SetCostume(PlayerBehavior player, PlayerDisplay msg)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		player.Display = msg;
		player.ChangeCostume(msg.Body);
		if (string.IsNullOrEmpty(msg.Head))
		{
			player.SetCostumeVisible(CharacterCostume.CostumeType.Head, isVisible: false);
		}
		else
		{
			player.SetCostumeVisible(CharacterCostume.CostumeType.Head, isVisible: true);
			player.ChangeCostume(msg.Head);
		}
		if (string.IsNullOrEmpty(msg.Beard))
		{
			player.SetCostumeVisible(CharacterCostume.CostumeType.Beard, isVisible: false);
		}
		else
		{
			player.SetCostumeVisible(CharacterCostume.CostumeType.Beard, isVisible: true);
			player.ChangeCostume(msg.Beard);
		}
		player.ChangeCostume(msg.Hair);
		player.ChangeEquipment(msg.Equip);
		SetCostumeColors(player, msg);
		player.ChangeBodySize(msg.BodySize);
		int voiceType = Math.Max(msg.VoiceType, 1);
		player.Voice.Set(player.IsMale, voiceType);
		player.ChangePortraitType(msg.Portrait, msg.PortraitBg, KUtility.ToColor(msg.PortraitBgColor));
		player.SetEffects(msg.Effects);
		player.SetRendererEnabled(!msg.Invisible);
	}

	public void HideOtherPlayer(bool hide)
	{
		_hideOtherPlayer = hide;
		int i = 0;
		for (int count = _players.Count; i < count; i++)
		{
			_players[i].SetRendererEnabled(!hide);
		}
	}

	private IEnumerator Start()
	{
		Connections.Frontend.On(delegate(AppearPlayer msg, PacketHeader header)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			bool flag = msg.EntityId == PlayerBehavior.LocalPlayer.EntityId;
			Location location = PathMovable.GetLocation(msg.Move, Connections.Frontend.GetBufferedServerTime());
			if (flag)
			{
				KSingleton<TerrainA6>.Instance().SetCorrectionPostion(location.Position.ToVector2());
			}
			float expireAt = Time.time + 86400f;
			PlayerBehavior player11 = GetPlayer(msg.EntityId);
			if ((Object)(object)player11 != (Object)null)
			{
				player11.ExpireAt = expireAt;
			}
			else
			{
				PlayerBehavior playerBehavior = null;
				Vector3 worldPosition = location.Position.ToVector3();
				byte floor = location.Floor;
				if (flag)
				{
					bool male = msg.EntityType == 1000;
					playerBehavior = MakePlayerObject(male, worldPosition, msg.EntityId);
					playerBehavior.Floor = floor;
					((Object)((Component)playerBehavior).gameObject).name = "Player";
					Object.Destroy((Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject);
					PlayerBehavior.LocalPlayer = playerBehavior;
					playerBehavior.PlayerName = msg.Name;
					playerBehavior.EntityTypeId = msg.EntityType;
					SetClan(playerBehavior, msg.Member);
					SetTitle(playerBehavior, msg.Title);
					playerBehavior.Teleport(location.Position.ToClientPosition());
					playerBehavior.SetWeaponData(msg.Display.WeaponInfo);
					SetCostume(PlayerBehavior.LocalPlayer, msg.Display);
					GameSystem<StatisticsSystem>.Instance().MaybeCacheFreq();
					OnAppearPlayer(playerBehavior);
				}
				else
				{
					playerBehavior = MakePlayerObject(msg.EntityType == 1000, worldPosition, msg.EntityId);
					playerBehavior.Floor = floor;
					playerBehavior.PlayerName = msg.Name;
					playerBehavior.EntityTypeId = msg.EntityType;
					SetClan(playerBehavior, msg.Member);
					SetTitle(playerBehavior, msg.Title);
					playerBehavior.ExpireAt = expireAt;
					SetCostume(playerBehavior, msg.Display);
					playerBehavior.SetRendererEnabled(!_hideOtherPlayer);
					_players.Add(playerBehavior);
					OnAppearPlayer(playerBehavior);
					playerBehavior.SetWeaponData(msg.Display.WeaponInfo);
				}
				Messages.Rider? rider = msg.Rider;
				if (rider.HasValue)
				{
					AnimalManager animalManager = KSingleton<AnimalManager>.Instance();
					Messages.Rider? rider2 = msg.Rider;
					animalManager.MakeVehicle(rider2.Value, ((Component)playerBehavior).gameObject);
				}
				Connections.Frontend.Handle(182u, msg.Survival, header);
			}
		});
		Connections.Frontend.On(delegate(Teleported msg, PacketHeader _)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Vector3 pos = TerrainA6.TilePositionToClientPosition(msg.Tile, tileCenter: true);
			KSingleton<PlayerController>.Instance().Teleport(pos, msg.Type);
			if (this.Teleported != null)
			{
				this.Teleported(msg.Type);
			}
		});
		Connections.Frontend.On(delegate(Member msg, PacketHeader header)
		{
			PlayerBehavior playerIncludeLocalPlayer3 = GetPlayerIncludeLocalPlayer(msg.EntityId);
			if ((Object)(object)playerIncludeLocalPlayer3 != (Object)null)
			{
				SetClan(playerIncludeLocalPlayer3, msg);
			}
		});
		Connections.Frontend.On(delegate(Title msg, PacketHeader header)
		{
			ulong entityId = msg.EntityId;
			PlayerBehavior playerIncludeLocalPlayer2 = GetPlayerIncludeLocalPlayer(entityId);
			if ((Object)(object)playerIncludeLocalPlayer2 != (Object)null)
			{
				SetTitle(playerIncludeLocalPlayer2, msg);
			}
		});
		Connections.Frontend.On(delegate(PlayerDisplay msg, PacketHeader header)
		{
			PlayerBehavior playerIncludeLocalPlayer = GetPlayerIncludeLocalPlayer(msg.EntityId);
			if ((Object)(object)playerIncludeLocalPlayer != (Object)null)
			{
				SetCostume(playerIncludeLocalPlayer, msg);
				playerIncludeLocalPlayer.SetWeaponData(msg.WeaponInfo);
			}
		});
		Connections.Frontend.RegisterRelayHandler(delegate(PlayerBattle msg, float timePassed)
		{
			PlayerBehavior player10 = GetPlayer(msg.PlayerInfo.PlayerId);
			if ((Object)(object)player10 != (Object)null)
			{
				player10.SetCombatMode(msg.IsAimMode, timePassed);
			}
		});
		Connections.Frontend.RegisterRelayHandler(delegate(PlayerAimTarget msg, float timePassed)
		{
			PlayerBehavior player9 = GetPlayer(msg.PlayerInfo.PlayerId);
			if ((Object)(object)player9 != (Object)null)
			{
				GameObject target = KSingleton<ObjectManager>.Instance().FindObject(msg.Target);
				player9.Target = target;
			}
		});
		Connections.Frontend.RegisterRelayHandler(delegate(PlayerEmoticon msg, float timePassed)
		{
			KSingleton<EmoticonEffectControl>.Instance().Show(msg.PlayerInfo.PlayerId, msg.EmoticonType, msg.Power);
		});
		Connections.Frontend.RegisterRelayHandler(delegate(PlayerChangeEquip msg, float timePassed)
		{
			PlayerBehavior player8 = GetPlayer(msg.PlayerInfo.PlayerId);
			if ((Object)(object)player8 != (Object)null)
			{
				player8.ChangeEquipment(msg.Name);
			}
		});
		Connections.Frontend.RegisterRelayHandler(delegate(PlayerChangeCostume msg, float timePassed)
		{
			PlayerBehavior player7 = GetPlayer(msg.PlayerInfo.PlayerId);
			if ((Object)(object)player7 != (Object)null)
			{
				player7.ChangeCostume(msg.Name);
			}
		});
		Connections.Frontend.On(delegate(PlayerDrawLine msg, PacketHeader header)
		{
			PlayerBehavior player6 = GetPlayer(msg.PlayerId);
			if ((Object)(object)player6 != (Object)null && player6.GetRenderEnabled())
			{
				player6.AddDrawLineBuffer(msg.DrawCommands);
			}
		});
		Connections.Frontend.On(delegate(Messages.PlayerVoice msg, PacketHeader header)
		{
			PlayerBehavior player5 = GetPlayer(msg.PlayerId);
			if ((Object)(object)player5 != (Object)null && player5.GetRenderEnabled())
			{
				player5.OnVoiceMsg(Convert.FromBase64String(msg.VoiceData));
			}
		});
		Connections.Frontend.On(delegate(Dead msg, PacketHeader header)
		{
			SideEffectGroup effect = KSingleton<UIManager>.Instance().SideEffect;
			KUtility.DelayedCall((MonoBehaviour)(object)effect, delegate
			{
				effect.PlayDeathEffect(DeathActionDescriptor.GetDeathMsg(), UIManager.FindScript<InteractionGroup>().ShowPlayerDeadInteractionMenu);
			}, (float)(header.Time - Connections.Frontend.GetBufferedServerTime()));
		});
		Connections.Frontend.On<Revived>(delegate
		{
			PlayerBehavior.LocalPlayer.Respawn();
		});
		Connections.Frontend.RegisterDynamicRelayHandler("Particle", delegate(MessagePackObjectDictionary data)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			MessagePackObject val4 = default(MessagePackObject);
			if (data.TryGetValue(MessagePackObject.op_Implicit("entity_id"), ref val4))
			{
				ulong id4 = ((MessagePackObject)(ref val4)).AsUInt64();
				PlayerBehavior player4 = GetPlayer(id4);
				if (!((Object)(object)player4 == (Object)null) && player4.GetRenderEnabled() && !data.TryGetValue(MessagePackObject.op_Implicit("path"), ref val4))
				{
					string path = ((MessagePackObject)(ref val4)).AsString();
					float time = 0f;
					if (data.TryGetValue(MessagePackObject.op_Implicit("time"), ref val4))
					{
						time = ((MessagePackObject)(ref val4)).AsSingle();
					}
					player4.SetParticleEffect(path, time);
				}
			}
		});
		Connections.Frontend.RegisterDynamicRelayHandler("Sound", delegate(MessagePackObjectDictionary data)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			MessagePackObject val3 = default(MessagePackObject);
			if (data.TryGetValue(MessagePackObject.op_Implicit("entity_id"), ref val3))
			{
				ulong id3 = ((MessagePackObject)(ref val3)).AsUInt64();
				PlayerBehavior player3 = GetPlayer(id3);
				if (!((Object)(object)player3 == (Object)null) && player3.GetRenderEnabled() && data.TryGetValue(MessagePackObject.op_Implicit("path"), ref val3))
				{
					player3.PlaySound(((MessagePackObject)(ref val3)).AsString());
				}
			}
		});
		Connections.Frontend.RegisterDynamicRelayHandler("Music", delegate(MessagePackObjectDictionary data)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			MessagePackObject val2 = default(MessagePackObject);
			if (data.TryGetValue(MessagePackObject.op_Implicit("player"), ref val2))
			{
				ulong id2 = ((MessagePackObject)(ref val2)).AsUInt64();
				PlayerBehavior player2 = GetPlayer(id2);
				if (!((Object)(object)player2 == (Object)null) && player2.GetRenderEnabled() && data.TryGetValue(MessagePackObject.op_Implicit("IsPlay"), ref val2))
				{
					if (((MessagePackObject)(ref val2)).AsBoolean())
					{
						if (data.TryGetValue(MessagePackObject.op_Implicit("music"), ref val2))
						{
							byte[] buffer = ((MessagePackObject)(ref val2)).AsBinary();
							MemoryStream stream = new MemoryStream(buffer);
							Sequence sequence = new Sequence();
							sequence.Load(stream);
							Music music = Music.Create(sequence);
							if (data.TryGetValue(MessagePackObject.op_Implicit("instrument"), ref val2))
							{
								string instrument = ((MessagePackObject)(ref val2)).AsString();
								player2.PlayMusic(music, instrument);
							}
						}
					}
					else
					{
						player2.StopMusic();
					}
				}
			}
		});
		Connections.Frontend.RegisterDynamicRelayHandler("Dirty", delegate(MessagePackObjectDictionary data)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			MessagePackObject val = default(MessagePackObject);
			if (data.TryGetValue(MessagePackObject.op_Implicit("player"), ref val))
			{
				ulong id = ((MessagePackObject)(ref val)).AsUInt64();
				PlayerBehavior player = GetPlayer(id);
				if (!((Object)(object)player == (Object)null) && data.TryGetValue(MessagePackObject.op_Implicit("Dirty"), ref val))
				{
					player.SkinDirtyLevel = (CharacterCostume.SkinDirty)((MessagePackObject)(ref val)).AsInt32();
				}
			}
		});
		Connections.Frontend.Legacy_RegisterNotificationHandler(1300, delegate(Notify msg, PacketHeader header)
		{
			MainStatus mainStatus = KSingleton<PlayerController>.Instance().MainStatus;
			mainStatus.UpdateMainStatus(msg.Data);
		});
		do
		{
			yield return null;
		}
		while (!Connections.Frontend.Connected() || GameManager.IsPrologueMode);
		KSingleton<GameManager>.Instance().SendReady();
	}

	private void Update()
	{
		if (Time.time < _playerDisappearCheckTime)
		{
			return;
		}
		for (int num = _players.Count - 1; num >= 0; num--)
		{
			PlayerBehavior playerBehavior = _players[num];
			if (!playerBehavior.IsLocalPlayer && playerBehavior.ExpireAt > 0f && Time.time >= playerBehavior.ExpireAt)
			{
				_players.RemoveAt(num);
				Object.Destroy((Object)(object)((Component)playerBehavior).gameObject);
			}
		}
		_playerDisappearCheckTime = Time.time + 1f;
	}

	private void SetTitle(PlayerBehavior player, Title msg)
	{
		player.Title = msg;
		if (this.PlayerTitleChanged != null)
		{
			this.PlayerTitleChanged(player);
		}
	}

	private void SetClan(PlayerBehavior player, Member msg)
	{
		player.Clan = msg;
		if (this.PlayerClanChanged != null)
		{
			this.PlayerClanChanged(player);
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
		if (this.PlayerDisappeared != null)
		{
			this.PlayerDisappeared(player);
		}
	}
}
