using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Messages;
using TerrainData;
using UnityEngine;

public class MapContext : KSingleton<MapContext>
{
	public const float ZoomMinBaseRatio = 1f;

	public const float ZoomMaxBaseRatio = 3f;

	public const float BaseZoomScaleMapSize = 512f;

	public const int RevealSize = 20;

	private const float MeterPerPixel = 0.5f;

	public const int WorldMapSize = 1280;

	private float _zoomMinRatio = 1f;

	private float _zoomMaxRatio = 3f;

	[SerializeField]
	private FogOfWarCover _fowCover;

	[SerializeField]
	private Transform _container;

	[SerializeField]
	private Material _worldMaterial;

	[SerializeField]
	private UITexture _mapTexture;

	[SerializeField]
	private UILabel _posLabel;

	[SerializeField]
	private Vector2 _worldmapPosLabelOffset;

	[SerializeField]
	private Vector2 _minimapPosLabelOffset;

	private readonly float _cos45 = Mathf.Cos((float)Math.PI / 4f);

	private readonly float _sin45 = Mathf.Sin((float)Math.PI / 4f);

	private Vector4 _mapRect;

	private Vector2 _offset = Vector2.zero;

	private int _totalMapMeter = 4096;

	private bool _isWorldMapMode;

	private readonly BitArray2d _chunkLoaded = new BitArray2d();

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
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			if (!(_offset == value))
			{
				_offset = value;
				if (_isWorldMapMode)
				{
					float num = _sin45 * 1280f * ZoomScale;
					Vector4 val = -_mapRect * num;
					_offset.x = Mathf.Clamp(_offset.x, val.y, val.x);
					_offset.y = Mathf.Clamp(_offset.y, val.w, val.z);
				}
				_container.localPosition = Vector2.op_Implicit(_offset);
				if (!_isWorldMapMode)
				{
					((Component)_mapTexture).transform.localPosition = Vector2.op_Implicit(-_offset);
				}
				else
				{
					((Component)_mapTexture).transform.localPosition = Vector3.zero;
				}
			}
		}
	}

	public Point2 HumanePosition { get; set; }

	public int MapSize { get; private set; }

	public int MapNGUISize { get; private set; }

	public event Action ZoomChanged;

	public void Zoom(float zoomDelta, Vector2 center)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((float)Screen.width / 2f, (float)Screen.height / 2f);
		Vector2 val2 = center - val;
		float zoomScale = ZoomScale;
		ZoomScale *= 1f + zoomDelta;
		ZoomScale = Mathf.Clamp(ZoomScale, _zoomMinRatio, _zoomMaxRatio);
		float num = ((!(zoomScale > 0f)) ? 1f : (ZoomScale / zoomScale));
		Offset = (Offset - val2) * num + val2;
		((Component)_mapTexture).transform.localScale = Vector3.one * CurrentZoomScale();
		if (this.ZoomChanged != null)
		{
			this.ZoomChanged();
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	private void Start()
	{
		HumanePosition = new Point2(int.MaxValue, int.MaxValue);
		ZoomScale = 0f;
		_fowCover = ((Component)this).GetComponent<FogOfWarCover>();
		UITexture mapTexture = _mapTexture;
		mapTexture.onRender = (UIDrawCall.OnRenderCallback)Delegate.Combine(mapTexture.onRender, new UIDrawCall.OnRenderCallback(OnRender_MapTexture));
		_isWorldMapMode = false;
		((Component)_container).gameObject.SetActive(false);
		ApplyTerrainMeta();
		KSingleton<GameManager>.Instance().PostReconnect += ApplyTerrainMeta;
	}

	public float CurrentZoomScale()
	{
		return (!_isWorldMapMode) ? 1f : ZoomScale;
	}

	private void ApplyTerrainMeta()
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		_totalMapMeter = TerrainMeta.TileCount * 2;
		MapSize = TerrainMeta.TileCount;
		float num = (float)MapSize / 512f;
		_zoomMinRatio = 1f * num;
		_zoomMaxRatio = 3f * num;
		RecalcMapTextureSize();
		_chunkLoaded.Refresh(TerrainMeta.ChunkCount, TerrainMeta.ChunkCount);
		Texture texture = _worldMaterial.GetTexture("_MainTex");
		Texture2D val = (Texture2D)(object)((texture is Texture2D) ? texture : null);
		if ((Object)(object)val == (Object)null || ((Texture)val).width != MapSize || ((Texture)val).height != MapSize)
		{
			val = new Texture2D(MapSize, MapSize, (TextureFormat)3, false);
		}
		((Texture)val).wrapMode = (TextureWrapMode)1;
		((Texture)val).filterMode = (FilterMode)1;
		Color32[] array = (Color32[])(object)new Color32[((Texture)val).width * ((Texture)val).height];
		Color biomeColor = MapIndicatorMeta.GetBiomeColor(Biome.WarmOcean);
		for (int i = 0; i < array.Length; i++)
		{
			ref Color32 reference = ref array[i];
			reference = Color32.op_Implicit(biomeColor);
		}
		val.SetPixels32(array);
		_worldMaterial.SetTexture("_MainTex", (Texture)(object)val);
		_mapTexture.material = _worldMaterial;
		KSingleton<GameManager>.Instance().AddOnReady(delegate
		{
			Connections.Frontend.Send(default(GetDefoggedChunks));
		});
	}

	private void LateUpdate()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if (!((Object)(object)localPlayer == (Object)null) && TerrainA6.IsPlayerInitialized)
		{
			if (!((Component)_container).gameObject.activeSelf && IsLoadingCompleted())
			{
				((Component)_container).gameObject.SetActive(true);
			}
			Vector3 currentPosition = localPlayer.CurrentPosition;
			Vector2 playerTile = (_revealCenter = TerrainA6.ClientPositionToTilePosition(currentPosition));
			UpdateMap(playerTile);
			UpdatePosLabel(playerTile);
			UpdateChunkData();
		}
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

	public void Attach(bool worldMapMode, Transform parent)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		_isWorldMapMode = worldMapMode;
		RecalcMapTextureSize();
		((Component)this).transform.parent = parent;
		((Component)this).transform.localPosition = Vector3.zero;
		((Component)this).transform.localScale = Vector3.one;
		Offset = _offset;
		Zoom(0f, Vector2.zero);
	}

	public bool IsLoadingCompleted()
	{
		return _fowCover.IsDefoggingCompleted();
	}

	public void HandleDefoggedChunks(DefoggedChunks msg)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		int chunkCount = TerrainMeta.ChunkCount;
		BitArray2d bitArray2d = new BitArray2d(chunkCount, chunkCount);
		Texture2D val = null;
		for (int i = 0; i < msg.Chunks.Length; i++)
		{
			int x = msg.Chunks[i].x;
			int y = msg.Chunks[i].y;
			bitArray2d.Set(x, y, value: true);
			int num = ((x == 0) ? 1 : 0);
			int num2 = ((y == 0) ? 1 : 0);
			int num3 = ((x != chunkCount - 1) ? 18 : 17);
			int num4 = ((y != chunkCount - 1) ? 18 : 17);
			int num5 = num3 - num;
			int num6 = num4 - num2;
			TerrainChunkA6 terrainChunk = KSingleton<TerrainA6>.Instance().GetTerrainChunk(new Vector2((float)x, (float)y));
			Color[] orCreateChunkColorBuffer = GetOrCreateChunkColorBuffer(num5, num6);
			int num7 = x * 16 - 1 + num;
			int num8 = y * 16 - 1 + num2;
			for (int j = num; j < num3; j++)
			{
				for (int k = num2; k < num4; k++)
				{
					byte biome = msg.Biomes[i * 324 + k * 18 + j];
					Color tileColor = GetTileColor(terrainChunk, biome, new Point2(j - 1 + num, k - 1 + num2), new Point2(j + num7, k + num8));
					orCreateChunkColorBuffer[(k - num2) * num5 + (j - num)] = tileColor;
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				Texture mainTexture = _mapTexture.mainTexture;
				val = (Texture2D)(object)((mainTexture is Texture2D) ? mainTexture : null);
			}
			val.SetPixels(num7, num8, num5, num6, orCreateChunkColorBuffer);
			_chunkLoaded.Set(x, y, value: true);
		}
		if ((Object)(object)val != (Object)null)
		{
			val.Apply();
			UpdateVisibleMapRect();
		}
		_fowCover.SetDefoggedChunks(bitArray2d);
		KSingleton<MapIndicators>.Instance().RevealStaticIndicators(bitArray2d);
	}

	[NotNull]
	private Color[] GetOrCreateChunkColorBuffer(int sizeX, int sizeY)
	{
		if (_chunkColorBuffer == null || _chunkColorBuffer.Length != sizeX * sizeY)
		{
			_chunkColorBuffer = (Color[])(object)new Color[sizeX * sizeY];
		}
		return _chunkColorBuffer;
	}

	private void RecalcMapTextureSize()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (_isWorldMapMode)
		{
			MapNGUISize = 1280;
			_mapTexture.width = MapNGUISize;
			_mapTexture.height = MapNGUISize;
			Rect uvRect = default(Rect);
			((Rect)(ref uvRect)).width = 1f;
			((Rect)(ref uvRect)).height = 1f;
			_mapTexture.uvRect = uvRect;
		}
		else
		{
			MapNGUISize = (int)((float)_totalMapMeter / 0.5f);
			_mapTexture.width = 300;
			_mapTexture.height = 300;
			Rect uvRect2 = default(Rect);
			float height = (((Rect)(ref uvRect2)).width = 300f / (float)MapNGUISize);
			((Rect)(ref uvRect2)).height = height;
			_mapTexture.uvRect = uvRect2;
		}
	}

	private void UpdateVisibleMapRect()
	{
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		int chunkCount = TerrainMeta.ChunkCount;
		int chunkCount2 = TerrainMeta.ChunkCount;
		Vector4 val = default(Vector4);
		((Vector4)(ref val))._002Ector(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
		Vector2 val2 = default(Vector2);
		for (int i = 0; i < chunkCount; i++)
		{
			for (int j = 0; j < chunkCount2; j++)
			{
				if (_chunkLoaded.Get(i, j))
				{
					((Vector2)(ref val2))._002Ector((float)i - (float)(chunkCount - 1) / 2f, (float)j - (float)(chunkCount2 - 1) / 2f);
					float num = Mathf.Atan2(val2.y, val2.x) + (float)Math.PI / 4f;
					float magnitude = ((Vector2)(ref val2)).magnitude;
					float num2 = Mathf.Cos(num) * magnitude;
					float num3 = Mathf.Sin(num) * magnitude;
					val.x = Mathf.Min(val.x, num2);
					val.y = Mathf.Max(val.y, num2);
					val.z = Mathf.Min(val.z, num3);
					val.w = Mathf.Max(val.w, num3);
				}
			}
		}
		float num4 = (float)(chunkCount - 1) / 2f * Mathf.Sqrt(2f);
		val = (_mapRect = val / num4);
	}

	public Vector2 TileToMapPosition(Vector2 tilePos, bool applyScale = true)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)TerrainMeta.TileCount * 0.5f;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(num, num);
		Vector2 val2 = (tilePos - val) / (float)MapSize * (float)MapNGUISize;
		if (applyScale)
		{
			val2 *= ((Component)_mapTexture).transform.localScale.x;
		}
		return new Vector2(val2.x * _cos45 - val2.y * _cos45, val2.x * _cos45 + val2.y * _cos45);
	}

	private void UpdateMap(Vector2 playerTile)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (!_isWorldMapMode)
		{
			Vector2 val = TileToMapPosition(playerTile);
			Offset = -val;
			Rect uvRect = _mapTexture.uvRect;
			((Rect)(ref uvRect)).center = playerTile / (float)MapSize;
			_mapTexture.uvRect = uvRect;
		}
	}

	private void UpdatePosLabel(Vector2 playerTile)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = MapPositionParser.PositionToHumaneTile(TerrainA6.TilePositionToWorldPosition(playerTile));
		Point2 point = new Point2(Mathf.RoundToInt(val.x), Mathf.RoundToInt(val.y));
		if (HumanePosition != point)
		{
			string text = AnnounceBalloon.GetPositionText(point.x, point.y);
			if (Debug.isDebugBuild)
			{
				text += $"\n({(int)playerTile.x}, {(int)playerTile.y})";
			}
			_posLabel.text = text;
			HumanePosition = point;
		}
		((Component)_posLabel).transform.localPosition = Vector2.op_Implicit(TileToMapPosition(playerTile) + ((!_isWorldMapMode) ? _minimapPosLabelOffset : _worldmapPosLabelOffset));
	}

	private void UpdateChunkData()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		TerrainA6 terrainA = KSingleton<TerrainA6>.Instance();
		int chunkPoolSize = terrainA.GetChunkPoolSize();
		int chunkCount = TerrainMeta.ChunkCount;
		int chunkCount2 = TerrainMeta.ChunkCount;
		Texture2D val = null;
		for (int i = 0; i < chunkPoolSize; i++)
		{
			TerrainChunkA6 terrainChunk = terrainA.GetTerrainChunk(i);
			int num = (int)terrainChunk.Coords.x;
			int num2 = (int)terrainChunk.Coords.y;
			if (num < 0 || num >= chunkCount || num2 < 0 || num2 >= chunkCount2 || _chunkLoaded.Get(num, num2) || !terrainChunk.HasTileBiome())
			{
				continue;
			}
			Color[] orCreateChunkColorBuffer = GetOrCreateChunkColorBuffer(16, 16);
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					byte rawTileBiome = terrainChunk.GetRawTileBiome(j, k);
					Color tileColor = GetTileColor(terrainChunk, rawTileBiome, new Point2(j, k));
					orCreateChunkColorBuffer[k * 16 + j] = tileColor;
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				Texture mainTexture = _mapTexture.mainTexture;
				val = (Texture2D)(object)((mainTexture is Texture2D) ? mainTexture : null);
			}
			val.SetPixels(num * 16, num2 * 16, 16, 16, orCreateChunkColorBuffer);
			_chunkLoaded.Set(num, num2, value: true);
		}
		if ((Object)(object)val != (Object)null)
		{
			val.Apply();
			UpdateVisibleMapRect();
		}
	}

	public static Color GetTileColor(TerrainChunkA6 chunk, byte biome, Point2 localTile, [Optional] Point2 worldTile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetBiomeColor(biome);
	}

	public static Color GetBiomeColor(byte biome)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (TerrainA6.IsCollidableMasked(biome))
		{
			return MapIndicatorMeta.LandMakrColor;
		}
		if (TerrainA6.IsNotPlantableMasked(biome))
		{
			return MapIndicatorMeta.ScoopColor;
		}
		return MapIndicatorMeta.GetBiomeColor((Biome)biome);
	}

	public void RefreshChunk(int x, int y)
	{
		_chunkLoaded.Set(x, y, value: false);
	}

	public Vector2 ScreenPosToTilePos(Vector2 screenPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = NGUIMath.ScreenToParentPixels(screenPos, ((Component)_mapTexture).transform) / ((Component)_mapTexture).transform.localScale.x;
		Vector2 val2 = default(Vector2);
		val2.x = val.x * _cos45 + val.y * _sin45;
		val2.y = (0f - val.x) * _sin45 + val.y * _cos45;
		Vector2 result = Vector2.op_Implicit(Vector3.zero);
		result.x = (val2.x / (float)MapNGUISize + 0.5f) * (float)MapSize;
		result.y = (val2.y / (float)MapNGUISize + 0.5f) * (float)MapSize;
		return result;
	}

	public void SetIslandView()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		ZoomScale = _zoomMinRatio;
		float num = _sin45 * 1280f * ZoomScale;
		Vector4 val = -_mapRect * num;
		Offset = new Vector2(Mathf.Lerp(val.y, val.x, 0.5f), Mathf.Lerp(val.w, val.z, 0.5f));
		Zoom(0f, Vector2.zero);
	}
}
