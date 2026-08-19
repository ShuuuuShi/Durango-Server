using System;
using System.Collections.Generic;
using Durango.Development;
using UnityEngine;

namespace Durango.UI.InGame;

public class FillBorderAlert : MonoBehaviour
{
	public Action<FillBorderAlert> Finished;

	private const float FadeIn = 0.3f;

	private const float FadeOut = 0.3f;

	private const float BorderSize = 40f;

	private Mesh _bgMesh;

	private Mesh _fillMesh;

	private readonly List<Vector3> _bgVerts = new List<Vector3>();

	private readonly List<Color> _bgCols = new List<Color>();

	private readonly List<int> _bgTris = new List<int>();

	private readonly List<Vector3> _fillVerts = new List<Vector3>();

	private readonly List<Vector2> _fillUvs = new List<Vector2>();

	private readonly List<Color> _fillCols = new List<Color>();

	private readonly List<int> _fillTris = new List<int>();

	private readonly List<Vector3> _outter = new List<Vector3>();

	private readonly List<Vector3> _inner = new List<Vector3>();

	private Color _bgColor;

	private Color _fillColor;

	private float _startAt;

	private float _finishAt;

	private float _showAt;

	private float _hideAt;

	private const int PointCount = 2;

	private bool _isInit;

	public int Id { get; set; }

	public void Init(Color bgColor, Color borderColor, Texture2D fillTexture)
	{
		if (!_isInit)
		{
			_isInit = true;
			_bgMesh = new Mesh();
			MeshFilter meshFilter = base.gameObject.AddComponent<MeshFilter>();
			base.gameObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(NGUITools.defaultShader)
			{
				mainTexture = Texture2D.whiteTexture
			};
			meshFilter.sharedMesh = _bgMesh;
			_fillMesh = new Mesh();
			GameObject obj = base.gameObject.AddChild();
			MeshFilter meshFilter2 = obj.AddComponent<MeshFilter>();
			obj.AddComponent<MeshRenderer>().sharedMaterial = new Material(NGUITools.defaultShader)
			{
				mainTexture = fillTexture
			};
			meshFilter2.sharedMesh = _fillMesh;
			_bgColor = bgColor;
			_fillColor = borderColor;
		}
	}

	public void SetArc(Vector3 position, float radius, float startAngle, float endAngle)
	{
		MakeRadiusMesh(radius, startAngle, endAngle);
		base.transform.position = position;
	}

	public void SetRect(Vector3 position, float width, float height, float angle)
	{
		MakeRectMesh(width, height, angle);
		base.transform.position = position;
	}

	public void Show(float startAt, float finishAt, float showAt, float hideAt)
	{
		base.gameObject.SetActive(value: true);
		_startAt = startAt;
		_finishAt = finishAt;
		_showAt = showAt;
		_hideAt = hideAt;
		Update();
		if (!Application.isPlaying)
		{
			EditorUpdateLoop.Play(this);
		}
	}

	public void Stop(float delay)
	{
		if (delay > 0f)
		{
			float num = AreaOfEffectVisualizer.Now();
			_hideAt = Mathf.Min(num + delay, _hideAt);
			return;
		}
		base.gameObject.SetActive(value: false);
		if (!Application.isPlaying)
		{
			OnDisable();
		}
	}

	private void OnDisable()
	{
		if (Finished != null)
		{
			Finished(this);
		}
	}

	private void Update()
	{
		if (_isInit)
		{
			if (AreaOfEffectVisualizer.Now() > _hideAt + 0.3f)
			{
				Stop(0f);
				return;
			}
			UpdateProgress();
			UpdateAlpha();
			_bgMesh.SetColors(_bgCols);
			_fillMesh.SetColors(_fillCols);
		}
	}

	private void UpdateProgress()
	{
		float num = AreaOfEffectVisualizer.Now();
		Color fillColor = _fillColor;
		fillColor.a = 0f;
		for (int i = 0; i < _fillCols.Count; i++)
		{
			_fillCols[i] = fillColor;
		}
		float num2 = (num - _startAt) / (_finishAt - _startAt);
		float num3 = num / 5f % 1f;
		for (int j = 0; j < 2; j++)
		{
			float num4 = (float)_inner.Count * (num3 + (float)j / 2f);
			float num5 = (float)_inner.Count * num2 / 2f;
			int k = Mathf.CeilToInt(num4);
			for (int num6 = Mathf.CeilToInt(num4 + num5); k < num6; k++)
			{
				int num7 = k % _inner.Count;
				_fillCols[num7 * 2] = _fillColor;
				_fillCols[num7 * 2 + 1] = _fillColor;
			}
		}
	}

	private void UpdateAlpha()
	{
		float num = 1f;
		float num2 = AreaOfEffectVisualizer.Now();
		if (num2 > _hideAt)
		{
			num = 1f - (num2 - _hideAt) / 0.3f;
		}
		else if (num2 < _showAt + 0.3f)
		{
			num = (num2 - _showAt) / 0.3f;
		}
		for (int i = 0; i < _bgCols.Count; i++)
		{
			Color bgColor = _bgColor;
			bgColor.a *= num;
			_bgCols[i] = bgColor;
		}
		for (int j = 0; j < _fillCols.Count; j++)
		{
			Color value = _fillCols[j];
			value.a *= num;
			_fillCols[j] = value;
		}
	}

	private void BeginMakeMesh()
	{
		_inner.Clear();
		_outter.Clear();
		_bgMesh.Clear();
		_fillMesh.Clear();
		_bgVerts.Clear();
		_bgCols.Clear();
		_bgTris.Clear();
		_fillVerts.Clear();
		_fillCols.Clear();
		_fillUvs.Clear();
		_fillTris.Clear();
	}

	private void EndMakeMesh()
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < _outter.Count; i++)
		{
			zero += _outter[i];
		}
		zero /= (float)_outter.Count;
		_bgVerts.Add(zero);
		_bgCols.Add(_bgColor);
		for (int j = 0; j < _outter.Count; j++)
		{
			_bgVerts.Add(_outter[j]);
			_bgCols.Add(_bgColor);
			_bgTris.Add(0);
			_bgTris.Add((j + 1) % _outter.Count + 1);
			_bgTris.Add(j + 1);
			_fillVerts.Add(_inner[j]);
			_fillVerts.Add(_outter[j]);
			_fillCols.Add(_fillColor);
			_fillCols.Add(_fillColor);
			_fillUvs.Add(new Vector2(0f, 0f));
			_fillUvs.Add(new Vector2(1f, 0f));
			int num = j * 2;
			int num2 = (j + 1) % _outter.Count * 2;
			_fillTris.Add(num);
			_fillTris.Add(num2);
			_fillTris.Add(num + 1);
			_fillTris.Add(num2);
			_fillTris.Add(num2 + 1);
			_fillTris.Add(num + 1);
		}
		_bgMesh.SetVertices(_bgVerts);
		_bgMesh.SetColors(_bgCols);
		_bgMesh.SetTriangles(_bgTris, 0);
		_fillMesh.SetVertices(_fillVerts);
		_fillMesh.SetColors(_fillCols);
		_fillMesh.SetUVs(0, _fillUvs);
		_fillMesh.SetTriangles(_fillTris, 0);
	}

	private void MakeRadiusMesh(float radius, float start, float end)
	{
		BeginMakeMesh();
		float num = Mathf.Abs(start - end);
		if (num < 360f)
		{
			float num2 = Mathf.Asin(40f / (radius - 40f));
			float num3 = (float)Math.PI / 180f * start + num2;
			float num4 = (float)Math.PI / 180f * end - num2;
			Vector2 vector = new Vector2(Mathf.Sin(num3), Mathf.Cos(num3)) * (radius - 40f);
			Vector2 vector2 = new Vector2(Mathf.Sin(num4), Mathf.Cos(num4)) * (radius - 40f);
			FillCorner(vector, start - 90f, num3 * 57.29578f, isInner: true);
			FillArc(radius, num3, num4);
			FillCorner(vector2, num4 * 57.29578f, end + 90f, isInner: true);
			if (num > 180f)
			{
				Vector2 vector3 = new Vector2(Mathf.Sin(end * ((float)Math.PI / 180f)), Mathf.Cos(end * ((float)Math.PI / 180f)));
				Vector2 vector4 = new Vector2(Mathf.Sin(start * ((float)Math.PI / 180f)), Mathf.Cos(start * ((float)Math.PI / 180f)));
				FillLine(vector3 * (radius - 40f), Vector2.zero, 1f);
				FillCorner(Vector2.zero, end - 90f, start + 90f, isInner: false);
				FillLine(Vector2.zero, vector4 * (radius - 40f), 1f);
			}
			else
			{
				float num5 = 40f / Mathf.Sin(num * ((float)Math.PI / 180f) * 0.5f);
				float f = Mathf.Lerp(start, end, 0.5f) * ((float)Math.PI / 180f);
				Vector2 vector5 = new Vector2(Mathf.Sin(f), Mathf.Cos(f)) * num5;
				FillLine(vector2, vector5, 0f);
				FillCorner(vector5, end + 90f, start + 270f, isInner: true);
				FillLine(vector5, vector, 0f);
			}
		}
		else
		{
			FillArc(radius, 0f, (float)Math.PI * 2f);
		}
		EndMakeMesh();
	}

	private void MakeRectMesh(float width, float height, float angle)
	{
		BeginMakeMesh();
		Vector2[] array = new Vector2[4];
		Vector2 vector = new Vector2(Mathf.Sin(angle * ((float)Math.PI / 180f)), Mathf.Cos(angle * ((float)Math.PI / 180f)));
		Vector2 vector2 = new Vector2(vector.y, 0f - vector.x);
		ref Vector2 reference = ref array[0];
		reference = -vector * 0.5f * width + -vector2 * 0.5f * height;
		ref Vector2 reference2 = ref array[1];
		reference2 = vector * 0.5f * width + -vector2 * 0.5f * height;
		ref Vector2 reference3 = ref array[2];
		reference3 = vector * 0.5f * width + vector2 * 0.5f * height;
		ref Vector2 reference4 = ref array[3];
		reference4 = -vector * 0.5f * width + vector2 * 0.5f * height;
		for (int i = 0; i < array.Length; i++)
		{
			Vector2 vector3 = array[i];
			Vector2 vector4 = array[(i + 1) % array.Length];
			Vector2 normalized = (vector4 - vector3).normalized;
			vector3 += normalized * 40f;
			vector4 -= normalized * 40f;
			FillLine(vector3, vector4, 1f);
			FillCorner(vector4 + new Vector2(normalized.y, 0f - normalized.x) * 40f, angle + (float)(i - 1) * 90f, angle + (float)i * 90f, isInner: true);
		}
		EndMakeMesh();
	}

	private void FillArc(float radius, float a1, float a2)
	{
		int num = Mathf.RoundToInt((float)Math.PI * 2f * radius / 10f * (Mathf.Abs(a1 - a2) / ((float)Math.PI * 2f)));
		for (int i = 0; i < num; i++)
		{
			float f = Mathf.Lerp(a1, a2, (float)i / (float)(num - 1));
			Vector3 vector = new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
			_inner.Add(vector * (radius - 40f));
			_outter.Add(vector * radius);
		}
	}

	private void FillLine(Vector2 p1, Vector2 p2, float pivotRatio)
	{
		Vector2 vector = p2 - p1;
		float magnitude = vector.magnitude;
		int num = Mathf.RoundToInt(magnitude / 10f);
		Vector2 vector2 = vector / magnitude;
		Vector2 vector3 = new Vector2(vector2.y, 0f - vector2.x);
		for (int i = 0; i < num; i++)
		{
			Vector2 vector4 = Vector2.Lerp(p1, p2, (float)i / (float)(num - 1));
			Vector2 vector5 = vector4 - vector3 * 40f * (1f - pivotRatio);
			Vector2 vector6 = vector4 + vector3 * 40f * pivotRatio;
			_outter.Add(new Vector3(vector5.x, 0f, vector5.y));
			_inner.Add(new Vector3(vector6.x, 0f, vector6.y));
		}
	}

	private void FillCorner(Vector2 pos, float start, float end, bool isInner)
	{
		int num = Mathf.RoundToInt(Mathf.Abs(end - start) / 20f);
		for (int i = 1; i < num; i++)
		{
			float f = Mathf.Lerp(start, end, (float)i / (float)num) * ((float)Math.PI / 180f);
			Vector2 vector = new Vector2(Mathf.Sin(f), Mathf.Cos(f));
			Vector2 vector2 = pos;
			Vector2 vector3 = vector2 + vector * 40f;
			if (isInner)
			{
				_outter.Add(new Vector3(vector3.x, 0f, vector3.y));
				_inner.Add(new Vector3(vector2.x, 0f, vector2.y));
			}
			else
			{
				_outter.Add(new Vector3(vector2.x, 0f, vector2.y));
				_inner.Add(new Vector3(vector3.x, 0f, vector3.y));
			}
		}
	}
}
