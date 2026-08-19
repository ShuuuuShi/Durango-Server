using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.Control;

public class GlitteringDots : CustomFillSprite
{
	[Serializable]
	public struct DotPosition
	{
		public int Index;

		public float Ratio;
	}

	private struct Pos
	{
		public Vector3 Position;

		public float Ratio;
	}

	public const int MinDotCount = 1;

	public const int MinPointCount = 2;

	[HideInInspector]
	[SerializeField]
	private bool _fixedSpriteSize;

	[HideInInspector]
	[SerializeField]
	private float _duration;

	[HideInInspector]
	[SerializeField]
	private float _delay;

	[HideInInspector]
	[SerializeField]
	private float _speed = 100f;

	[HideInInspector]
	[SerializeField]
	private float _fadeDuration = 0.3f;

	[HideInInspector]
	[SerializeField]
	private Vector3[] _points;

	[HideInInspector]
	[SerializeField]
	private DotPosition[] _initPos;

	private float _dotAlpha;

	private float _enableAt;

	private float _hideAt;

	private float _totalLength;

	private readonly List<Pos> _positions = new List<Pos>();

	private readonly List<float> _dotPositions = new List<float>();

	private readonly List<Vector3> _right = new List<Vector3>();

	private readonly List<Vector3> _left = new List<Vector3>();

	private Vector3[] _spriteVerts = new Vector3[4];

	public Vector3[] Points => _points;

	public DotPosition[] InitPos => _initPos;

	public bool IsShow { get; private set; }

	protected override void OnEnable()
	{
		base.OnEnable();
		if (KUtility.GetSize(_points) != 0)
		{
			Play();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Hide();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (IsShow && Application.isPlaying && !(Time.time < _enableAt))
		{
			UpdateDots();
			CheckTimer();
			mChanged = true;
		}
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		if (!IsShow)
		{
			return;
		}
		UISpriteData atlasSprite = GetAtlasSprite();
		if (atlasSprite == null)
		{
			return;
		}
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		int size = verts.size;
		if (!Application.isPlaying)
		{
			InitPoints();
			for (int i = 0; i < _left.Count; i++)
			{
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = atlasSprite,
					Color = Color.red,
					Rect = new Rect(0f, 0f, 1f, 1f),
					Pivot = new Vector2(0.5f, 0.5f),
					Position = _left[i],
					Size = new Vector2(2f, 2f)
				});
			}
			for (int j = 0; j < _right.Count; j++)
			{
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = atlasSprite,
					Color = Color.green,
					Rect = new Rect(0f, 0f, 1f, 1f),
					Pivot = new Vector2(0.5f, 0.5f),
					Position = _right[j],
					Size = new Vector2(2f, 2f)
				});
			}
			return;
		}
		Color col = new Color(1f, 1f, 1f, _dotAlpha);
		Vector2 size2 = localSize;
		int k = 0;
		for (int size3 = KUtility.GetSize(_dotPositions); k < size3; k++)
		{
			float num = _dotPositions[k];
			if (_fixedSpriteSize)
			{
				int l;
				for (l = 0; l < _positions.Count && !(num < _positions[l].Ratio); l++)
				{
				}
				Pos pos = _positions[(l + _positions.Count - 1) % _positions.Count];
				Pos pos2 = _positions[l % _positions.Count];
				float ratio = pos.Ratio;
				float num2 = pos2.Ratio;
				if (num2 < ratio)
				{
					num2 += 1f;
				}
				Vector3 vector = Vector3.Lerp(t: (num - ratio) / (num2 - ratio), a: pos.Position, b: pos2.Position);
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = atlasSprite,
					Color = col,
					Rect = new Rect(0f, 0f, 1f, 1f),
					Pivot = new Vector2(0.5f, 0.5f),
					Position = vector,
					Size = size2
				});
				continue;
			}
			int count = _left.Count;
			float num3 = (float)count * num;
			float num4 = (float)count * (num + size2.x / _totalLength);
			int m = Mathf.FloorToInt(num3);
			for (int num5 = Mathf.CeilToInt(num4); m <= num5; m++)
			{
				int index = m % count;
				int index2 = (m + 1) % count;
				float num6 = Mathf.Clamp01(((float)m - num3) / (num4 - num3));
				float num7 = Mathf.Clamp01(((float)(m + 1) - num3) / (num4 - num3));
				float t2 = ((!((float)m < num3)) ? 0f : (num3 - (float)m));
				float t3 = ((!((float)m > num4)) ? 1f : (1f - ((float)m - num4)));
				ref Vector3 reference = ref _spriteVerts[0];
				reference = Vector3.Lerp(_left[index], _left[index2], t2);
				ref Vector3 reference2 = ref _spriteVerts[1];
				reference2 = Vector3.Lerp(_right[index], _right[index2], t2);
				ref Vector3 reference3 = ref _spriteVerts[2];
				reference3 = Vector3.Lerp(_right[index], _right[index2], t3);
				ref Vector3 reference4 = ref _spriteVerts[3];
				reference4 = Vector3.Lerp(_left[index], _left[index2], t3);
				DrawSprite(verts, uvs, cols, atlasSprite, _spriteVerts, col, new Rect(num6, 0f, num7 - num6, 1f));
			}
		}
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}

	private void CheckTimer()
	{
		if (_hideAt > 0f && Time.time > _hideAt)
		{
			Hide();
		}
	}

	private void UpdateDots()
	{
		float time = Time.time;
		float num = time - _enableAt;
		float num2 = ((!(_hideAt > 0f)) ? (-1f) : (_hideAt - time));
		float dotAlpha = ((num < _fadeDuration) ? (num / _fadeDuration) : ((!(num2 > 0f) || !(num2 < _fadeDuration)) ? 1f : (num2 / _fadeDuration)));
		_dotAlpha = dotAlpha;
		float num3 = _speed * Time.deltaTime / _totalLength;
		int i = 0;
		for (int size = KUtility.GetSize(_dotPositions); i < size; i++)
		{
			_dotPositions[i] = (_dotPositions[i] + num3) % 1f;
		}
	}

	public void Initialize()
	{
		InitPoints();
		InitDots();
	}

	private void InitPoints()
	{
		int num = 0;
		if (_points.Length >= 1)
		{
			Vector3 vector = _points[0];
			num++;
			int i = 1;
			for (int num2 = _points.Length; i < num2; i++)
			{
				Vector3 vector2 = _points[i];
				if (vector2 == vector)
				{
					vector2.x = float.NaN;
					continue;
				}
				vector = vector2;
				num++;
			}
		}
		Vector3[] points = _points;
		int size = KUtility.GetSize(points);
		if (size != num)
		{
			_points = new Vector3[num];
			int num3 = 0;
			for (int j = 0; j < size; j++)
			{
				Vector3 vector3 = points[j];
				if (!float.IsNaN(vector3.x))
				{
					_points[num3++] = vector3;
				}
			}
		}
		_totalLength = GetTotalLength(_points);
		_left.Clear();
		_right.Clear();
		_positions.Clear();
		for (int k = 0; k < _points.Length; k++)
		{
			Vector3 vector4 = _points[k];
			Pos pos = default(Pos);
			pos.Position = vector4;
			Pos item = pos;
			if (k > 0)
			{
				Vector3 vector5 = _points[k - 1];
				Pos pos2 = _positions[k - 1];
				item.Ratio = ((!(_totalLength > 0f)) ? 0f : (pos2.Ratio + (vector5 - vector4).magnitude / _totalLength));
			}
			_positions.Add(item);
		}
		if (_fixedSpriteSize)
		{
			return;
		}
		for (int l = 0; l < _points.Length; l++)
		{
			Vector3 vector6 = _points[l];
			Vector3 vector7 = _points[(l + 1) % _points.Length];
			Vector3 vector8 = _points[(l + _points.Length - 1) % _points.Length];
			Vector3 vector9 = _points[(l + 2) % _points.Length];
			Vector3 vector10 = vector7 - vector6;
			Vector3 vector11 = vector6 - vector8;
			Vector3 vector12 = vector9 - vector7;
			float f = Vector2.SignedAngle(-vector11, vector10);
			float num4 = Vector2.SignedAngle(-vector10, vector12);
			Vector3 normalized = vector10.normalized;
			Vector3 vector13 = vector6 + normalized * base.height * 0.5f / Mathf.Tan(Mathf.Abs(f) * 0.5f * ((float)Math.PI / 180f));
			Vector3 vector14 = vector7 - normalized * base.height * 0.5f / Mathf.Tan(Mathf.Abs(num4) * 0.5f * ((float)Math.PI / 180f));
			FillLine(vector13, vector14, 0.5f);
			if (!(Mathf.Abs(Mathf.Abs(num4) - 180f) < 10f))
			{
				Vector3 normalized2 = (-normalized + vector12.normalized).normalized;
				Vector3 vector15 = vector7 + normalized2 * ((float)base.height * 0.5f) / Mathf.Sin(Mathf.Abs(num4) * 0.5f * ((float)Math.PI / 180f));
				float num5 = Mathf.Atan2(vector10.y, vector10.x) * 57.29578f;
				float num6 = Mathf.Atan2(vector12.y, vector12.x) * 57.29578f;
				if (Mathf.Abs(num6 - num5) > 180f)
				{
					num6 -= 360f;
				}
				if (num4 > 0f)
				{
					FillCorner(vector15, num5 + 90f, num6 + 90f, isLeft: false);
				}
				else
				{
					FillCorner(vector15, num5 - 90f, num6 - 90f, isLeft: true);
				}
				continue;
			}
			break;
		}
	}

	private void InitDots()
	{
		_dotPositions.Clear();
		for (int i = 0; i < _initPos.Length; i++)
		{
			_dotPositions.Add(ToRatio(_initPos[i], _points));
		}
	}

	private void FillLine(Vector2 p1, Vector2 p2, float pivotRatio)
	{
		Vector2 vector = p2 - p1;
		float magnitude = vector.magnitude;
		int num = Mathf.RoundToInt(magnitude / 6f);
		Vector2 vector2 = vector / magnitude;
		Vector2 vector3 = new Vector2(vector2.y, 0f - vector2.x);
		float num2 = base.height;
		for (int i = 0; i < num; i++)
		{
			Vector2 vector4 = Vector2.Lerp(p1, p2, (float)i / (float)num);
			Vector2 vector5 = vector4 + vector3 * num2 * (1f - pivotRatio);
			Vector2 vector6 = vector4 - vector3 * num2 * pivotRatio;
			_right.Add(vector5);
			_left.Add(vector6);
		}
	}

	private void FillCorner(Vector2 pos, float start, float end, bool isLeft)
	{
		int num = Mathf.RoundToInt(Mathf.Abs(end - start) / 20f);
		float num2 = base.height;
		for (int i = 0; i < num; i++)
		{
			float f = Mathf.Lerp(start, end, (float)i / (float)num) * ((float)Math.PI / 180f);
			Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
			Vector2 vector2 = pos;
			Vector2 vector3 = vector2 + vector * num2;
			if (isLeft)
			{
				_left.Add(new Vector3(vector2.x, vector2.y));
				_right.Add(new Vector3(vector3.x, vector3.y));
			}
			else
			{
				_right.Add(new Vector3(vector2.x, vector2.y));
				_left.Add(new Vector3(vector3.x, vector3.y));
			}
		}
	}

	public void SetDepth(int d)
	{
		depth = d;
	}

	public void Play()
	{
		Show(_duration, _delay);
	}

	public void Show(float duration = 0f, float delay = 0f)
	{
		if (!IsShow && base.gameObject.activeInHierarchy)
		{
			IsShow = true;
			_enableAt = Time.time + delay;
			_hideAt = ((!(duration > 0f)) ? 0f : (_enableAt + duration));
			Initialize();
			_dotAlpha = 0f;
		}
	}

	public void Hide()
	{
		if (IsShow)
		{
			IsShow = false;
		}
	}

	public static Vector3 GetCenter(Vector3[] points)
	{
		Vector3 zero = Vector3.zero;
		int size = KUtility.GetSize(points);
		for (int i = 0; i < size; i++)
		{
			zero += points[i];
		}
		return zero / size;
	}

	public static float ToRatio(DotPosition pos, Vector3[] points)
	{
		float result = 0f;
		float totalLength = GetTotalLength(points);
		float num = 0f;
		int i = 0;
		for (int size = KUtility.GetSize(points); i < size; i++)
		{
			Vector3 vector = points[i];
			float magnitude = (points[(i + 1) % size] - vector).magnitude;
			if (i == pos.Index)
			{
				result = (num + magnitude * pos.Ratio) / totalLength;
				break;
			}
			num += magnitude;
		}
		return result;
	}

	public static float GetTotalLength(IList<Vector3> points)
	{
		float num = 0f;
		int i = 0;
		for (int size = KUtility.GetSize(points); i < size; i++)
		{
			Vector3 vector = points[i];
			Vector3 vector2 = points[(i + 1) % size];
			num += (vector2 - vector).magnitude;
		}
		return num;
	}
}
