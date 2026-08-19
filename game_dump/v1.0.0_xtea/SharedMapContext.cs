using System;
using K1Network;
using MapData;
using Messages;
using Player;
using TerrainData;
using UnityEngine;

public class SharedMapContext : MonoBehaviour
{
	private const int countPerChunk = 324;

	private const float _zoomMinRatio = 1f;

	private const float _zoomMaxRatio = 3f;

	[SerializeField]
	private Transform _container;

	[SerializeField]
	private Material _sharedMapMaterial;

	[SerializeField]
	private UITexture _mapTexture;

	[SerializeField]
	private FogOfSharedMap _fogOfSharedMap;

	[SerializeField]
	private BalloonContainer _balloonContainer;

	private readonly float _sin45 = Mathf.Sin((float)Math.PI / 4f);

	private readonly float _cos45 = Mathf.Cos((float)Math.PI / 4f);

	private readonly BitArray2d _chunkLoaded = new BitArray2d();

	private readonly BitArray2d _visibleGrid = new BitArray2d();

	private Vector4 _mapRect;

	private Vector2 _offset = Vector2.zero;

	private Color[] _chunkColorBuffer;

	private Material _prevMapMaterial;

	private Vector2 _revealCenter;

	private Vector2 _prevRevealCenter = -Vector2.one;

	public float ZoomScale { get; private set; }

	public Vector2 Offset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _offset;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			if (_offset != value)
			{
				_offset = value;
				float num = _sin45 * 1280f * ZoomScale;
				Vector4 val = -_mapRect * num;
				_offset.x = Mathf.Clamp(_offset.x, val.y, val.x);
				_offset.y = Mathf.Clamp(_offset.y, val.w, val.z);
				_container.localPosition = Vector2.op_Implicit(_offset);
			}
		}
	}

	public int MapSize { get; private set; }

	public Vector2 FocusPoint { get; private set; }

	public ulong RegionId { get; private set; }

	public int MapNGUISize { get; private set; }

	public event Action InitFinished;

	public event Action ZoomChanged;

	private void Awake()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		Connections.Frontend.On(delegate(RegionMapInfo msg, PacketHeader header)
		{
			HandleRegionMapInfo(msg);
		});
		MapNGUISize = 1280;
		_mapTexture.width = MapNGUISize;
		_mapTexture.height = MapNGUISize;
		Rect uvRect = default(Rect);
		((Rect)(ref uvRect)).width = 1f;
		((Rect)(ref uvRect)).height = 1f;
		_mapTexture.uvRect = uvRect;
		UITexture mapTexture = _mapTexture;
		mapTexture.onRender = (UIDrawCall.OnRenderCallback)Delegate.Combine(mapTexture.onRender, new UIDrawCall.OnRenderCallback(OnRender_MapTexture));
		_balloonContainer.TileToMapPosition = (Vector2 tilePos) => TileToMapPosition(tilePos);
		_balloonContainer.TileToHumanePosition = (Vector2 tilePos) => MapPositionParser.PositionToHumaneTile(TerrainA6.TilePositionToWorldPosition(tilePos), MapSize * 200);
		Zoom(0f, Vector2.zero);
	}

	public void InitTerrain(ulong regionId, Vector2 posFocus)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		FocusPoint = posFocus;
		if (RegionId != regionId)
		{
			Connections.Frontend.Send(new GetRegionMapInfo
			{
				RegionId = regionId
			});
		}
		else if (this.InitFinished != null)
		{
			this.InitFinished();
		}
	}

	public void SetPinPoint(Player.PlayerInfo info, Vector2 tilePos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_balloonContainer.AddAnnounceBalloon(AnnounceType.SharePinPoint, tilePos, info);
		RefreshPinPosition();
	}

	public void RefreshPinPosition()
	{
		_balloonContainer.UpdatePosition();
	}

	public void Zoom(float zoomDelta, Vector2 center)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((float)Screen.width / 2f, (float)Screen.height / 2f);
		Vector2 val2 = center - val;
		float zoomScale = ZoomScale;
		ZoomScale *= 1f + zoomDelta;
		ZoomScale = Mathf.Clamp(ZoomScale, 1f, 3f);
		float num = ((!(zoomScale > 0f)) ? 1f : (ZoomScale / zoomScale));
		Offset = (Offset - val2) * num + val2;
		((Component)_mapTexture).transform.localScale = Vector3.one * ZoomScale;
		if (this.ZoomChanged != null)
		{
			this.ZoomChanged();
		}
	}

	private void HandleRegionMapInfo(RegionMapInfo msgRegionMapInfo)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		MapSize = msgRegionMapInfo.TileCount.x;
		RegionId = msgRegionMapInfo.RegionId;
		int num = MapSize / 16;
		_chunkLoaded.Refresh(num, num);
		_visibleGrid.Refresh(num, num);
		Texture2D orCreateTexture = GetOrCreateTexture(MapSize);
		Color32[] array = (Color32[])(object)new Color32[((Texture)orCreateTexture).width * ((Texture)orCreateTexture).height];
		Color biomeColor = MapIndicatorMeta.GetBiomeColor(Biome.WarmOcean);
		for (int i = 0; i < array.Length; i++)
		{
			ref Color32 reference = ref array[i];
			reference = Color32.op_Implicit(biomeColor);
		}
		orCreateTexture.SetPixels32(array);
		DefoggedChunks defoggedChunks = msgRegionMapInfo.DefoggedChunks;
		for (int j = 0; j < defoggedChunks.Chunks.Length; j++)
		{
			int x = defoggedChunks.Chunks[j].x;
			int y = defoggedChunks.Chunks[j].y;
			_visibleGrid.Set(x, y, value: true);
			int num2 = ((x == 0) ? 1 : 0);
			int num3 = ((y == 0) ? 1 : 0);
			int num4 = ((x != num - 1) ? 18 : 17);
			int num5 = ((y != num - 1) ? 18 : 17);
			int num6 = num4 - num2;
			int num7 = num5 - num3;
			TerrainChunkA6 terrainChunk = KSingleton<TerrainA6>.Instance().GetTerrainChunk(new Vector2((float)x, (float)y));
			Color[] chunkColorBuffer = GetChunkColorBuffer(num6, num7);
			int num8 = x * 16 - 1 + num2;
			int num9 = y * 16 - 1 + num3;
			for (int k = num2; k < num4; k++)
			{
				for (int l = num3; l < num5; l++)
				{
					byte biome = defoggedChunks.Biomes[j * 324 + l * 18 + k];
					Color tileColor = MapContext.GetTileColor(terrainChunk, biome, new Point2(k - 1 + num2, l - 1 + num3), new Point2(k + num8, l + num9));
					chunkColorBuffer[(l - num3) * num6 + (k - num2)] = tileColor;
				}
			}
			orCreateTexture.SetPixels(num8, num9, num6, num7, chunkColorBuffer);
			_chunkLoaded.Set(x, y, value: true);
		}
		orCreateTexture.Apply();
		UpdateVisibleMapRect();
		_fogOfSharedMap.SetDefoggedChunks(MapSize, _visibleGrid);
		if (this.InitFinished != null)
		{
			this.InitFinished();
		}
	}

	private Texture2D GetOrCreateTexture(int length)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		Texture texture = _sharedMapMaterial.GetTexture("_MainTex");
		Texture2D val = (Texture2D)(object)((texture is Texture2D) ? texture : null);
		if ((Object)(object)val == (Object)null || ((Texture)val).width != length || ((Texture)val).height != length)
		{
			val = new Texture2D(MapSize, MapSize, (TextureFormat)3, false);
			((Texture)val).wrapMode = (TextureWrapMode)1;
			((Texture)val).filterMode = (FilterMode)1;
			_sharedMapMaterial.SetTexture("_MainTex", (Texture)(object)val);
			_mapTexture.material = _sharedMapMaterial;
		}
		return val;
	}

	private void OnRender_MapTexture(Material mat)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mat != (Object)null && ((Object)(object)mat != (Object)(object)_prevMapMaterial || _revealCenter != _prevRevealCenter))
		{
			Vector4 val = default(Vector4);
			val.x = (_revealCenter.x - 10f) / (float)MapSize;
			val.y = (_revealCenter.y - 10f) / (float)MapSize;
			val.z = (_revealCenter.x + 10f) / (float)MapSize;
			val.w = (_revealCenter.y + 10f) / (float)MapSize;
			mat.SetVector("_RevealRange", val);
			_prevMapMaterial = mat;
			_prevRevealCenter = _revealCenter;
		}
	}

	private Color[] GetChunkColorBuffer(int sizeX, int sizeY)
	{
		if (_chunkColorBuffer == null || _chunkColorBuffer.Length != sizeX * sizeY)
		{
			_chunkColorBuffer = (Color[])(object)new Color[sizeX * sizeY];
		}
		return _chunkColorBuffer;
	}

	private Vector2 TileToMapPosition(Vector2 tilePos)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)MapSize * 0.5f;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(num, num);
		Vector2 val2 = (tilePos - val) / (float)MapSize * (float)MapNGUISize;
		val2 *= ((Component)_mapTexture).transform.localScale.x;
		return new Vector2(val2.x * _cos45 - val2.y * _cos45, val2.x * _cos45 + val2.y * _cos45);
	}

	private void UpdateVisibleMapRect()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		int num = MapSize / 16;
		int num2 = num;
		int num3 = num;
		Vector4 rect = default(Vector4);
		((Vector4)(ref rect))._002Ector(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				if (_chunkLoaded.Get(i, j))
				{
					AdjustVisibleMapRect(ref rect, i, j, num2, num3);
				}
			}
		}
		AdjustVisibleMapRect(ref rect, (int)(FocusPoint.x / 16f), (int)(FocusPoint.y / 16f), num2, num3);
		float num4 = (float)(num2 - 1) / 2f * Mathf.Sqrt(2f);
		rect = (_mapRect = rect / num4);
	}

	private static void AdjustVisibleMapRect(ref Vector4 rect, int x, int y, int maxX, int maxY)
	{
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((float)x - (float)(maxX - 1) / 2f, (float)y - (float)(maxY - 1) / 2f);
		float num = Mathf.Atan2(val.y, val.x) + (float)Math.PI / 4f;
		float magnitude = ((Vector2)(ref val)).magnitude;
		float num2 = Mathf.Cos(num) * magnitude;
		float num3 = Mathf.Sin(num) * magnitude;
		rect.x = Mathf.Min(rect.x, num2);
		rect.y = Mathf.Max(rect.y, num2);
		rect.z = Mathf.Min(rect.z, num3);
		rect.w = Mathf.Max(rect.w, num3);
	}
}
