using System;
using System.Collections.Generic;
using Estate;
using K1Network;
using MapData;
using Messages;
using Shared.Estate;
using UnityEngine;

public class EstateSystem : GameSystem<EstateSystem>
{
	private readonly List<Estate.EstateInfo> _estates = new List<Estate.EstateInfo>();

	private static AccessRights[] _rightsList;

	public static AccessRights[] RightsList
	{
		get
		{
			if (_rightsList == null)
			{
				Array values = Enum.GetValues(typeof(AccessRights));
				List<AccessRights> list = new List<AccessRights>();
				for (int i = 0; i < values.Length; i++)
				{
					AccessRights accessRights = (AccessRights)(int)values.GetValue(i);
					if (accessRights > AccessRights.None)
					{
						list.Add(accessRights);
					}
				}
				_rightsList = list.ToArray();
			}
			return _rightsList;
		}
	}

	public static KeyValuePair<PresetLicense, AccessRights>[] Presets { get; private set; }

	public InteractionRights Rights { get; private set; }

	private void Awake()
	{
		Connections.Frontend.On<EstateGrids>(OnEstateGrid);
		Connections.Frontend.On<EstateLicenseChanged>(OnEstateLicenseChanged);
		KSingleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			if (!GameManager.IsPrologueMode)
			{
				Init();
			}
		};
	}

	private void Init()
	{
		InitPermissionPreset();
		KSingleton<TerrainA6>.Instance().LoadingChunksFinished += OnChunkLoadFinish;
		_estates.Clear();
	}

	private void InitPermissionPreset()
	{
		Presets = new KeyValuePair<PresetLicense, AccessRights>[3]
		{
			new KeyValuePair<PresetLicense, AccessRights>(PresetLicense.All, AccessRights.Enter | AccessRights.UseFacility | AccessRights.Give | AccessRights.Take | AccessRights.Occupy | AccessRights.Destruct),
			new KeyValuePair<PresetLicense, AccessRights>(PresetLicense.Some, AccessRights.Enter | AccessRights.UseFacility | AccessRights.Give),
			new KeyValuePair<PresetLicense, AccessRights>(PresetLicense.None, AccessRights.None)
		};
	}

	public void InitInteractionRights(Dictionary<int, int[]> dict)
	{
		Rights = new InteractionRights(dict);
	}

	private void OnEstateGrid(EstateGrids msg, PacketHeader header)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		Vector2 coords = default(Vector2);
		for (int size = KUtility.GetSize(msg.Chunks); i < size; i++)
		{
			((Vector2)(ref coords))._002Ector((float)msg.Chunks[i].Key, (float)msg.Chunks[i].Value);
			TerrainChunkA6 terrainChunk = KSingleton<TerrainA6>.Instance().GetTerrainChunk(coords);
			if ((Object)(object)terrainChunk == (Object)null)
			{
				continue;
			}
			for (int j = 0; j < 4; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					Point2 unit = terrainChunk.ChunkTileOffset / 4 + new Point2(j, k);
					Messages.EstateInfo estateInfo = msg.Estates.Get(new KeyValuePair<int, int>(unit.x, unit.y));
					SetEstateId(unit, estateInfo.Id, estateInfo.OwnerId);
				}
			}
		}
		foreach (KeyValuePair<KeyValuePair<int, int>, Messages.EstateInfo> estate in msg.Estates)
		{
			Estate.EstateInfo estateInfo2 = RequestEstateInfo(estate.Value.Id, forceRefresh: false);
			Point2 point = new Point2(estate.Key.Key, estate.Key.Value);
			estateInfo2.AddUnit(point);
			if (estate.Value.Type == OwnerType.Player)
			{
				Color color = Color32.op_Implicit((estate.Value.OwnerId != GameManager.PlayerId) ? new Color32((byte)198, (byte)198, (byte)198, byte.MaxValue) : new Color32(byte.MaxValue, (byte)198, (byte)0, byte.MaxValue));
				MapEstateIndicator mapEstateIndicator = MapIndicators.Add<MapEstateIndicator>(point, IndicatorType.Estate);
				mapEstateIndicator.Set(point, 4, color);
			}
		}
		UpdateEstateBorder();
	}

	private void SetEstateId(Point2 unit, ulong estateId, ulong ownerId)
	{
		Point2 point = unit * 4;
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(point);
		if ((Object)(object)chunkFromTile == (Object)null)
		{
			return;
		}
		ulong num = 0uL;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				Point2 tile = chunkFromTile.FromWorldTile(point + new Point2(i, j));
				TileObject tileObject = chunkFromTile.StaticObjectChunk.GetTileObject(tile);
				if (tileObject != null)
				{
					num = tileObject.EstateId;
					tileObject.EstateId = estateId;
					tileObject.OwnerId = ownerId;
				}
			}
		}
		if (num != 0L)
		{
			GetEstateInfo(num)?.RemoveUnit(unit);
		}
	}

	private void OnEstateLicenseChanged(EstateLicenseChanged msg, PacketHeader header)
	{
		ulong? estateId = msg.EstateId;
		if (estateId.HasValue)
		{
			RequestEstateInfo(msg.EstateId.Value, forceRefresh: true);
			return;
		}
		ulong? clanId = msg.ClanId;
		if (!clanId.HasValue)
		{
			return;
		}
		int i = 0;
		for (int count = _estates.Count; i < count; i++)
		{
			if (_estates[i].OwnerType == OwnerType.ClanEstate && _estates[i].Owner == msg.ClanId.Value)
			{
				RequestEstateInfo(_estates[i].Id, forceRefresh: true);
			}
		}
	}

	private int EstateInfoIndexOf(ulong id)
	{
		int i = 0;
		for (int count = _estates.Count; i < count; i++)
		{
			if (_estates[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	public Estate.EstateInfo GetEstateInfo(ulong id)
	{
		int num = EstateInfoIndexOf(id);
		return (num != -1) ? _estates[num] : null;
	}

	private Estate.EstateInfo RequestEstateInfo(ulong id, bool forceRefresh)
	{
		Estate.EstateInfo info = GetEstateInfo(id);
		if (info == null)
		{
			info = new Estate.EstateInfo(id);
			_estates.Add(info);
		}
		else if (!forceRefresh && info.State != 0)
		{
			return info;
		}
		if (info.RestrictedArea)
		{
			info.State = Estate.EstateInfo.StateEnum.Loaded;
			return info;
		}
		info.State = Estate.EstateInfo.StateEnum.Loading;
		string url = $"{KSingleton<GameManager>.Instance().GatewayUrl}estate_licenses/{id}";
		KUtility.RequestYml(url, delegate(EstateJson json)
		{
			if (json == null)
			{
				info.State = Estate.EstateInfo.StateEnum.None;
			}
			else
			{
				info.Set(json);
				info.State = Estate.EstateInfo.StateEnum.Loaded;
				info.RefreshEstateFences();
			}
		}, forceRefresh);
		return info;
	}

	private void OnChunkLoadFinish()
	{
		UpdateEstateBorder();
	}

	private void UpdateEstateBorder()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (KSingleton<TerrainA6>.Instance().IsChunkLoading)
		{
			return;
		}
		for (int num = _estates.Count - 1; num >= 0; num--)
		{
			_estates[num].RefreshEstateFences();
			Rect bound = _estates[num].Bound;
			Vector2[] array = (Vector2[])(object)new Vector2[4]
			{
				new Vector2(((Rect)(ref bound)).xMin, ((Rect)(ref bound)).yMin),
				new Vector2(((Rect)(ref bound)).xMin, ((Rect)(ref bound)).yMax),
				new Vector2(((Rect)(ref bound)).xMax, ((Rect)(ref bound)).yMax),
				new Vector2(((Rect)(ref bound)).xMax, ((Rect)(ref bound)).yMin)
			};
			bool flag = false;
			int i = 0;
			for (int num2 = array.Length; i < num2; i++)
			{
				TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(new Point2(array[i]));
				if ((Object)(object)chunkFromTile != (Object)null)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_estates[num].Dispose();
				_estates.RemoveAt(num);
			}
		}
	}

	public void SetEstateLicense(ulong estateId, License license)
	{
		Connections.Frontend.Send(new SetEstateLicense
		{
			EstateId = estateId,
			License = license
		});
	}

	public void SetClanEstateLicense(License license)
	{
		Connections.Frontend.Send(new SetClanEstateLicense
		{
			License = license
		});
	}

	public void RequestAddEstateUnit(ulong id, Point2 estatePos)
	{
		Connections.Frontend.Send(new AddEstateUnit
		{
			EstateId = id,
			Unit = new KeyValuePair<int, int>(estatePos.x, estatePos.y)
		});
	}
}
