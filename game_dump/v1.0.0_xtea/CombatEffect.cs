using System;
using Holoville.HOTween;
using UnityEngine;

public class CombatEffect : MonoBehaviour
{
	public enum Type
	{
		Off,
		Battle,
		Leaving,
		Waiting
	}

	[ExposedInEditor(null)]
	public float BorderSize = 10f;

	[SerializeField]
	private UISprite _borderEffect;

	[SerializeField]
	private UISprite[] _sideEffects;

	[SerializeField]
	private CombatModeButton _combatModeButton;

	[SerializeField]
	private float _speed;

	[SerializeField]
	private Color _colorWarning1;

	[SerializeField]
	private Color _colorWarning2;

	[SerializeField]
	private Color _colorDanger1;

	[SerializeField]
	private Color _colorDanger2;

	private Color _color1;

	private Color _color2;

	private int _width;

	private int _height;

	private UIWidget _widget;

	private bool _showEffect;

	private float _borderEffectBeginTime = -1f;

	[SerializeField]
	private float _borderEffectBeginWidth = 35f;

	[SerializeField]
	private float _borderEffectAnimatingDuration = 1f;

	private BetterList<int> _borderEffectiveVertexIndices = new BetterList<int>();

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private bool IsBorderEffectAnimating => _borderEffectBeginTime > 0f && Time.time - _borderEffectBeginTime < _borderEffectAnimatingDuration;

	private void Awake()
	{
		int i = 0;
		for (int num = _sideEffects.Length; i < num; i++)
		{
			UISprite obj = _sideEffects[i];
			obj.onPostFill = (UIWidget.OnPostFillCallback)Delegate.Combine(obj.onPostFill, new UIWidget.OnPostFillCallback(OnSideEffectPostFill));
		}
		UISprite borderEffect = _borderEffect;
		borderEffect.onPostFill = (UIWidget.OnPostFillCallback)Delegate.Combine(borderEffect.onPostFill, new UIWidget.OnPostFillCallback(OnBorderEffectPostFill));
		_width = UIManager.ScreenWidth;
		_height = UIManager.ScreenHeight;
		TweenAlpha component = ((Component)this).GetComponent<TweenAlpha>();
		component.SetOnFinished(delegate
		{
			if (Widget.alpha == 0f)
			{
				((Component)this).gameObject.SetActive(false);
			}
		});
		if (GameManager.IsPrologueMode)
		{
			((Component)_combatModeButton).gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		int i = 0;
		for (int num = _sideEffects.Length; i < num; i++)
		{
			UpdateColor(_sideEffects[i]);
			_sideEffects[i].SetDirty();
		}
		if (IsBorderEffectAnimating)
		{
			_borderEffect.Invalidate(includeChildren: false);
		}
	}

	private void UpdateColor(UIWidget widget)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)widget == (Object)null)
		{
			return;
		}
		UIGeometry geometry = widget.geometry;
		int width = _width;
		int height = _height;
		int num = width / 2;
		int num2 = height / 2;
		float num3 = Time.time * _speed % (float)((width + height) * 2);
		Vector2 val = default(Vector2);
		if (num3 < (float)(width + height))
		{
			if (num3 < (float)width)
			{
				val.x = num3 - (float)num;
				val.y = num2;
			}
			else
			{
				val.x = num;
				val.y = (float)num2 - (num3 - (float)width);
			}
		}
		else
		{
			num3 -= (float)(width + height);
			if (num3 < (float)width)
			{
				val.x = (float)num - num3;
				val.y = -num2;
			}
			else
			{
				val.x = -num;
				val.y = num3 - (float)width - (float)num2;
			}
		}
		float num4 = Mathf.Atan2(val.y, val.x);
		if (num4 < 0f)
		{
			num4 += (float)Math.PI * 2f;
		}
		Vector3 val2 = Vector3.zero;
		Transform val3 = ((Component)widget).transform;
		while ((Object)(object)val3 != (Object)null)
		{
			val2 += val3.localPosition;
			val3 = val3.parent;
		}
		int size = geometry.cols.size;
		for (int i = 0; i < size; i++)
		{
			Vector3 val4 = geometry.verts[i] + val2;
			float num5 = Mathf.Atan2(val4.y, val4.x);
			if (num5 < 0f)
			{
				num5 += (float)Math.PI * 2f;
			}
			float num6 = Mathf.Abs(num5 - num4);
			if (num6 > (float)Math.PI)
			{
				num6 = (float)Math.PI * 2f - num6;
			}
			float num7 = num6 / (float)Math.PI;
			float a = geometry.cols[i].a;
			Color value = Color.Lerp(_color1, _color2, num7 * num7);
			value.a = a;
			geometry.cols[i] = value;
		}
	}

	private void OnSideEffectPostFill(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		UpdateColor(widget);
	}

	private void OnBorderEffectPostFill(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)widget == (Object)null))
		{
			UIGeometry geometry = widget.geometry;
			if (_borderEffectiveVertexIndices.size == 0)
			{
				InitBorderEffectVertices(widget);
			}
			for (int i = 0; i < _borderEffectiveVertexIndices.size; i++)
			{
				int i2 = _borderEffectiveVertexIndices[i];
				Vector3 value = geometry.verts[i2];
				value.x = ((!(value.x > 0f)) ? (value.x + BorderSize) : (value.x - BorderSize));
				value.y = ((!(value.y > 0f)) ? (value.y + BorderSize) : (value.y - BorderSize));
				geometry.verts[i2] = value;
			}
		}
	}

	private void InitBorderEffectVertices(UIWidget widget)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		UIGeometry geometry = widget.geometry;
		int size = geometry.verts.size;
		Vector2 val = Vector2.one * 99999f;
		Vector2 val2 = -Vector2.one * 99999f;
		for (int i = 0; i < size; i++)
		{
			val.x = Mathf.Min(val.x, geometry.verts[i].x);
			val.y = Mathf.Min(val.y, geometry.verts[i].y);
			val2.x = Mathf.Max(val2.x, geometry.verts[i].x);
			val2.y = Mathf.Max(val2.y, geometry.verts[i].y);
		}
		for (int j = 0; j < size; j++)
		{
			Vector3 val3 = geometry.verts[j];
			if (!Mathf.Approximately(val.x, val3.x) && !Mathf.Approximately(val2.x, val3.x) && !Mathf.Approximately(val.y, val3.y) && !Mathf.Approximately(val2.y, val3.y))
			{
				_borderEffectiveVertexIndices.Add(j);
			}
		}
	}

	private void BeginBorderEffect()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		((Component)_borderEffect).gameObject.SetActive(true);
		if (_borderEffectBeginTime <= 0f)
		{
			_borderEffectBeginTime = Time.time;
			BorderSize = _borderEffectBeginWidth;
			TweenParms val = new TweenParms();
			val.Prop("BorderSize", (object)0);
			val.Ease((EaseType)0);
			HOTween.To((object)this, _borderEffectAnimatingDuration, val);
		}
	}

	private void EndBorderEffect()
	{
		((Component)_borderEffect).gameObject.SetActive(false);
		_borderEffectBeginTime = -1f;
	}

	public void SetCombatEffect(CombatSystem.State combatState)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (combatState == CombatSystem.State.Battle)
		{
			_color1 = _colorDanger1;
			_color2 = _colorDanger2;
		}
		else
		{
			_color1 = _colorWarning1;
			_color2 = _colorWarning2;
		}
		bool flag = combatState != CombatSystem.State.None;
		if (_showEffect != flag)
		{
			if (flag)
			{
				((Component)this).gameObject.SetActive(true);
				TweenAlpha component = ((Component)this).GetComponent<TweenAlpha>();
				component.tweenFactor = 0f;
				component.PlayForward();
				BeginBorderEffect();
			}
			else
			{
				TweenAlpha component2 = ((Component)this).GetComponent<TweenAlpha>();
				component2.PlayReverse();
				EndBorderEffect();
			}
			UIManager.FindScript<PlayerHudGroup>().ShowExpBar(!flag);
			_showEffect = flag;
		}
	}
}
