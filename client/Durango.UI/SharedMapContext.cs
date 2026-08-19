using System;
using Durango.Network;
using Durango.Player;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class SharedMapContext : MonoBehaviour
{
	[SerializeField]
	private Transform _container;

	[SerializeField]
	private Material _worldMaterial;

	[SerializeField]
	private UITexture _mapTexture;

	[SerializeField]
	private FogOfWarCover _fowCover;

	[SerializeField]
	private BalloonContainer _balloonContainer;

	private readonly float _sin45 = Mathf.Sin((float)Math.PI / 4f);

	private readonly float _cos45 = Mathf.Cos((float)Math.PI / 4f);

	private readonly BitArray2D _visibleGrid = new BitArray2D();

	private Vector2 _offset = Vector2.zero;

	private string _terrainId;

	private string _regionId;

	private float _zoomMidRatio;

	private float _zoomMaxRatio;

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

	public float ZoomScale { get; private set; }

	public Vector2 Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			if (_offset != value)
			{
				_offset = value;
				float num = _sin45 * 1280f * ZoomScale;
				float num2 = Mathf.Max(0f, num - (float)UIManager.ScreenWidth * 0.5f) + (float)UIManager.ScreenWidth * 0.1f;
				float num3 = Mathf.Max(0f, num - (float)UIManager.ScreenHeight * 0.5f) + (float)UIManager.ScreenHeight * 0.1f;
				_offset.x = Mathf.Clamp(_offset.x, 0f - num2, num2);
				_offset.y = Mathf.Clamp(_offset.y, 0f - num3, num3);
				_container.localPosition = _offset;
			}
		}
	}

	public int MapSize { get; private set; }

	public event Action ZoomChanged;

	private void Awake()
	{
		_worldMaterial = UnityEngine.Object.Instantiate(_worldMaterial);
		_worldMaterial.SetTexture("_RevealTex", null);
		_mapTexture.width = 1280;
		_mapTexture.height = 1280;
		Rect uvRect = default(Rect);
		uvRect.width = 1f;
		uvRect.height = 1f;
		_mapTexture.uvRect = uvRect;
		_mapTexture.gameObject.SetActive(value: false);
		MapSize = 512;
		_balloonContainer.TileToMapPosition = TileToMapPosition;
		_balloonContainer.TileToHumanePosition = (Vector2 tilePos) => MapPositionParser.PositionToHumaneTile(Util.TilePositionToWorldPosition(tilePos), MapSize * 200);
	}

	private void Start()
	{
		Zoom(0f, Vector2.zero);
	}

	public void Load(string regionId, [NotNull] Action loaded)
	{
		_balloonContainer.RemoveAnnounceBalloons(AnnounceType.SharePinPoint);
		if (_regionId == regionId)
		{
			loaded();
			return;
		}
		_mapTexture.gameObject.SetActive(value: false);
		_regionId = regionId;
		Connections.Frontend.Send(new GetRegionMapInfo
		{
			RegionId = regionId
		}).On(delegate(RegionMapInfo msg, PacketHeader header)
		{
			if (!(_regionId != msg.RegionId))
			{
				_terrainId = msg.TerrainId;
				MapSize = msg.TileCount.x;
				float num = (float)MapSize / 512f;
				ZoomMidRatio = 1f * num;
				ZoomMaxRatio = 3f * num;
				int num2 = MapSize / 16;
				_visibleGrid.Resize(num2, num2);
				_worldMaterial.SetTexture("_CoverTex", null);
				_fowCover.Initialize(MapSize, delegate(RenderTexture tex)
				{
					_worldMaterial.SetTexture("_CoverTex", tex);
					_mapTexture.RemoveFromPanel();
				});
				DefoggedChunks defoggedChunks = msg.DefoggedChunks;
				for (int i = 0; i < defoggedChunks.Chunks.Length; i++)
				{
					int x = defoggedChunks.Chunks[i].x;
					int y = defoggedChunks.Chunks[i].y;
					_visibleGrid.Set(x, y, value: true);
				}
				_fowCover.SetDefoggedChunks(_visibleGrid);
				LoadBiomes();
				loaded();
			}
		});
	}

	private void LoadBiomes()
	{
		string terrainId = _terrainId;
		MapSystem.RequestBiomes(terrainId, delegate(byte[] bytes)
		{
			if (!(terrainId != _terrainId))
			{
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
				for (int i = 0; i < bytes.Length; i++)
				{
					ref Color32 reference = ref array[i];
					reference = Singleton<MapContext>.Instance().GetBiomeColor(bytes[i]);
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
				_regionId = string.Empty;
			}
		});
	}

	public void SetPinInfo(Durango.Player.PlayerInfo info, Vector2 pinPoint)
	{
		_balloonContainer.AddAnnounceBalloon(AnnounceType.SharePinPoint, pinPoint, info);
		RefreshPinPosition();
	}

	public void FocusToTilePostion(Vector2 tilePos)
	{
		int num = MapSize / 2;
		tilePos.x -= num;
		tilePos.y -= num;
		Vector2 vector = tilePos / MapSize * 1280f;
		float num2 = Mathf.Sin((float)Math.PI / 4f);
		float num3 = Mathf.Cos((float)Math.PI / 4f);
		Vector2 vector2 = new Vector2(vector.x * num3 - vector.y * num2, vector.x * num2 + vector.y * num3);
		vector2 *= ZoomScale;
		Offset = -vector2;
	}

	private void RefreshPinPosition()
	{
		_balloonContainer.UpdatePosition();
	}

	public void Zoom(float zoomDelta, Vector2 center)
	{
		Vector2 vector = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		Vector2 vector2 = center - vector;
		float zoomScale = ZoomScale;
		ZoomScale *= 1f + zoomDelta;
		ZoomScale = Mathf.Clamp(ZoomScale, 0.5f, ZoomMaxRatio);
		float num = ((!(zoomScale > 0f)) ? 1f : (ZoomScale / zoomScale));
		Offset = (Offset - vector2) * num + vector2;
		_mapTexture.transform.localScale = Vector3.one * ZoomScale;
		RefreshPinPosition();
		if (this.ZoomChanged != null)
		{
			this.ZoomChanged();
		}
	}

	private Vector2 TileToMapPosition(Vector2 tilePos)
	{
		float num = (float)MapSize * 0.5f;
		Vector2 vector = new Vector2(num, num);
		Vector2 vector2 = (tilePos - vector) / MapSize * 1280f;
		vector2 *= _mapTexture.transform.localScale.x;
		return new Vector2(vector2.x * _cos45 - vector2.y * _cos45, vector2.x * _cos45 + vector2.y * _cos45);
	}
}
