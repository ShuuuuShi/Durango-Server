using Durango.Terrain;
using Durango.UI.InGame;
using Durango.Utils;
using UnityEngine;

namespace Durango.Development;

public class TestGrid : MonoBehaviour
{
	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private GameObject _parent;

	[SerializeField]
	private Color _tileColor;

	[SerializeField]
	private Color _gridColor;

	[SerializeField]
	private int _gridSize;

	[SerializeField]
	private Color _borderColor;

	[SerializeField]
	private int _borderSize;

	private float _hideAt;

	private bool _isInit;

	private void Awake()
	{
		HideGrid();
	}

	private void OnEnable()
	{
		if (Singleton<TerrainBase>.HasInstance())
		{
			Init();
			_parent.SetActive(value: true);
			_hideAt = ((!(_hideAt > Time.time)) ? 0f : _hideAt);
			Singleton<TerrainBase>.Instance().LoadingChunksFinished += OnChunkLoadFinish;
			if (!Singleton<TerrainBase>.Instance().IsChunkLoading)
			{
				OnChunkLoadFinish();
			}
		}
		else
		{
			HideGrid();
		}
	}

	private void OnDisable()
	{
		_parent.SetActive(value: false);
		if (Singleton<TerrainBase>.HasInstance())
		{
			Singleton<TerrainBase>.Instance().LoadingChunksFinished -= OnChunkLoadFinish;
		}
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_texture.mainTexture = Texture2D.whiteTexture;
			_texture.fillGeometry = false;
			_texture.SetDimensions(9600, 9600);
			MakeGridTexture();
		}
	}

	private void MakeGridTexture()
	{
		UIGeometry geometry = _texture.geometry;
		geometry.Clear();
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Vector3 vector = new Vector3(i * 3200, j * 3200);
				Point2 point = new Point2(3200, 3200);
				Color tileColor = _tileColor;
				if (tileColor.a > 0f)
				{
					DrawQuad(geometry, vector, point.ToVector2(), tileColor);
				}
				for (int k = 1; k < 16; k++)
				{
					Vector3 pos = vector + Vector3.right * ((float)(k * 200) - (float)_gridSize * 0.5f);
					DrawQuad(geometry, pos, new Vector2(_gridSize, point.y), _gridColor);
				}
				for (int l = 1; l < 16; l++)
				{
					Vector3 pos2 = vector + Vector3.up * ((float)(l * 200) - (float)_gridSize * 0.5f);
					DrawQuad(geometry, pos2, new Vector2(point.x, _gridSize), _gridColor);
				}
				DrawQuad(geometry, vector - new Vector3(1f, 1f) * _borderSize * 0.5f, new Vector2(_borderSize, point.y + _borderSize), _borderColor);
				DrawQuad(geometry, vector - new Vector3(1f, 1f) * _borderSize * 0.5f + Vector3.right * point.x, new Vector2(_borderSize, point.y + _borderSize), _borderColor);
				DrawQuad(geometry, vector - new Vector3(1f, 1f) * _borderSize * 0.5f, new Vector2(point.x + _borderSize, _borderSize), _borderColor);
				DrawQuad(geometry, vector - new Vector3(1f, 1f) * _borderSize * 0.5f + Vector3.up * point.y, new Vector2(point.x + _borderSize, _borderSize), _borderColor);
			}
		}
		_texture.MarkAsChanged();
	}

	private void DrawQuad(UIGeometry geometry, Vector3 pos, Vector2 size, Color color)
	{
		if (color.a > 0f)
		{
			geometry.verts.Add(pos);
			geometry.verts.Add(pos + Vector3.right * size.x);
			geometry.verts.Add(pos + Vector3.right * size.x + Vector3.up * size.y);
			geometry.verts.Add(pos + Vector3.up * size.y);
			geometry.uvs.Add(new Vector2(0f, 0f));
			geometry.uvs.Add(new Vector2(0f, 1f));
			geometry.uvs.Add(new Vector2(1f, 1f));
			geometry.uvs.Add(new Vector2(1f, 0f));
			geometry.cols.Add(color);
			geometry.cols.Add(color);
			geometry.cols.Add(color);
			geometry.cols.Add(color);
		}
	}

	private void OnChunkLoadFinish()
	{
		Point2 tileOffset = Singleton<GridAreaViewer>.Instance().GetTileOffset();
		_parent.transform.position = Util.TilePositionToClientPosition(tileOffset);
	}

	private void Update()
	{
		if (0f < _hideAt && _hideAt < Time.time)
		{
			HideGrid();
		}
	}

	public void ShowGrid(float duration = 0f)
	{
		if (!base.enabled)
		{
			_hideAt = ((!(duration > 0f)) ? 0f : (Time.time + duration));
			base.enabled = true;
		}
	}

	public void HideGrid()
	{
		if (base.enabled)
		{
			base.enabled = false;
		}
	}
}
