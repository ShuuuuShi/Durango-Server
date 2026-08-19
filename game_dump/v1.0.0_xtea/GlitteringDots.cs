using System;
using UnityEngine;

public class GlitteringDots : MonoBehaviour
{
	public enum PresetShape
	{
		Rect,
		Hexagon
	}

	public class Dot
	{
		public UISprite DotSprite;

		public DotPos Pos;

		public int Index
		{
			get
			{
				return Pos.Index;
			}
			set
			{
				Pos.Index = value;
			}
		}

		public float Ratio
		{
			get
			{
				return Pos.Ratio;
			}
			set
			{
				Pos.Ratio = value;
			}
		}
	}

	[Serializable]
	public struct DotPos
	{
		public int Index;

		public float Ratio;
	}

	public const int MinDotCount = 1;

	public const int MinPointCount = 2;

	[SerializeField]
	private PresetShape _preset;

	[SerializeField]
	private float _duration;

	[SerializeField]
	private float _delay;

	[SerializeField]
	private int _depth;

	[SerializeField]
	private SpriteData _dotSprite;

	[SerializeField]
	private Color _dotColor = Color.white;

	[SerializeField]
	private int _dotSize = 20;

	[SerializeField]
	private float _speed = 100f;

	[SerializeField]
	private float _fadeDuration = 0.3f;

	[SerializeField]
	private Vector3[] _points;

	[SerializeField]
	private DotPos[] _initPos;

	private Dot[] _dots;

	private float[] _length;

	private float _enableAt;

	private float _hideAt;

	private bool _isShow;

	public PresetShape Preset
	{
		get
		{
			return _preset;
		}
		set
		{
			_preset = value;
		}
	}

	public Vector3[] Points => _points;

	public DotPos[] InitPos => _initPos;

	private void OnEnable()
	{
		if (_points != null && _points.Length != 0)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		Hide();
	}

	private void Update()
	{
		if (_isShow && !(Time.time < _enableAt))
		{
			UpdateDots();
			CheckTimer();
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
		if (num < _fadeDuration)
		{
			SetAlpha(num / _fadeDuration);
		}
		else if (num2 > 0f && num2 < _fadeDuration)
		{
			SetAlpha(num2 / _fadeDuration);
		}
		else
		{
			SetAlpha(1f);
		}
		int i = 0;
		for (int num3 = ((_dots != null) ? _dots.Length : 0); i < num3; i++)
		{
			SetNextPosition(ref _dots[i], _speed * Time.deltaTime);
		}
	}

	private void SetAlpha(float alpha)
	{
		int i = 0;
		for (int num = ((_dots != null) ? _dots.Length : 0); i < num; i++)
		{
			_dots[i].DotSprite.alpha = alpha;
		}
	}

	private void SetNextPosition(ref Dot dot, float delta)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		float num = _length[dot.Index];
		float num2 = num * (1f - dot.Ratio);
		if (delta > num2)
		{
			delta -= num2;
			dot.Index = (dot.Index + 1) % _points.Length;
			dot.Ratio = 0f;
			SetNextPosition(ref dot, delta);
		}
		else
		{
			dot.Ratio += delta / num;
			((Component)dot.DotSprite).transform.localPosition = GetPos(dot.Pos, _points);
		}
	}

	private void Initialize()
	{
		InitPoints();
		InitDots();
	}

	private void InitPoints()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		if (_points != null && _points.Length >= 1)
		{
			Vector3 val = _points[0];
			num++;
			int i = 1;
			for (int num2 = _points.Length; i < num2; i++)
			{
				Vector3 val2 = _points[i];
				if (val2 == val)
				{
					val2.x = float.NaN;
					continue;
				}
				val = val2;
				num++;
			}
		}
		Vector3[] points = _points;
		int num3 = ((points != null) ? points.Length : 0);
		if (num3 != num)
		{
			_points = (Vector3[])(object)new Vector3[num];
			int num4 = 0;
			for (int j = 0; j < num3; j++)
			{
				Vector3 val3 = points[j];
				if (!float.IsNaN(val3.x))
				{
					_points[num4++] = val3;
				}
			}
		}
		float totalLength = GetTotalLength(_points);
		if (_initPos == null || _points == null || _initPos.Length < 1 || _points.Length < 2 || totalLength == 0f)
		{
			InitPreset(Preset);
		}
		if (_length == null || _length.Length != _points.Length)
		{
			_length = new float[_points.Length];
		}
		int k = 0;
		for (int num5 = _length.Length; k < num5; k++)
		{
			Vector3 val4 = _points[k];
			Vector3 val5 = _points[(k + 1) % num5];
			float[] length = _length;
			int num6 = k;
			Vector3 val6 = val5 - val4;
			length[num6] = ((Vector3)(ref val6)).magnitude;
		}
	}

	private void InitDots()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		int num = ((_dots != null) ? _dots.Length : 0);
		int num2 = _initPos.Length;
		if (num != num2)
		{
			Dot[] dots = _dots;
			_dots = new Dot[num2];
			int i = 0;
			for (int num3 = Mathf.Min(num, num2); i < num3; i++)
			{
				_dots[i] = dots[i];
			}
			for (int j = num2; j < num; j++)
			{
				Object.Destroy((Object)(object)((Component)dots[j].DotSprite).gameObject);
			}
		}
		for (int k = 0; k < num2; k++)
		{
			if (_dots[k] == null)
			{
				_dots[k] = new Dot();
			}
			UISprite uISprite = _dots[k].DotSprite;
			if ((Object)(object)uISprite == (Object)null)
			{
				uISprite = ((Component)this).gameObject.AddChild<UISprite>();
			}
			_dotSprite.Set(uISprite);
			uISprite.color = _dotColor;
			UIUtility.ResizeToSquare(uISprite, _dotSize);
			uISprite.depth = _depth;
			_dots[k].DotSprite = uISprite;
			_dots[k].Pos = _initPos[k];
		}
	}

	public void SetPointSize(int size)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		size = Mathf.Max(2, size);
		if (_points == null || _points.Length != size)
		{
			Vector3[] points = _points;
			_points = (Vector3[])(object)new Vector3[size];
			int num = ((points != null) ? points.Length : 0);
			int i = 0;
			for (int num2 = Mathf.Min(num, size); i < num2; i++)
			{
				ref Vector3 reference = ref _points[i];
				reference = points[i];
			}
			for (int j = num; j < size; j++)
			{
				ref Vector3 reference2 = ref _points[j];
				reference2 = Vector3.right * ((float)(j - num) - (float)(size - num) * 0.5f) * 10f;
			}
		}
	}

	public void SetInitPointSize(int size)
	{
		size = Mathf.Max(1, size);
		if (_initPos == null || _initPos.Length != size)
		{
			DotPos[] initPos = _initPos;
			_initPos = new DotPos[size];
			int num = ((initPos != null) ? initPos.Length : 0);
			int i = 0;
			for (int num2 = Mathf.Min(num, size); i < num2; i++)
			{
				ref DotPos reference = ref _initPos[i];
				reference = initPos[i];
			}
		}
	}

	public void InitPreset(PresetShape preset)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = ((Component)this).GetComponent<UIWidget>();
		Vector2 val = ((!((Object)(object)component == (Object)null)) ? new Vector2((float)component.width, (float)component.height) : new Vector2(100f, 100f));
		Vector3 val2 = ((!((Object)(object)component == (Object)null)) ? component.localCenter : Vector3.zero);
		switch (preset)
		{
		case PresetShape.Rect:
		{
			SetPointSize(4);
			Vector2 val3 = val * 0.5f;
			ref Vector3 reference4 = ref _points[0];
			reference4 = new Vector3(0f - val3.x, 0f - val3.y);
			ref Vector3 reference5 = ref _points[1];
			reference5 = new Vector3(0f - val3.x, val3.y);
			ref Vector3 reference6 = ref _points[2];
			reference6 = new Vector3(val3.x, val3.y);
			ref Vector3 reference7 = ref _points[3];
			reference7 = new Vector3(val3.x, 0f - val3.y);
			SetInitPointSize(2);
			ref DotPos reference8 = ref _initPos[0];
			reference8 = ToPosition(0f, _points);
			ref DotPos reference9 = ref _initPos[1];
			reference9 = ToPosition(0.5f, _points);
			break;
		}
		case PresetShape.Hexagon:
		{
			float num = Mathf.Max(val.x, val.y) * 0.5f;
			float num2 = 0f;
			SetPointSize(6);
			int i = 0;
			for (int num3 = _points.Length; i < num3; i++)
			{
				ref Vector3 reference = ref _points[i];
				reference = new Vector3(Mathf.Cos(num2) * num, Mathf.Sin(num2) * num);
				num2 -= (float)Math.PI * 2f / (float)num3;
			}
			SetInitPointSize(2);
			ref DotPos reference2 = ref _initPos[0];
			reference2 = ToPosition(0f, _points);
			ref DotPos reference3 = ref _initPos[1];
			reference3 = ToPosition(0.5f, _points);
			break;
		}
		}
		int j = 0;
		for (int num4 = ((_points != null) ? _points.Length : 0); j < num4; j++)
		{
			ref Vector3 reference10 = ref _points[j];
			reference10 += val2;
		}
	}

	public void SetDepth(int depth)
	{
		_depth = depth;
	}

	public void SetSprite(UIAtlas atlas, string spriteName)
	{
		_dotSprite.atlas = atlas;
		_dotSprite.sprite = spriteName;
	}

	public void Play()
	{
		Show(_duration, _delay);
	}

	public void Show(float duration = 0f, float delay = 0f)
	{
		if (!_isShow && ((Component)this).gameObject.activeInHierarchy)
		{
			_isShow = true;
			_enableAt = Time.time + delay;
			_hideAt = ((!(duration > 0f)) ? 0f : (_enableAt + duration));
			Initialize();
			int i = 0;
			for (int num = ((_dots != null) ? _dots.Length : 0); i < num; i++)
			{
				((Component)_dots[i].DotSprite).gameObject.SetActive(true);
			}
			SetAlpha(0f);
			((Behaviour)this).enabled = true;
		}
	}

	public void Hide()
	{
		if (_isShow)
		{
			_isShow = false;
			int i = 0;
			for (int num = ((_dots != null) ? _dots.Length : 0); i < num; i++)
			{
				((Component)_dots[i].DotSprite).gameObject.SetActive(false);
			}
			((Behaviour)this).enabled = false;
		}
	}

	public static Vector3 GetPos(DotPos pos, Vector3[] points)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = points[pos.Index];
		Vector3 val2 = points[(pos.Index + 1) % points.Length];
		return Vector3.Lerp(val, val2, pos.Ratio);
	}

	public static Vector3 GetCenter(Vector3[] points)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		int num = ((points != null) ? points.Length : 0);
		for (int i = 0; i < num; i++)
		{
			val += points[i];
		}
		return val / (float)num;
	}

	public static DotPos ToPosition(float lenRatio, Vector3[] points)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		int index = 0;
		float ratio = 0f;
		float totalLength = GetTotalLength(points);
		float num = 0f;
		int i = 0;
		for (int num2 = ((points != null) ? points.Length : 0); i < num2; i++)
		{
			Vector3 val = points[i];
			Vector3 val2 = points[(i + 1) % num2];
			Vector3 val3 = val2 - val;
			float magnitude = ((Vector3)(ref val3)).magnitude;
			float num3 = num / totalLength;
			float num4 = (num + magnitude) / totalLength;
			if (num3 <= lenRatio && lenRatio <= num4)
			{
				index = i;
				ratio = (lenRatio - num3) / (num4 - num3);
				break;
			}
			num += magnitude;
		}
		DotPos result = default(DotPos);
		result.Index = index;
		result.Ratio = ratio;
		return result;
	}

	public static float ToRatio(DotPos pos, Vector3[] points)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		float result = 0f;
		float totalLength = GetTotalLength(points);
		float num = 0f;
		int i = 0;
		for (int num2 = ((points != null) ? points.Length : 0); i < num2; i++)
		{
			Vector3 val = points[i];
			Vector3 val2 = points[(i + 1) % num2];
			Vector3 val3 = val2 - val;
			float magnitude = ((Vector3)(ref val3)).magnitude;
			if (i == pos.Index)
			{
				result = (num + magnitude * pos.Ratio) / totalLength;
				break;
			}
			num += magnitude;
		}
		return result;
	}

	public static float GetTotalLength(Vector3[] points)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		int i = 0;
		for (int num2 = ((points != null) ? points.Length : 0); i < num2; i++)
		{
			Vector3 val = points[i];
			Vector3 val2 = points[(i + 1) % num2];
			float num3 = num;
			Vector3 val3 = val2 - val;
			num = num3 + ((Vector3)(ref val3)).magnitude;
		}
		return num;
	}

	public static void Resize(float ratio, Vector3[] points)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Vector3 center = GetCenter(points);
		int i = 0;
		for (int num = ((points != null) ? points.Length : 0); i < num; i++)
		{
			Vector3 val = points[i];
			Vector3 val2 = val - center;
			float magnitude = ((Vector3)(ref val2)).magnitude;
			((Vector3)(ref val2)).Normalize();
			ref Vector3 reference = ref points[i];
			reference = center + val2 * magnitude * ratio;
		}
	}
}
