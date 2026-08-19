using System;
using System.Collections.Generic;
using ChatData;
using MapData;
using Player;
using UnityEngine;

public class MapIndicators : KSingleton<MapIndicators>
{
	[SerializeField]
	private AreaEffectIndicator _areaEffectIndicator;

	[SerializeField]
	private BalloonContainer _balloonContainer;

	[SerializeField]
	private ListObjectPool _indicatorLabels;

	[SerializeField]
	private FadeOutLabel _fadeOutTooltipLabel;

	[SerializeField]
	private Vector2 _indicatorLabelOffset;

	private List<KeyValuePair<Type, ListObjectPool>> _pools = new List<KeyValuePair<Type, ListObjectPool>>();

	private List<MapIndicator> _indicators = new List<MapIndicator>();

	private GameObjectPool<AreaEffectIndicator> _areaEffectIndicatorPool;

	private bool _isInit;

	public static List<MapIndicator> Indicators
	{
		get
		{
			if (!KSingleton<MapIndicators>.HasInstance())
			{
				return null;
			}
			return KSingleton<MapIndicators>.Instance()._indicators;
		}
	}

	public event Action<Vector2> IndicatorDraged;

	public event Action<MapIndicator> IndicatorClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_areaEffectIndicatorPool = new GameObjectPool<AreaEffectIndicator>(_areaEffectIndicator);
		_indicatorLabels.Init(null);
		_balloonContainer.TileToMapPosition = (Vector2 tilePos) => KSingleton<MapContext>.Instance().TileToMapPosition(tilePos);
		_balloonContainer.TileToHumanePosition = (Vector2 tilePos) => MapPositionParser.PositionToHumaneTile(TerrainA6.TilePositionToWorldPosition(tilePos));
		GameSystem<SocialSystem>.Instance().ChatAdded += SocialSystem_ChatAdded;
		Conversation.MessagesUpdated += Conversation_MessageUpdated;
		KSingleton<GameManager>.Instance().PreReconnect += Clear;
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(((Component)this).transform);
		while (stack.Count > 0)
		{
			Transform val = stack.Pop();
			MapIndicator component = ((Component)val).GetComponent<MapIndicator>();
			if ((Object)(object)component != (Object)null)
			{
				Type type = ((object)component).GetType();
				ListObjectPool pool = GetPool(type);
				if (pool == null)
				{
					pool = new ListObjectPool();
					pool.BaseObject = ((Component)component).gameObject;
					pool.Init(OnInitIndicator);
					_pools.Add(new KeyValuePair<Type, ListObjectPool>(type, pool));
				}
			}
			int i = 0;
			for (int childCount = val.childCount; i < childCount; i++)
			{
				stack.Push(val.GetChild(i));
			}
		}
	}

	private void OnInitIndicator(GameObject obj)
	{
		UIEventListener uIEventListener = UIEventListener.Get(obj);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickIndicator));
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragIndicator));
	}

	private void Clear()
	{
		_indicators.Clear();
		foreach (KeyValuePair<Type, ListObjectPool> pool in _pools)
		{
			pool.Value.Clear();
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		Init();
	}

	private void OnDisable()
	{
		Conversation.MessagesUpdated -= Conversation_MessageUpdated;
	}

	private void LateUpdate()
	{
		UpdateIndicators();
		UpdateAreaEffectIndicator();
		UpdateIndicatorLabels();
		_balloonContainer.UpdatePosition();
	}

	public static ulong TileToId(Point2 tile)
	{
		return (ulong)tile.GetHashCode();
	}

	public static T Add<T>(Point2 tile, IndicatorType type) where T : MapIndicator
	{
		return Add<T>(TileToId(tile), type);
	}

	public static T Add<T>(ulong id, IndicatorType type) where T : MapIndicator
	{
		if (!KSingleton<MapIndicators>.HasInstance())
		{
			return (T)null;
		}
		return KSingleton<MapIndicators>.Instance().AddIndicator<T>(id, type);
	}

	public static T Get<T>(Point2 tile, IndicatorType type) where T : MapIndicator
	{
		return Get<T>(TileToId(tile), type);
	}

	public static T Get<T>(ulong id, IndicatorType type) where T : MapIndicator
	{
		MapIndicator indicator = KSingleton<MapIndicators>.Instance().GetIndicator(id, type);
		T result = (T)null;
		if ((Object)(object)indicator != (Object)null)
		{
			result = indicator as T;
		}
		return result;
	}

	public static void Remove(Point2 tile, IndicatorType type)
	{
		Remove(TileToId(tile), type);
	}

	public static void Remove(ulong id, IndicatorType type)
	{
		if (KSingleton<MapIndicators>.HasInstance())
		{
			KSingleton<MapIndicators>.Instance().RemoveIndicator(id, type);
		}
	}

	public static void Remove(IndicatorType type)
	{
		if (KSingleton<MapIndicators>.HasInstance())
		{
			KSingleton<MapIndicators>.Instance().RemoveIndicator(type);
		}
	}

	private T AddIndicator<T>(ulong id, IndicatorType type) where T : MapIndicator
	{
		Init();
		MapIndicator indicator = GetIndicator(id, type);
		if ((Object)(object)indicator != (Object)null)
		{
			T val = indicator as T;
			if (!((Object)(object)val == (Object)null))
			{
				return val;
			}
			RemoveIndicator(id, type);
		}
		ListObjectPool pool = GetPool(typeof(T));
		if (pool == null)
		{
			return (T)null;
		}
		T val2 = ((ListObjectPoolBase<GameObject>)pool).Add<T>();
		val2.Set(id, type);
		_indicators.Add(val2);
		return val2;
	}

	private int IndexOf(ulong id, IndicatorType type)
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

	private MapIndicator GetIndicator(ulong id, IndicatorType type)
	{
		int num = IndexOf(id, type);
		return (num != -1) ? _indicators[num] : null;
	}

	private void RemoveIndicator(ulong id, IndicatorType type)
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
			ListObjectPool pool = GetPool(((object)mapIndicator).GetType());
			int index2 = pool.IndexOf(((Component)mapIndicator).gameObject);
			pool.Remove(index2);
		}
	}

	private ListObjectPool GetPool(Type type)
	{
		for (int i = 0; i < _pools.Count; i++)
		{
			if ((object)_pools[i].Key == type)
			{
				return _pools[i].Value;
			}
		}
		return null;
	}

	public void AddAreaEffectIndicator(MapIndicator ind, Color color, float radius, float validRadius = 0f, bool fixedScale = false)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		AreaEffectIndicator areaEffectIndicator = GetAreaEffectIndicator(ind);
		areaEffectIndicator.Show();
		areaEffectIndicator.Set(ind, radius, validRadius, fixedScale);
		areaEffectIndicator.SetColor(color);
	}

	public void RemoveAreaEffectIndicator(MapIndicator ind)
	{
		AreaEffectIndicator areaEffectIndicator = GetAreaEffectIndicator(ind, make: false);
		RemoveAreaEffectIndicator(areaEffectIndicator);
	}

	private void RemoveAreaEffectIndicator(AreaEffectIndicator indicator)
	{
		if ((Object)(object)indicator != (Object)null)
		{
			indicator.Hide();
			_areaEffectIndicatorPool.Push(indicator);
		}
	}

	private AreaEffectIndicator GetAreaEffectIndicator(MapIndicator ind, bool make = true)
	{
		int i = 0;
		for (int count = _areaEffectIndicatorPool.Count; i < count; i++)
		{
			if ((Object)(object)_areaEffectIndicatorPool[i].Indicator == (Object)(object)ind)
			{
				return _areaEffectIndicatorPool[i];
			}
		}
		return (!make) ? null : _areaEffectIndicatorPool.Pop();
	}

	public void AddIndicatorLabel(MapIndicator ind, SpriteData spriteData, string text)
	{
		IndicatorLabel indicatorLabel = ((ListObjectPoolBase<GameObject>)_indicatorLabels).Add<IndicatorLabel>();
		indicatorLabel.Set(ind, spriteData, text);
	}

	public void ClearIndicatorLabels()
	{
		_indicatorLabels.Clear();
	}

	private void UpdateIndicatorLabels()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _indicatorLabels.Count; i++)
		{
			IndicatorLabel indicatorLabel = ((ListObjectPoolBase<GameObject>)_indicatorLabels).Get<IndicatorLabel>(i);
			((Component)indicatorLabel).transform.localPosition = Vector2.op_Implicit(KSingleton<MapContext>.Instance().TileToMapPosition(indicatorLabel.Indicator.GetTile()) + _indicatorLabelOffset);
		}
	}

	public void AddAnnounceBalloon(AnnounceType type, Vector2 tilePos, PlayerInfo info)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_balloonContainer.AddAnnounceBalloon(type, tilePos, info);
	}

	public void HideToolTipLabel()
	{
		((Component)_fadeOutTooltipLabel).transform.parent = ((Component)this).transform;
		((Component)_fadeOutTooltipLabel).gameObject.SetActive(false);
	}

	private void UpdateAreaEffectIndicator()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vector2 center = TerrainA6.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition);
		MapContext mapContext = KSingleton<MapContext>.Instance();
		for (int num = _areaEffectIndicatorPool.Count - 1; num >= 0; num--)
		{
			AreaEffectIndicator areaEffectIndicator = _areaEffectIndicatorPool[num];
			if (areaEffectIndicator.Check(center))
			{
				((Component)areaEffectIndicator).transform.localPosition = Vector2.op_Implicit(mapContext.TileToMapPosition(areaEffectIndicator.Indicator.GetTile()));
				if (areaEffectIndicator.FixedScale)
				{
					((Component)areaEffectIndicator).transform.localScale = Vector3.one;
				}
				else
				{
					((Component)areaEffectIndicator).transform.localScale = Vector3.one * mapContext.CurrentZoomScale() * (float)mapContext.MapNGUISize / (float)mapContext.MapSize;
				}
			}
			else
			{
				RemoveAreaEffectIndicator(areaEffectIndicator);
			}
		}
	}

	private void OnClickIndicator(GameObject obj)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		MapIndicator mapIndicator = null;
		Vector3 point = ((RaycastHit)(ref UICamera.lastHit)).point;
		float num = 6400f;
		for (int i = 0; i < _indicators.Count; i++)
		{
			MapIndicator mapIndicator2 = _indicators[i];
			Vector3 position = ((Component)mapIndicator2).transform.position;
			Vector3 val = point - position;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				mapIndicator = mapIndicator2;
				num = sqrMagnitude;
			}
		}
		if ((Object)(object)mapIndicator != (Object)null && this.IndicatorClicked != null)
		{
			this.IndicatorClicked(mapIndicator);
			if (!string.IsNullOrEmpty(mapIndicator.Tooltip))
			{
				_fadeOutTooltipLabel.Show(mapIndicator, mapIndicator.Tooltip);
			}
		}
	}

	private void OnDragIndicator(GameObject obj, Vector2 delta)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (this.IndicatorDraged != null)
		{
			this.IndicatorDraged(delta);
		}
	}

	private void SocialSystem_ChatAdded(ChatStruct chat)
	{
		if (!((Object)(object)chat.Chatter != (Object)null) || chat.Chatter.ChatLineAddible)
		{
			ParseAnnouncePosition(chat);
		}
	}

	private void Conversation_MessageUpdated(Conversation conv)
	{
		if (conv.Messages.Count != 0)
		{
			ChatStruct chat = conv.Messages[conv.Messages.Count - 1];
			ParseAnnouncePosition(chat);
		}
	}

	private void ParseAnnouncePosition(ChatStruct chat)
	{
		if (chat.MsgType != 0 || !MapPositionParser.TryGetPosition(chat.FindText(), out var humaneX, out var humaneY))
		{
			return;
		}
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(chat.EntityId, delegate(PlayerInfo info)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			if (info.Valid)
			{
				Vector3 position = MapPositionParser.HumaneTileToPosition(new Vector2((float)humaneX, (float)humaneY));
				_balloonContainer.AddAnnounceBalloon(AnnounceType.MyPosition, TerrainA6.WorldPositionToTilePosition(position), info);
			}
		}, useOldCache: true);
	}

	public void RevealStaticIndicators(IBitArray2d visibleGrid)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _indicators.Count; i++)
		{
			MapIndicator mapIndicator = _indicators[i];
			if (mapIndicator.VisibleType == IndicatorVisibleType.Dark)
			{
				mapIndicator.SetVisible(isVisible: true);
			}
			else if (mapIndicator.VisibleType == IndicatorVisibleType.Fog)
			{
				Vector2 val = TerrainA6.TilePositionToChunkCoords(new Point2(mapIndicator.GetTile()));
				bool visible = visibleGrid.Get((int)((Vector2)(ref val))[0], (int)((Vector2)(ref val))[1]);
				mapIndicator.SetVisible(visible);
			}
		}
	}

	private void UpdateIndicators()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = TerrainA6.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition);
		MapContext mapContext = KSingleton<MapContext>.Instance();
		int num = 81;
		for (int num2 = _indicators.Count - 1; num2 >= 0; num2--)
		{
			MapIndicator mapIndicator = _indicators[num2];
			if (!mapIndicator.IsValid())
			{
				RemoveIndicator(num2);
			}
			else
			{
				Vector2 tile = mapIndicator.GetTile();
				if (mapIndicator.VisibleType == IndicatorVisibleType.Reveal)
				{
					Vector2 val2 = tile - val;
					mapIndicator.SetVisible(((Vector2)(ref val2)).sqrMagnitude < (float)num);
				}
				Transform transform = ((Component)mapIndicator).transform;
				if (!mapIndicator.FixedScale)
				{
					transform.localScale = Vector3.one * mapContext.CurrentZoomScale();
				}
				transform.localPosition = Vector2.op_Implicit(mapContext.TileToMapPosition(tile));
				mapIndicator.OnUpdate();
			}
		}
	}
}
