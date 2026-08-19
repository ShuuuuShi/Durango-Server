using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Faction;
using Durango.Logic.Map;
using Durango.Logic.Party;
using Durango.Logic.PlayGuide;
using Durango.Logic.Social;
using Durango.Logic.WarpRush;
using Durango.Player;
using Durango.Terrain;
using Durango.Utils;
using Messages;
using Shared.Building;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class MapIndicators : Durango.Utils.Singleton<MapIndicators>
{
	[SerializeField]
	private AreaEffectIndicator _areaEffectIndicator;

	[SerializeField]
	private BalloonContainer _balloonContainer;

	[SerializeField]
	private IndicatorLabel _indicatorLabel;

	[SerializeField]
	private FadeOutLabel _fadeOutTooltipLabel;

	[SerializeField]
	private Vector2 _indicatorLabelOffset;

	[SerializeField]
	private bool _isDirectionalLocalPlayerIndicator;

	private readonly List<KeyValuePair<Type, ListObjectPool>> _pools = new List<KeyValuePair<Type, ListObjectPool>>();

	private readonly List<MapIndicator> _indicators = new List<MapIndicator>();

	private readonly ListObjectPool<AreaEffectIndicator> _areaEffectIndicators = new ListObjectPool<AreaEffectIndicator>();

	private readonly ListObjectPool<IndicatorLabel> _indicatorLabels = new ListObjectPool<IndicatorLabel>();

	private bool[] _hideTypes;

	public List<MapIndicator> Indicators => _indicators;

	public event Action<MapIndicator> IndicatorClicked;

	protected override void OnAwake()
	{
		_hideTypes = new bool[(int)(Enums<IndicatorType>.Max() + 1)];
		_areaEffectIndicators.BaseObject = _areaEffectIndicator;
		_indicatorLabels.BaseObject = _indicatorLabel;
		_indicatorLabels.UseBase = true;
		_indicatorLabels.Init(null);
		_balloonContainer.TileToMapPosition = (Vector2 tilePos) => Durango.Utils.Singleton<MapContext>.Instance().TileToMapPosition(tilePos);
		_balloonContainer.TileToHumanePosition = (Vector2 tilePos) => MapPositionParser.PositionToHumaneTile(Durango.Terrain.Util.TilePositionToWorldPosition(tilePos));
		MapIndicator[] componentsInChildren = GetComponentsInChildren<MapIndicator>();
		foreach (MapIndicator mapIndicator in componentsInChildren)
		{
			Type type = mapIndicator.GetType();
			ListObjectPool pool = GetPool(type);
			if (pool == null)
			{
				pool = new ListObjectPool();
				pool.BaseObject = mapIndicator.gameObject;
				pool.Init(OnInitIndicator);
				_pools.Add(new KeyValuePair<Type, ListObjectPool>(type, pool));
			}
		}
		GameSystem<SocialSystem>.Instance().ChatAdded += SocialSystem_ChatAdded;
		Durango.Utils.Singleton<MapContext>.Instance().ScaleChanged += MapContext_ScaleChanged;
		Durango.Utils.Singleton<MapContext>.Instance().Attached += MapContext_Attached;
		GameSystem<MapSystem>.Instance().PointsUpdated += MapSystem_PointsUpdated;
		Durango.Logic.Social.Conversation.MessagesUpdated += Conversation_MessageUpdated;
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated += delegate
		{
			BroadcastRefresh(MapIndicator.Refresh.ClanInfoUpdated);
		};
		Durango.Utils.Singleton<PlayerManager>.Instance().PlayerClanChanged += delegate
		{
			BroadcastRefresh(MapIndicator.Refresh.PlayerClanChanged);
		};
		GameSystem<StatisticsSystem>.Instance().LevelChanged += delegate
		{
			BroadcastRefresh(MapIndicator.Refresh.LevelChanged);
		};
		GameSystem<ToDoListSystem>.Instance().Added += ToDoListAdded;
		GameSystem<ToDoListSystem>.Instance().Removed += ToDoListRemoved;
		Durango.Utils.Singleton<AnimalManager>.Instance().AnimalAppeared += OnAppearAnimal;
		Durango.Utils.Singleton<AnimalManager>.Instance().AnimalDisappeared += OnDisappearAnimal;
		Durango.Utils.Singleton<PlayerManager>.Instance().PlayerAppeared += OnAppearPlayer;
		Durango.Utils.Singleton<PlayerManager>.Instance().PlayerDisappeared += OnDisappearPlayer;
		GameSystem<GatheringSystem>.Instance().CollectiblePermissionChanged += GatheringSystem_CollectiblePermissionChanged;
		GameSystem<WarpAcceleratorSystem>.Instance().WarpAcceleratorsUpdated += OnUpdateWarpAccelerators;
		GameSystem<PartySystem>.Instance().MembersUpdated += PartySystem_MembersUpdated;
		GameSystem<WarpRushSystem>.Instance().MembersUpdated += WarpRushSystem_MembersUpdated;
		Durango.Utils.Singleton<ArtifactManager>.Instance().Removed += ArtifactManager_Removed;
		Artifact.ArtifactStateChanged += OnChangeArtifactState;
		Artifact.ArtifactDisplayChanged += OnChangeArtifactDisplay;
		Durango.Utils.Singleton<GameManager>.Instance().PreReconnect += Clear;
	}

	protected override void OnDestroyed()
	{
		Durango.Logic.Social.Conversation.MessagesUpdated -= Conversation_MessageUpdated;
		Artifact.ArtifactStateChanged -= OnChangeArtifactState;
		Artifact.ArtifactDisplayChanged -= OnChangeArtifactDisplay;
	}

	private void OnInitIndicator(GameObject obj)
	{
		UIEventListener uIEventListener = UIEventListener.Get(obj);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickIndicator));
	}

	private void Clear()
	{
		_indicators.Clear();
		for (int i = 0; i < _pools.Count; i++)
		{
			_pools[i].Value.Clear();
		}
		_areaEffectIndicators.Clear();
	}

	private void LateUpdate()
	{
		if (!(PlayerBehavior.LocalPlayer == null))
		{
			UpdateIndicators();
			UpdateAreaEffectIndicator();
			UpdateIndicatorLabels();
			_balloonContainer.UpdatePosition();
		}
	}

	public static T GetOrAdd<T>(string id, IndicatorType type) where T : MapIndicator
	{
		if (Durango.Utils.Singleton<MapIndicators>.HasInstance())
		{
			return Durango.Utils.Singleton<MapIndicators>.Instance().GetOrAddIndicator<T>(id, type);
		}
		return null;
	}

	public static void Remove(string id, IndicatorType type)
	{
		if (Durango.Utils.Singleton<MapIndicators>.HasInstance())
		{
			Durango.Utils.Singleton<MapIndicators>.Instance().RemoveIndicator(id, type);
		}
	}

	public static void Remove(IndicatorType type)
	{
		if (Durango.Utils.Singleton<MapIndicators>.HasInstance())
		{
			Durango.Utils.Singleton<MapIndicators>.Instance().RemoveIndicator(type);
		}
	}

	public bool HasOneOrMoreWarpHoles()
	{
		return _indicators.Any((MapIndicator indicator) => indicator.Type == IndicatorType.Warphole);
	}

	public void Hide(IndicatorType type, bool hide)
	{
		if (type < IndicatorType.Death || (int)type >= _hideTypes.Length || _hideTypes[(int)type] == hide)
		{
			return;
		}
		_hideTypes[(int)type] = hide;
		for (int i = 0; i < _indicators.Count; i++)
		{
			MapIndicator mapIndicator = _indicators[i];
			if (mapIndicator.Type == type)
			{
				mapIndicator.ToggleHideFlag(MapIndicator.HideFlag.Type, hide);
			}
		}
	}

	public void HideTypesClear()
	{
		for (int i = 0; i < _hideTypes.Length; i++)
		{
			_hideTypes[i] = false;
		}
		for (int j = 0; j < _indicators.Count; j++)
		{
			_indicators[j].ToggleHideFlag(MapIndicator.HideFlag.Type, hide: false);
		}
	}

	public bool ContainsIndicator(string id, IndicatorType type)
	{
		return IndexOf(id, type) != -1;
	}

	public MapIndicator GetIndicator(string id, IndicatorType type)
	{
		int num = IndexOf(id, type);
		if (num == -1)
		{
			return null;
		}
		return _indicators[num];
	}

	private T GetOrAddIndicator<T>(string id, IndicatorType type) where T : MapIndicator
	{
		MapIndicator indicator = GetIndicator(id, type);
		if (indicator != null)
		{
			T val = indicator as T;
			if (!(val == null))
			{
				return val;
			}
			RemoveIndicator(id, type);
		}
		return AddIndicator<T>(id, type);
	}

	private T AddIndicator<T>(string id, IndicatorType type) where T : MapIndicator
	{
		ListObjectPool pool = GetPool(typeof(T));
		if (pool == null)
		{
			return null;
		}
		T val = pool.Add<T>();
		val.Set(id, type);
		_indicators.Add(val);
		val.OnInitialized();
		if (type >= IndicatorType.Death && (int)type < _hideTypes.Length && _hideTypes[(int)type])
		{
			val.ToggleHideFlag(MapIndicator.HideFlag.Type, hide: true);
		}
		return val;
	}

	private int IndexOf(string id, IndicatorType type)
	{
		for (int i = 0; i < _indicators.Count; i++)
		{
			if (_indicators[i].Id == id && _indicators[i].Type == type)
			{
				return i;
			}
		}
		return -1;
	}

	private void RemoveIndicator(string id, IndicatorType type)
	{
		RemoveIndicator(IndexOf(id, type));
	}

	private void RemoveIndicator(IndicatorType type)
	{
		for (int i = 0; i < _indicators.Count; i++)
		{
			if (_indicators[i].Type == type)
			{
				RemoveIndicator(i);
				i--;
			}
		}
	}

	private void RemoveIndicator(int index)
	{
		if (index >= 0 && index < _indicators.Count)
		{
			MapIndicator mapIndicator = _indicators[index];
			_indicators.RemoveAt(index);
			ListObjectPool pool = GetPool(mapIndicator.GetType());
			int index2 = pool.IndexOf(mapIndicator.gameObject);
			pool.Remove(index2);
		}
	}

	private ListObjectPool GetPool(Type type)
	{
		for (int i = 0; i < _pools.Count; i++)
		{
			if (_pools[i].Key == type)
			{
				return _pools[i].Value;
			}
		}
		return null;
	}

	public void AddAreaEffectIndicator(MapIndicator ind, Color color, float radius, float validRadius = 0f, bool fixedScale = false)
	{
		AreaEffectIndicator orAddIndicator = GetOrAddIndicator(ind);
		orAddIndicator.Show();
		orAddIndicator.Set(ind, radius, validRadius, fixedScale);
		orAddIndicator.SetColor(color);
	}

	public void RemoveAreaEffectIndicator(MapIndicator ind)
	{
		int num = IndexOf(ind);
		if (num != -1)
		{
			_areaEffectIndicators[num].Hide();
			_areaEffectIndicators.Remove(num);
		}
	}

	private AreaEffectIndicator GetOrAddIndicator(MapIndicator ind)
	{
		int i = 0;
		for (int count = _areaEffectIndicators.Count; i < count; i++)
		{
			if (_areaEffectIndicators[i].Indicator == ind)
			{
				return _areaEffectIndicators[i];
			}
		}
		return _areaEffectIndicators.Add();
	}

	private int IndexOf(MapIndicator ind)
	{
		int i = 0;
		for (int count = _areaEffectIndicators.Count; i < count; i++)
		{
			if (_areaEffectIndicators[i].Indicator == ind)
			{
				return i;
			}
		}
		return -1;
	}

	public void AddIndicatorLabel(MapIndicator ind, SpriteData spriteData, string text)
	{
		_indicatorLabels.Add().Set(ind, spriteData, text);
	}

	public void ClearIndicatorLabels()
	{
		_indicatorLabels.Clear();
	}

	private void UpdateIndicatorLabels()
	{
		for (int i = 0; i < _indicatorLabels.Count; i++)
		{
			_indicatorLabels[i].UpdatePosition(_indicatorLabelOffset);
		}
	}

	public void AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, Durango.Player.PlayerInfo info)
	{
		_balloonContainer.AddAnnounceBalloon(type, tilePos, info);
	}

	public void AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, string entityId, string spriteName, string titleName, int spriteSize)
	{
		_balloonContainer.AddAnnounceBalloon(type, tilePos, entityId, spriteName, titleName, spriteSize);
	}

	public void RemoveAnnounceBalloon(AnnounceType type, string entityId)
	{
		_balloonContainer.RemoveAnnounceBalloons(type, entityId);
	}

	public void HideToolTipLabel()
	{
		_fadeOutTooltipLabel.transform.parent = base.transform;
		_fadeOutTooltipLabel.gameObject.SetActive(value: false);
	}

	private void UpdateAreaEffectIndicator()
	{
		Vector2 center = Durango.Terrain.Util.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition);
		MapContext mapContext = Durango.Utils.Singleton<MapContext>.Instance();
		for (int num = _areaEffectIndicators.Count - 1; num >= 0; num--)
		{
			AreaEffectIndicator areaEffectIndicator = _areaEffectIndicators[num];
			if (areaEffectIndicator.Check(center))
			{
				areaEffectIndicator.transform.localPosition = mapContext.TileToMapPosition(areaEffectIndicator.Indicator.GetTile());
				if (areaEffectIndicator.FixedScale)
				{
					areaEffectIndicator.transform.localScale = Vector3.one;
				}
				else
				{
					areaEffectIndicator.transform.localScale = Vector3.one * mapContext.CurrentZoomScale * mapContext.MapNGUISize / mapContext.MapSize;
				}
			}
			else
			{
				areaEffectIndicator.Hide();
				_areaEffectIndicators.Remove(num);
			}
		}
	}

	private void OnClickIndicator(GameObject obj)
	{
		MapIndicator mapIndicator = null;
		Vector3 vector = NGUIMath.ScreenToParentPixels(UICamera.currentTouch.pos, base.transform);
		float num = 6400f;
		for (int i = 0; i < _indicators.Count; i++)
		{
			MapIndicator mapIndicator2 = _indicators[i];
			Vector3 position = mapIndicator2.Widget.GetPosition(0.5f, 0.5f);
			float sqrMagnitude = (vector - position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				mapIndicator = mapIndicator2;
				num = sqrMagnitude;
			}
		}
		if (mapIndicator != null && this.IndicatorClicked != null)
		{
			this.IndicatorClicked(mapIndicator);
			if (!string.IsNullOrEmpty(mapIndicator.Tooltip))
			{
				_fadeOutTooltipLabel.Show(mapIndicator, mapIndicator.Tooltip);
			}
		}
	}

	private void MapContext_ScaleChanged()
	{
		MapContext mapContext = Durango.Utils.Singleton<MapContext>.Instance();
		float relativeZoomScale = mapContext.RelativeZoomScale;
		bool isWorldMapMode = mapContext.IsWorldMapMode;
		int i = 0;
		for (int count = _indicators.Count; i < count; i++)
		{
			MapIndicator mapIndicator = _indicators[i];
			bool hide = isWorldMapMode && mapIndicator.VisibleZoom > 0f && relativeZoomScale <= mapIndicator.VisibleZoom;
			mapIndicator.ToggleHideFlag(MapIndicator.HideFlag.Zoom, hide);
		}
	}

	private void MapContext_Attached()
	{
		bool isWorldMapMode = Durango.Utils.Singleton<MapContext>.Instance().IsWorldMapMode;
		_balloonContainer.SetWorldmapMode(isWorldMapMode);
		BroadcastRefresh(MapIndicator.Refresh.MapModeChanged);
	}

	private void MapSystem_PointsUpdated()
	{
		Points points = GameSystem<MapSystem>.Instance().Points;
		if (points.HasReachableHomePoint())
		{
			EntityTile value = points.HomePoint.Value;
			MapIconIndicator orAddIndicator = GetOrAddIndicator<MapIconIndicator>("myhome", IndicatorType.MyHome);
			if (orAddIndicator != null)
			{
				orAddIndicator.SetTarget(value.Tile);
				orAddIndicator.SetIcon("icon_map_house", Color.white, 35, 15);
			}
		}
		else
		{
			Remove(IndicatorType.MyHome);
		}
		if (points.HasReachableDeathPoint())
		{
			RegionTile value2 = points.DeathPoint.Value;
			MapIconIndicator orAddIndicator2 = GetOrAddIndicator<MapIconIndicator>("death", IndicatorType.Death);
			if (orAddIndicator2 != null)
			{
				orAddIndicator2.SetTarget(value2.Tile);
				orAddIndicator2.SetIcon("icon_map_dead", PresetColor.UIRed, 35, 10);
			}
		}
		else
		{
			Remove(IndicatorType.Death);
		}
	}

	private void SocialSystem_ChatAdded(ChatStruct chat)
	{
		if (chat.Chatter == null || chat.Chatter.ChatLineAddible)
		{
			ParseAnnouncePosition(chat);
		}
	}

	private void Conversation_MessageUpdated(Durango.Logic.Social.Conversation conv)
	{
		if (conv.Messages.Count != 0)
		{
			ChatStruct chat = conv.Messages[conv.Messages.Count - 1];
			ParseAnnouncePosition(chat);
		}
	}

	private void BroadcastRefresh(MapIndicator.Refresh type)
	{
		int i = 0;
		for (int count = _indicators.Count; i < count; i++)
		{
			_indicators[i].OnRefresh(type);
		}
	}

	private void ParseAnnouncePosition(ChatStruct chat)
	{
		if (chat.MsgType != 0 || !MapPositionParser.TryGetPosition(chat.FindText(), out var humaneX, out var humaneY))
		{
			return;
		}
		Durango.Utils.Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(chat.EntityId, delegate(Durango.Player.PlayerInfo info)
		{
			if (info.Valid)
			{
				Vector3 position = MapPositionParser.HumaneTileToPosition(new Vector2(humaneX, humaneY));
				_balloonContainer.AddAnnounceBalloon(AnnounceType.MyPosition, Durango.Terrain.Util.WorldPositionToTilePosition(position), info);
			}
		});
	}

	private void UpdateIndicators()
	{
		if (PlayerBehavior.LocalPlayer == null || !Durango.Utils.Singleton<MapContext>.HasInstance())
		{
			return;
		}
		MapContext mapContext = Durango.Utils.Singleton<MapContext>.Instance();
		bool flag = !mapContext.IsWorldMapMode;
		Vector2 center = -mapContext.Offset;
		Vector4 boundary = new Vector4((float)UIManager.ScreenWidth * 0.5f - 50f, (float)UIManager.ScreenHeight * 0.5f - 50f);
		boundary.z = boundary.x;
		boundary.w = boundary.y;
		Vector2 vector = Durango.Terrain.Util.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition);
		Vector2 center2 = mapContext.TileToMapPosition(vector);
		for (int num = _indicators.Count - 1; num >= 0; num--)
		{
			MapIndicator mapIndicator = _indicators[num];
			mapIndicator.OnUpdate();
			if (!mapIndicator.IsValid())
			{
				RemoveIndicator(num);
			}
			else
			{
				Vector2 tile = mapIndicator.GetTile();
				if (!mapIndicator.StickToBoundary)
				{
					float sqrMagnitude = (tile - vector).sqrMagnitude;
					mapIndicator.ToggleHideFlag(MapIndicator.HideFlag.Minimap, flag && sqrMagnitude >= 2304f);
					if (mapIndicator.CheckReveal)
					{
						mapIndicator.ToggleHideFlag(MapIndicator.HideFlag.Reveal, sqrMagnitude >= 81f);
					}
				}
				if (!mapIndicator.IsHidden)
				{
					Transform obj = mapIndicator.transform;
					Vector2 vector2 = mapContext.TileToMapPosition(tile);
					if (mapIndicator.StickToBoundary)
					{
						if (flag)
						{
							Vector4 boundary2 = new Vector4(75f, 75f, 75f, 55f);
							vector2 = StickToBoundary(vector2, center2, boundary2);
						}
						else
						{
							vector2 = StickToBoundary(vector2, center, boundary);
						}
					}
					obj.localPosition = vector2;
				}
			}
		}
	}

	private static Vector2 StickToBoundary(Vector2 position, Vector2 center, Vector4 boundary)
	{
		Vector2 vector = position - center;
		float num = ((!(vector.x < 0f)) ? boundary.x : boundary.z);
		if (Mathf.Abs(vector.x) >= num)
		{
			position.x = center.x + num * Mathf.Sign(vector.x);
		}
		num = ((!(vector.y < 0f)) ? boundary.y : boundary.w);
		if (Mathf.Abs(vector.y) >= num)
		{
			position.y = center.y + num * Mathf.Sign(vector.y);
		}
		return position;
	}

	private void ToDoListAdded(Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately)
	{
		if (TryGetFactionPointTodo(collection, out var toDo))
		{
			Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(toDo.FactionType);
			AddAnnounceBalloon(AnnounceType.GeneralPoint, toDo.TargetTile, collection.Key, IconMap.Get(toDo.FactionType), (faction != null) ? faction.Name.ToString() : toDo.FactionType.ToString(), 0);
		}
	}

	private void ToDoListRemoved(Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately)
	{
		if (TryGetFactionPointTodo(collection, out var toDo))
		{
			RemoveAnnounceBalloon(AnnounceType.GeneralPoint, toDo.Key);
		}
	}

	private void OnAppearAnimal(AnimalBehavior animal)
	{
		GetOrAddIndicator<MapAnimalIndicator>(animal.EntityId, IndicatorType.Animal).SetAnimal(animal);
	}

	private void OnDisappearAnimal(AnimalBehavior animal)
	{
		Remove(animal.EntityId, IndicatorType.Animal);
	}

	private void OnAppearPlayer(PlayerBehavior player)
	{
		IndicatorType indicatorType = ((!(player == PlayerBehavior.LocalPlayer)) ? IndicatorType.Player : IndicatorType.LocalPlayer);
		MapPlayerIndicator orAddIndicator = GetOrAddIndicator<MapPlayerIndicator>(player.EntityId, indicatorType);
		if (indicatorType == IndicatorType.LocalPlayer && _isDirectionalLocalPlayerIndicator)
		{
			orAddIndicator.IsDirectional = true;
		}
		orAddIndicator.SetPlayer(player);
	}

	private void OnDisappearPlayer(PlayerBehavior player)
	{
		IndicatorType type = ((!(player == PlayerBehavior.LocalPlayer)) ? IndicatorType.Player : IndicatorType.LocalPlayer);
		MapPlayerIndicator mapPlayerIndicator = GetIndicator(player.EntityId, type) as MapPlayerIndicator;
		if (mapPlayerIndicator != null)
		{
			mapPlayerIndicator.SetPlayer(null);
		}
	}

	private void GatheringSystem_CollectiblePermissionChanged(string id, bool hasPermission)
	{
		AnimalBehavior animal = Durango.Utils.Singleton<AnimalManager>.Instance().GetAnimal(id);
		if ((bool)animal && hasPermission)
		{
			MapIconIndicator orAddIndicator = GetOrAddIndicator<MapIconIndicator>(id, IndicatorType.Animal);
			orAddIndicator.SetTarget(animal.gameObject);
			orAddIndicator.SetIcon("icon_map_poi_animal_dead", new Color32(163, 163, 163, 225), 30, 20);
		}
	}

	private void OnUpdateWarpAccelerators()
	{
		using Reusable<HashSet<string>> reusable = ReusableHashSet<string>.Pop();
		HashSet<string> value = reusable.Value;
		int i = 0;
		for (int size = KUtility.GetSize(_indicators); i < size; i++)
		{
			if (_indicators[i].Type == IndicatorType.WarpAccelerator)
			{
				value.Add(_indicators[i].Id);
			}
		}
		foreach (WarpAcceleratorInfo warpAccelerator in GameSystem<WarpAcceleratorSystem>.Instance().WarpAccelerators)
		{
			if (warpAccelerator.Warpaccelerator.CurrentPhase > 0)
			{
				GetOrAddIndicator<MapWarpAcceleratorIndicator>(warpAccelerator.EntityId, IndicatorType.WarpAccelerator).SetInfo(warpAccelerator);
				value.Remove(warpAccelerator.EntityId);
			}
		}
		foreach (string item in value)
		{
			Remove(item, IndicatorType.WarpAccelerator);
		}
	}

	private void PartySystem_MembersUpdated()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_indicators); i < size; i++)
		{
			if (_indicators[i].Type == IndicatorType.Player)
			{
				(_indicators[i] as MapPlayerIndicator).SetPartyMember(null, -1);
			}
		}
		PartySystem partySystem = GameSystem<PartySystem>.Instance();
		int j = 0;
		for (int memberCount = partySystem.MemberCount; j < memberCount; j++)
		{
			Durango.Logic.Party.Member member = partySystem.GetMember(j);
			if (!(member.EntityId == PlayerBehavior.LocalPlayer.EntityId) && member.IsAccepted)
			{
				GetOrAddIndicator<MapPlayerIndicator>(member.EntityId, IndicatorType.Player).SetPartyMember(member, j + 1);
			}
		}
	}

	private void WarpRushSystem_MembersUpdated()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_indicators); i < size; i++)
		{
			if (_indicators[i].Type == IndicatorType.Player)
			{
				(_indicators[i] as MapPlayerIndicator).SetWarpRushMember(null);
			}
		}
		foreach (Durango.Logic.WarpRush.Member member in GameSystem<WarpRushSystem>.Instance().Members)
		{
			if (!(member.EntityId == PlayerBehavior.LocalPlayer.EntityId))
			{
				GetOrAddIndicator<MapPlayerIndicator>(member.EntityId, IndicatorType.Player).SetWarpRushMember(member);
			}
		}
	}

	private void ArtifactManager_Removed(Artifact artifact)
	{
		if (!TerrainMeta.HasGlobalIndicator(artifact.EntityType, artifact.WorldTile))
		{
			Remove(artifact.EntityId, IndicatorType.Artifact);
		}
	}

	private void OnChangeArtifactState(Artifact artifact)
	{
		UpdateArtifactIndicator(artifact);
	}

	private void OnChangeArtifactDisplay(Artifact artifact)
	{
		UpdateArtifactIndicator(artifact);
	}

	private void UpdateArtifactIndicator(Artifact artifact)
	{
		if (!TerrainMeta.HasGlobalIndicator(artifact.EntityType, artifact.WorldTile))
		{
			if (artifact.Blueprint.Indicator != null && artifact.Blueprint.Indicator.Active && artifact.BuildState >= BuildingState.Completed && artifact.Condition != Shared.Building.Condition.Broken)
			{
				GetOrAddIndicator<MapArtifactIndicator>(artifact.EntityId, IndicatorType.Artifact).SetArtifact(artifact.gameObject, artifact.Blueprint.Indicator);
			}
			else
			{
				Remove(artifact.EntityId, IndicatorType.Artifact);
			}
		}
	}

	private bool TryGetFactionPointTodo(Durango.Logic.PlayGuide.ToDoCollection collection, out Durango.Logic.Faction.MissionToDo toDo)
	{
		toDo = null;
		if (collection.IsDisabled)
		{
			return false;
		}
		if (collection.ToDoList != null && collection.ToDoList.Count > 0)
		{
			for (int i = 0; i < collection.ToDoList.Count; i++)
			{
				if (collection.ToDoList[i] is Durango.Logic.Faction.MissionToDo missionToDo && missionToDo.TargetTile != Vector2.zero)
				{
					toDo = missionToDo;
					if (!missionToDo.IsCompleted)
					{
						return true;
					}
				}
			}
		}
		return toDo != null;
	}
}
