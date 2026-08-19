using System;
using System.Diagnostics;
using System.IO;
using Durango.Logic.Estate;
using Durango.Network;
using Durango.Terrain;
using Durango.Utils;
using Messages;
using Shared.Estate;
using Shared.Region;
using UnityEngine;

namespace Durango.UI;

public class MapContext : Singleton<MapContext>
{
	public const int RevealSize = 20;

	public const int WorldMapSize = 1280;

	private const float MeterPerPixel = 0.5f;

	public const float BaseZoomScaleMapSize = 512f;

	public const float ZoomMidBaseRatio = 1f;

	public const float ZoomMaxBaseRatio = 3f;

	public const float ZoomMinRatio = 0.5f;

	private float _zoomMidRatio;

	private float _zoomMaxRatio;

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
	private Color _posVariableColor;

	[SerializeField]
	private Color _posValueColor;

	[SerializeField]
	private Vector2 _worldmapPosLabelOffset;

	[SerializeField]
	private Vector2 _minimapPosLabelOffset;

	[SerializeField]
	private UISprite _worldmapPosBG;

	[SerializeField]
	private UISprite _minimapPosBG;

	[SerializeField]
	[EnumList(typeof(Biome), true, 0, -1)]
	private Color[] _biomeColors;

	[SerializeField]
	private Color _landmarkColor;

	[SerializeField]
	private Color _scoopColor;

	private readonly float _cos45 = Mathf.Cos((float)Math.PI / 4f);

	private readonly float _sin45 = Mathf.Sin((float)Math.PI / 4f);

	private float _zoomScale;

	private readonly BitArray2D _dirtyChunk = new BitArray2D();

	private bool _hasDirtyChunk;

	private readonly Color[] _chunkColorBuffer = new Color[256];

	private Material _prevMapMaterial;

	private Vector2 _revealCenter;

	private Vector2 _prevRevealCenter = -Vector2.one;

	private byte[] _wholeBiomes;

	private string _terrainId;

	private double _nextReloadTime;

	private bool _isWorldMapMode = true;

	private float ZoomMidRatio
	{
		get
		{
			return _zoomMidRatio;
		}
		set
		{
			_zoomMidRatio = ((!(value > 0.5f)) ? 0.5f : value);
		}
	}

	private float ZoomMaxRatio
	{
		get
		{
			return _zoomMaxRatio;
		}
		set
		{
			_zoomMaxRatio = ((!(value > ZoomMidRatio)) ? ZoomMidRatio : value);
		}
	}

	public float CurrentZoomScale => (!IsWorldMapMode) ? 1f : _zoomScale;

	public float RelativeZoomScale => CurrentZoomScale * 512f / (float)MapSize;

	public bool IsWorldMapMode
	{
		get
		{
			return _isWorldMapMode;
		}
		set
		{
			if (_isWorldMapMode != value)
			{
				_isWorldMapMode = value;
				SetPosLabelBG(_isWorldMapMode);
			}
		}
	}

	public Vector2 Offset { get; private set; }

	public Point2 HumanePosition { get; set; }

	public int MapSize { get; private set; }

	public int MapNGUISize { get; private set; }

	public event Action ScaleChanged;

	public event Action Attached;

	protected override void OnAwake()
	{
		_worldMaterial = UnityEngine.Object.Instantiate(_worldMaterial);
		HumanePosition = new Point2(int.MaxValue, int.MaxValue);
		UITexture mapTexture = _mapTexture;
		mapTexture.onRender = (UIDrawCall.OnRenderCallback)Delegate.Combine(mapTexture.onRender, new UIDrawCall.OnRenderCallback(OnRender_MapTexture));
		IsWorldMapMode = false;
		ApplyTerrainMeta();
		Singleton<GameManager>.Instance().PostReconnect += ApplyTerrainMeta;
		Connections.Frontend.On(delegate(DefoggedChunks msg, PacketHeader header)
		{
			int chunkCount = TerrainMeta.ChunkCount;
			BitArray2D bitArray2D = new BitArray2D(chunkCount, chunkCount);
			for (int i = 0; i < msg.Chunks.Length; i++)
			{
				int x = msg.Chunks[i].x;
				int y = msg.Chunks[i].y;
				bitArray2D.Set(x, y, value: true);
			}
			_fowCover.SetDefoggedChunks(bitArray2D);
			GameSystem<EstateSystem>.Instance().UpdateEstateInfos();
		});
	}

	public void FocusToTilePostion(Vector2 tilePosition)
	{
		Vector2 vector = TileToMapPosition(tilePosition);
		Focus(-vector);
	}

	public void Focus(Vector2 offset)
	{
		Offset = offset;
		if (IsWorldMapMode)
		{
			float num = _sin45 * 1280f * _zoomScale;
			float num2 = Mathf.Max(0f, num - (float)UIManager.ScreenWidth * 0.5f) + (float)UIManager.ScreenWidth * 0.1f;
			float num3 = Mathf.Max(0f, num - (float)UIManager.ScreenHeight * 0.5f) + (float)UIManager.ScreenHeight * 0.1f;
			float x = Mathf.Clamp(Offset.x, 0f - num2, num2);
			float y = Mathf.Clamp(Offset.y, 0f - num3, num3);
			Offset = new Vector2(x, y);
		}
		RefreshMapPosition();
	}

	private void RefreshMapPosition()
	{
		_container.localPosition = Offset;
		_mapTexture.transform.localPosition = ((!IsWorldMapMode) ? new Vector3(0f - Offset.x, 0f - Offset.y) : Vector3.zero);
	}

	private void Scale(float scale)
	{
		_zoomScale = Mathf.Clamp(scale, 0.5f, ZoomMaxRatio);
		RefreshMapScale();
	}

	private void RefreshMapScale()
	{
		_mapTexture.transform.localScale = Vector3.one * CurrentZoomScale;
		if (this.ScaleChanged != null)
		{
			this.ScaleChanged();
		}
	}

	public void Zoom(float zoomDelta, Vector2 center)
	{
		float zoomScale = _zoomScale;
		float scale = _zoomScale * (1f + zoomDelta);
		Scale(scale);
		float num = _zoomScale / zoomScale;
		Vector2 vector = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
		Vector2 vector2 = center - vector;
		Focus((Offset - vector2) * num + vector2);
	}

	private void ApplyTerrainMeta()
	{
		if (!(_terrainId == GameManager.Region.TerrainId))
		{
			_terrainId = GameManager.Region.TerrainId;
			MapSize = TerrainMeta.TileCount;
			float num = (float)MapSize / 512f;
			ZoomMidRatio = 1f * num;
			ZoomMaxRatio = 3f * num;
			_zoomScale = ZoomMidRatio;
			RecalcMapTextureSize();
			_dirtyChunk.Resize(TerrainMeta.ChunkCount, TerrainMeta.ChunkCount);
			_worldMaterial.SetTexture("_CoverTex", null);
			_fowCover.Initialize(MapSize, delegate(RenderTexture tex)
			{
				_worldMaterial.SetTexture("_CoverTex", tex);
				_mapTexture.RemoveFromPanel();
			});
			Singleton<GameManager>.Instance().AddOnReady(delegate
			{
				Connections.Frontend.Send(default(GetDefoggedChunks));
			});
			LoadBiomes();
		}
	}

	private void LoadBiomes()
	{
		_nextReloadTime = -1.0;
		_mapTexture.gameObject.SetActive(value: false);
		string terrainId = _terrainId;
		MapSystem.RequestBiomes(terrainId, delegate(byte[] bytes)
		{
			if (!(terrainId != _terrainId))
			{
				_wholeBiomes = bytes;
				Texture2D texture2D = _worldMaterial.mainTexture as Texture2D;
				if (texture2D == null || texture2D.width != MapSize || texture2D.height != MapSize)
				{
					texture2D = new Texture2D(MapSize, MapSize, TextureFormat.ARGB32, mipmap: false)
					{
						wrapMode = TextureWrapMode.Clamp,
						filterMode = FilterMode.Bilinear
					};
				}
				Color32[] array = new Color32[MapSize * MapSize];
				for (int i = 0; i < _wholeBiomes.Length; i++)
				{
					ref Color32 reference = ref array[i];
					reference = GetBiomeColor(_wholeBiomes[i]);
				}
				texture2D.SetPixels32(array);
				texture2D.Apply();
				_worldMaterial.mainTexture = texture2D;
				_mapTexture.material = _worldMaterial;
				_mapTexture.gameObject.SetActive(value: true);
			}
		}, delegate
		{
			if (!(terrainId != _terrainId))
			{
				_wholeBiomes = null;
				_nextReloadTime = Time.time + 30f;
			}
		});
	}

	private void LateUpdate()
	{
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if (!(localPlayer == null) && TerrainBase.IsPlayerInitialized)
		{
			if (_nextReloadTime >= 0.0 && _nextReloadTime <= (double)Time.time)
			{
				LoadBiomes();
			}
			Vector2 playerTile = (_revealCenter = Durango.Terrain.Util.ClientPositionToTilePosition(localPlayer.CurrentPosition));
			UpdateMapUV(playerTile);
			UpdatePosLabel(playerTile);
			UpdateChunkData();
		}
	}

	private void OnRender_MapTexture(Material mat)
	{
		if (!(mat == null) && (!(mat == _prevMapMaterial) || !(_revealCenter == _prevRevealCenter)))
		{
			Vector4 value = default(Vector4);
			value.x = (_revealCenter.x - 10f) / (float)MapSize;
			value.y = (_revealCenter.y - 10f) / (float)MapSize;
			value.z = (_revealCenter.x + 10f) / (float)MapSize;
			value.w = (_revealCenter.y + 10f) / (float)MapSize;
			mat.SetVector("_RevealRange", value);
			_prevMapMaterial = mat;
			_prevRevealCenter = _revealCenter;
		}
	}

	public void Attach(bool worldMapMode, Transform parent)
	{
		IsWorldMapMode = worldMapMode;
		RecalcMapTextureSize();
		base.transform.parent = parent;
		base.transform.localPosition = Vector3.zero;
		base.transform.localScale = Vector3.one;
		NGUITools.MarkParentAsChanged(base.gameObject);
		RefreshMapPosition();
		RefreshMapScale();
		if (this.Attached != null)
		{
			this.Attached();
		}
	}

	private void RecalcMapTextureSize()
	{
		if (IsWorldMapMode)
		{
			MapNGUISize = 1280;
			_mapTexture.width = MapNGUISize;
			_mapTexture.height = MapNGUISize;
			Rect uvRect = default(Rect);
			uvRect.width = 1f;
			uvRect.height = 1f;
			_mapTexture.uvRect = uvRect;
		}
		else
		{
			int num = TerrainMeta.TileCount * 2;
			MapNGUISize = (int)((float)num / 0.5f);
			_mapTexture.width = 300;
			_mapTexture.height = 300;
			Rect uvRect2 = default(Rect);
			float height = (uvRect2.width = 300f / (float)MapNGUISize);
			uvRect2.height = height;
			_mapTexture.uvRect = uvRect2;
		}
	}

	public Vector2 TileToMapPosition(Vector2 tilePos, bool applyScale = true)
	{
		float num = (float)TerrainMeta.TileCount * 0.5f;
		Vector2 vector = new Vector2(num, num);
		Vector2 vector2 = (tilePos - vector) / MapSize * MapNGUISize;
		if (applyScale)
		{
			vector2 *= CurrentZoomScale;
		}
		return new Vector2(vector2.x * _cos45 - vector2.y * _cos45, vector2.x * _cos45 + vector2.y * _cos45);
	}

	private void UpdateMapUV(Vector2 playerTile)
	{
		if (!IsWorldMapMode)
		{
			FocusToTilePostion(playerTile);
			Rect uvRect = _mapTexture.uvRect;
			uvRect.center = playerTile / MapSize;
			_mapTexture.uvRect = uvRect;
		}
	}

	public void HidePosLabel()
	{
		_posLabel.alpha = 0f;
	}

	public void ShowPosLabel()
	{
		_posLabel.alpha = 1f;
	}

	private void SetPosLabelBG(bool worldmapMode)
	{
		if (!(_worldmapPosBG == null) && !(_minimapPosBG == null))
		{
			_worldmapPosBG.gameObject.SetActive(worldmapMode);
			_minimapPosBG.gameObject.SetActive(!worldmapMode);
		}
	}

	private void UpdatePosLabel(Vector2 playerTile)
	{
		Vector2 vector = MapPositionParser.PositionToHumaneTile(Durango.Terrain.Util.TilePositionToWorldPosition(playerTile));
		Point2 point = new Point2(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
		if (HumanePosition != point)
		{
			_posLabel.text = string.Format("[{2}]X[-] [{3}]{0}[-] [{2}]Y[-] [{3}]{1}[-]", point.x, point.y, NGUIText.EncodeColor(_posVariableColor), NGUIText.EncodeColor(_posValueColor));
			HumanePosition = point;
		}
		_posLabel.transform.localPosition = TileToMapPosition(playerTile) + ((!IsWorldMapMode) ? _minimapPosLabelOffset : _worldmapPosLabelOffset);
	}

	private void UpdateChunkData()
	{
		if (!_hasDirtyChunk || _wholeBiomes == null)
		{
			return;
		}
		Texture2D texture2D = _worldMaterial.mainTexture as Texture2D;
		if (texture2D == null)
		{
			return;
		}
		for (int i = 0; i < _dirtyChunk.Width; i++)
		{
			for (int j = 0; j < _dirtyChunk.Height; j++)
			{
				if (!_dirtyChunk.Get(i, j))
				{
					continue;
				}
				int num = i * 16;
				int num2 = j * 16;
				for (int k = 0; k < 16; k++)
				{
					for (int l = 0; l < 16; l++)
					{
						int num3 = num + k;
						int num4 = num2 + l;
						int num5 = num4 * MapSize + num3;
						byte biome = (byte)((num5 >= _wholeBiomes.Length) ? 12 : _wholeBiomes[num5]);
						Point2 point = new Point2(k, l);
						Point2 tile = point + new Point2(i * 16, j * 16);
						Color tileColor = GetTileColor(biome, tile);
						_chunkColorBuffer[l * 16 + k] = tileColor;
					}
				}
				texture2D.SetPixels(i * 16, j * 16, 16, 16, _chunkColorBuffer);
				_dirtyChunk.Set(i, j, value: false);
			}
		}
		texture2D.Apply();
		_hasDirtyChunk = false;
	}

	public Color GetTileColor(byte biome, Point2 tile)
	{
		Color biomeColor = GetBiomeColor(biome);
		EstateInfo estateInfo = EstateSystem.GetEstateInfo(tile);
		if (estateInfo == null || estateInfo.License.Type == OwnerType.System)
		{
			return biomeColor;
		}
		Color color = Color.white;
		bool flag = estateInfo.IsLocalPlayers();
		switch (estateInfo.License.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			color = ((!flag) ? PresetColor.EstateArea : PresetColor.PlayerEstateArea);
			break;
		case OwnerType.ClanEstate:
			color = ((!flag) ? PresetColor.ClanTerritory : PresetColor.PlayerClanTerritory);
			break;
		case OwnerType.ClanWarphole:
			if (estateInfo.License.ProtectedUntil.HasValue)
			{
				double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
				color = ((!(predictedServerTime <= estateInfo.License.ProtectedUntil.Value)) ? PresetColor.EnemyEstateArea : ((!flag) ? PresetColor.ClanTerritory : PresetColor.PlayerClanTerritory));
			}
			break;
		}
		return biomeColor * color;
	}

	public Color GetBiomeColor(byte biome)
	{
		if (Durango.Terrain.Util.IsCollidableMasked(biome))
		{
			return _landmarkColor;
		}
		if (Durango.Terrain.Util.IsNotPlantableMasked(biome))
		{
			return _scoopColor;
		}
		return (_biomeColors.Length > biome) ? _biomeColors[biome] : Color.black;
	}

	public void RefreshChunk(int x, int y)
	{
		_dirtyChunk.Set(x, y, value: true);
		_hasDirtyChunk = true;
	}

	public Vector2 ScreenPosToTilePos(Vector2 screenPos)
	{
		Vector2 vector = NGUIMath.ScreenToParentPixels(screenPos, _mapTexture.transform) / _mapTexture.transform.localScale.x;
		Vector2 vector2 = default(Vector2);
		vector2.x = vector.x * _cos45 + vector.y * _sin45;
		vector2.y = (0f - vector.x) * _sin45 + vector.y * _cos45;
		Vector2 result = Vector3.zero;
		result.x = (vector2.x / (float)MapNGUISize + 0.5f) * (float)MapSize;
		result.y = (vector2.y / (float)MapNGUISize + 0.5f) * (float)MapSize;
		return result;
	}

	public void ZoomOut(bool toPlayer)
	{
		Scale(0.5f);
		Vector2 tilePosition;
		if (toPlayer)
		{
			Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
			tilePosition = Durango.Terrain.Util.ClientPositionToTilePosition(currentPosition);
		}
		else
		{
			int num = TerrainMeta.TileCount / 2;
			tilePosition = new Vector2(num, num);
		}
		FocusToTilePostion(tilePosition);
	}

	[Conditional("UNITY_EDITOR")]
	[ExposedInEditor(null)]
	private void SaveToTexture()
	{
		Texture2D texture2D = _worldMaterial.mainTexture as Texture2D;
		Color[] pixels = texture2D.GetPixels();
		Color biomeColor = GetBiomeColor(12);
		for (int i = 0; i < pixels.Length; i++)
		{
			if (pixels[i] == biomeColor)
			{
				pixels[i].a = 0f;
			}
		}
		Texture2D texture2D2 = new Texture2D(texture2D.width, texture2D.height, TextureFormat.ARGB32, mipmap: false);
		texture2D2.SetPixels(pixels);
		texture2D2.Apply();
		File.WriteAllBytes(GameManager.Region.TemplateId + ".png", texture2D2.EncodeToPNG());
	}
}
