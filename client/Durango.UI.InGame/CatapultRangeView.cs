using System;
using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.InGame;

public class CatapultRangeView : Singleton<CatapultRangeView>
{
	private const int VertexDevideCount = 90;

	[SerializeField]
	private float _borderSize;

	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private float _borderAlpha = 1f;

	[SerializeField]
	private float _backgroundAlpha = 0.1f;

	[SerializeField]
	private float _waveSpeed;

	[SerializeField]
	private AnimationCurve _waveSpeedCurve;

	[SerializeField]
	private AnimationCurve _waveAlphaCurve;

	[SerializeField]
	private float _waveDelay;

	[SerializeField]
	private AnimationCurve _waveAlpha;

	[SerializeField]
	private float _waveSize;

	private Mesh _mesh;

	private Texture2D _waveTexture;

	private Mesh _waveMesh;

	private float _outterRadius;

	private float _innerRadius;

	private float _waveRatio;

	private float _waveStartAt;

	private float _waveEndAt;

	private float _nextWaveStartAt;

	private bool _waveEnabled;

	private bool _isChanged;

	private bool _isWaveChanged;

	private readonly List<Vector3> _verts = new List<Vector3>();

	private readonly List<Color> _colors = new List<Color>();

	private readonly List<int> _tris = new List<int>();

	private readonly List<Vector3> _waveVerts = new List<Vector3>();

	private readonly List<Color> _waveCols = new List<Color>();

	private readonly List<Vector2> _waveUvs = new List<Vector2>();

	private readonly List<int> _waveTris = new List<int>();

	protected override void OnAwake()
	{
		Hide();
	}

	private void Start()
	{
		_mesh = MakeMesh(base.gameObject, Texture2D.whiteTexture);
		if (_waveAlpha.length > 0)
		{
			_waveTexture = new Texture2D((int)(_waveSize * 0.5f), 1, TextureFormat.RGBA32, mipmap: false);
			SetWaveTexture(_waveTexture);
		}
		else
		{
			_waveTexture = Texture2D.whiteTexture;
		}
		_waveMesh = MakeMesh(base.gameObject.AddChild(), _waveTexture);
	}

	private void Update()
	{
		float time = Time.time;
		if (time > _waveStartAt && time < _waveEndAt)
		{
			_waveRatio = (Time.time - _waveStartAt) / (_waveEndAt - _waveStartAt);
			if (_waveRatio >= 1f)
			{
				_waveRatio = 0f;
				_nextWaveStartAt = Time.time + _waveDelay;
				_waveStartAt = 0f;
				_waveEndAt = 0f;
			}
			_isWaveChanged = true;
		}
		else if (_waveEnabled && Time.time > _nextWaveStartAt)
		{
			_waveRatio = 0f;
			_waveStartAt = Time.time;
			_waveEndAt = _waveStartAt + (_outterRadius - _innerRadius + _waveSize) / _waveSpeed;
			_isWaveChanged = true;
		}
		if (_isChanged)
		{
			Refresh();
		}
		if (_isWaveChanged)
		{
			RefreshWave();
		}
	}

	[ExposedInEditor(null)]
	private void UpdateWaveTexture()
	{
		if (Application.isPlaying && _waveAlpha.length > 0)
		{
			SetWaveTexture(_waveTexture);
		}
	}

	private void SetWaveTexture(Texture2D texture)
	{
		float time = _waveAlpha.keys[_waveAlpha.length - 1].time;
		for (int i = 0; i < texture.width; i++)
		{
			float num = ((float)i + 0.5f) / (float)texture.width;
			Color white = Color.white;
			white.a = _waveAlpha.Evaluate(num * time);
			texture.SetPixel(i, 1, white);
		}
		texture.Apply();
	}

	private Mesh MakeMesh(GameObject obj, Texture2D tex)
	{
		Mesh mesh = new Mesh();
		MeshFilter meshFilter = obj.AddMissingComponent<MeshFilter>();
		MeshRenderer meshRenderer = obj.AddMissingComponent<MeshRenderer>();
		Material material = new Material(NGUITools.defaultShader);
		material.mainTexture = tex;
		meshRenderer.sharedMaterial = material;
		meshFilter.sharedMesh = mesh;
		return mesh;
	}

	private void Refresh()
	{
		_isChanged = false;
		_verts.Clear();
		_colors.Clear();
		_tris.Clear();
		int num = 0;
		while (true)
		{
			if (num > 0)
			{
				int num2 = (num - 1) * 6;
				int num3 = num % 90 * 6;
				_tris.Add(num2);
				_tris.Add(num3);
				_tris.Add(num2 + 1);
				_tris.Add(num3);
				_tris.Add(num3 + 1);
				_tris.Add(num2 + 1);
				_tris.Add(num2 + 2);
				_tris.Add(num3 + 2);
				_tris.Add(num2 + 3);
				_tris.Add(num3 + 2);
				_tris.Add(num3 + 3);
				_tris.Add(num2 + 3);
				_tris.Add(num2 + 4);
				_tris.Add(num3 + 4);
				_tris.Add(num2 + 5);
				_tris.Add(num3 + 4);
				_tris.Add(num3 + 5);
				_tris.Add(num2 + 5);
			}
			if (num >= 90)
			{
				break;
			}
			float f = (float)Math.PI * 2f * ((float)num / 90f);
			Vector3 vector = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
			Vector3 item = vector * _innerRadius;
			Vector3 item2 = vector * (_innerRadius + _borderSize);
			Vector3 item3 = vector * (_outterRadius - _borderSize);
			Vector3 item4 = vector * _outterRadius;
			_verts.Add(item);
			_verts.Add(item2);
			_verts.Add(item2);
			_verts.Add(item3);
			_verts.Add(item3);
			_verts.Add(item4);
			_colors.Add(GetColor(_borderAlpha));
			_colors.Add(GetColor(_borderAlpha));
			_colors.Add(GetColor(_backgroundAlpha));
			_colors.Add(GetColor(_backgroundAlpha));
			_colors.Add(GetColor(_borderAlpha));
			_colors.Add(GetColor(_borderAlpha));
			num++;
		}
		_mesh.SetVertices(_verts);
		_mesh.SetColors(_colors);
		_mesh.SetTriangles(_tris, 0);
	}

	private void RefreshWave()
	{
		_isWaveChanged = false;
		if (_waveRatio <= 0f)
		{
			_waveMesh.Clear();
			return;
		}
		_waveVerts.Clear();
		_waveCols.Clear();
		_waveUvs.Clear();
		_waveTris.Clear();
		float num = ((_waveSpeedCurve.length <= 0) ? 0f : _waveSpeedCurve.keys[_waveSpeedCurve.length - 1].time);
		float num2 = Mathf.Lerp(_innerRadius, _outterRadius + _waveSize, (!(num > 0f)) ? _waveRatio : _waveSpeedCurve.Evaluate(num * _waveRatio));
		float num3 = ((_waveAlphaCurve.length <= 0) ? 0f : _waveAlphaCurve.keys[_waveAlphaCurve.length - 1].time);
		float a = ((!(num3 > 0f)) ? 1f : _waveAlphaCurve.Evaluate(num3 * _waveRatio));
		int num4 = 0;
		while (true)
		{
			if (num4 > 0)
			{
				int num5 = (num4 - 1) * 2;
				int num6 = num4 % 90 * 2;
				_waveTris.Add(num5);
				_waveTris.Add(num6);
				_waveTris.Add(num5 + 1);
				_waveTris.Add(num6);
				_waveTris.Add(num6 + 1);
				_waveTris.Add(num5 + 1);
			}
			if (num4 >= 90)
			{
				break;
			}
			float f = (float)Math.PI * 2f * ((float)num4 / 90f);
			Vector3 vector = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
			float num7 = num2 - _waveSize;
			float num8 = num2;
			float num9 = ((!(num7 > _innerRadius)) ? ((_innerRadius - num7) / _waveSize) : 0f);
			float num10 = ((!(num8 < _outterRadius)) ? (1f - (num8 - _outterRadius) / _waveSize) : 1f);
			float alpha = Mathf.Min(a, num10 - num9);
			Vector3 item = vector * Mathf.Max(num7, _innerRadius);
			Vector3 item2 = vector * Mathf.Min(num8, _outterRadius);
			_waveVerts.Add(item);
			_waveVerts.Add(item2);
			_waveCols.Add(GetColor(alpha));
			_waveCols.Add(GetColor(alpha));
			_waveUvs.Add(new Vector2(num9, 0f));
			_waveUvs.Add(new Vector2(num10, 0f));
			num4++;
		}
		_waveMesh.SetVertices(_waveVerts);
		_waveMesh.SetColors(_waveCols);
		_waveMesh.SetUVs(0, _waveUvs);
		_waveMesh.SetTriangles(_waveTris, 0);
	}

	public void Show(Vector3 position, float inner, float outter)
	{
		_innerRadius = inner;
		_outterRadius = outter;
		base.transform.position = position;
		_isChanged = true;
		_isWaveChanged = true;
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		_waveRatio = 0f;
		_waveStartAt = 0f;
		_waveEndAt = 0f;
		_waveEnabled = false;
		base.gameObject.SetActive(value: false);
	}

	public void ShowWave(bool show)
	{
		if (_waveEnabled != show)
		{
			_waveEnabled = show;
			if (show)
			{
				_waveRatio = 0f;
				_nextWaveStartAt = 0f;
				_waveStartAt = 0f;
				_waveEndAt = 0f;
				_isWaveChanged = true;
			}
		}
	}

	private Color GetColor(float alpha)
	{
		return new Color(_color.r, _color.g, _color.b, alpha);
	}
}
