using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class HyperGaugeViewer : MonoBehaviour
{
	public delegate int ToIntDelegate(float value);

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _mainUpper;

	[SerializeField]
	private UISprite _mainTrailSprite;

	[SerializeField]
	private UISprite _subUpper;

	[SerializeField]
	private UISprite _subTrailSprite;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UILabel _labelMax;

	[SerializeField]
	private Color[] _labelColors;

	[SerializeField]
	private float[] _labelColorRatios;

	[SerializeField]
	private UISprite[] _velocityArrows;

	[SerializeField]
	private float[] _velocityPoints;

	[SerializeField]
	private float _smoothPeriod;

	[CanBeNull]
	[SerializeField]
	private UISprite _extraGauge;

	[CanBeNull]
	[SerializeField]
	private UISprite _extraGaugeTrail;

	[CanBeNull]
	[SerializeField]
	private UISprite _extraGaugeBackground;

	[CanBeNull]
	[SerializeField]
	private Color[] _extraGaugeColors;

	[CanBeNull]
	[SerializeField]
	private Color[] _extraGaugeTrailColors;

	[CanBeNull]
	[SerializeField]
	private UILabel _phaseLabel;

	private readonly List<GaugeNode> _current = new List<GaugeNode>();

	private readonly List<GaugeNode> _max = new List<GaugeNode>();

	private Vector2 _range;

	private int _lastGaugeCur = int.MaxValue;

	private int _lastGaugeMax = int.MaxValue;

	private float _mainTrailRatio;

	private float _extraTrailRatio;

	private float _subTrailRatio;

	private float _gaugeScale;

	private float[] _lifeGaugeRatio;

	private int _lastLifeGaugePhase;

	public ToIntDelegate ToIntFunction;

	private void Awake()
	{
		if (_velocityPoints != null)
		{
			Array.Sort(_velocityPoints);
		}
	}

	public void Set(Gauge gauge, bool smooth = true, float gaugeScale = 1f, float[] lifeGaugeRatio = null)
	{
		if (gauge == null)
		{
			_range = Vector2.zero;
			_current.Clear();
			_max.Clear();
		}
		else
		{
			_range.x = gauge.RealMin();
			_range.y = gauge.RealMax();
			if (smooth && _smoothPeriod > 0f)
			{
				SetSmooth(_current, gauge, _smoothPeriod);
				SetSmooth(_max, gauge.MaxGauge, _smoothPeriod);
			}
			else
			{
				_current.Clear();
				_max.Clear();
				_current.AddRange(gauge.Determination);
				if (gauge.MaxGauge != null)
				{
					_max.AddRange(gauge.MaxGauge.Determination);
				}
			}
		}
		if (_current.Count == 0)
		{
			if (_mainUpper != null)
			{
				_mainUpper.gameObject.SetActive(value: false);
			}
			if (_subUpper != null)
			{
				_subUpper.gameObject.SetActive(value: false);
			}
		}
		else
		{
			if (_mainUpper != null)
			{
				_mainUpper.gameObject.SetActive(value: true);
			}
			if (_subUpper != null)
			{
				_subUpper.gameObject.SetActive(_max != null);
			}
		}
		_gaugeScale = gaugeScale;
		_lifeGaugeRatio = lifeGaugeRatio;
		if (_lifeGaugeRatio != null)
		{
			Array.Sort(_lifeGaugeRatio);
		}
		Update();
	}

	public void RemoveTrail()
	{
		if (_mainUpper != null)
		{
			_mainTrailRatio = GetGaugeRatio(_mainUpper);
		}
		if (_max.Count > 0 && _subUpper != null)
		{
			_subTrailRatio = GetGaugeRatio(_subUpper);
		}
		_lastLifeGaugePhase = -1;
	}

	private static void SetSmooth(List<GaugeNode> list, Gauge gauge, float smoothTime)
	{
		if (gauge == null)
		{
			list.Clear();
			return;
		}
		if (list.Count == 0)
		{
			list.AddRange(gauge.Determination);
			return;
		}
		double currentTime = Gauge.CurrentTime;
		double num = currentTime + (double)smoothTime;
		Gauge.CurrentValueAndVelocity(list, Gauge.CurrentTime, out var value, out var _);
		float value2 = gauge.Get(num);
		list.Clear();
		list.AddRange(gauge.Determination);
		InsertGaugeNode(list, new GaugeNode
		{
			Time = currentTime,
			Value = value
		});
		InsertGaugeNode(list, new GaugeNode
		{
			Time = num,
			Value = value2
		});
	}

	private static void InsertGaugeNode(IList<GaugeNode> list, GaugeNode node)
	{
		int num = -1;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (node.Time < list[i].Time)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			list.Add(node);
		}
		else
		{
			list.Insert(num, node);
		}
	}

	private void SetGaugeSpriteRatio([CanBeNull] UISprite gaugeSprite, float ratio)
	{
		if (gaugeSprite == null)
		{
			return;
		}
		if (gaugeSprite.type == UIBasicSprite.Type.Filled)
		{
			int num = (int)(gaugeSprite.fillAmount * (float)gaugeSprite.width);
			int num2 = (int)((float)gaugeSprite.width * ratio);
			if (num != num2)
			{
				gaugeSprite.fillAmount = ratio;
			}
			return;
		}
		int width = gaugeSprite.width;
		int num3 = (int)((float)_background.width * ratio);
		if (num3 > 0)
		{
			if (width != num3)
			{
				gaugeSprite.width = num3;
			}
			gaugeSprite.alpha = 1f;
		}
		else
		{
			gaugeSprite.alpha = 0f;
		}
	}

	private float GetGaugeRatio([NotNull] UISprite gaugeSprite)
	{
		int num = ((gaugeSprite.type != UIBasicSprite.Type.Filled) ? ((gaugeSprite.alpha > 0f) ? gaugeSprite.width : 0) : ((int)(gaugeSprite.fillAmount * (float)gaugeSprite.width)));
		return (float)num / (float)_background.width;
	}

	private void SetTrailGaugeSprite([NotNull] UISprite gaugeSprite, [NotNull] UISprite trailSprite, ref float trailRatio)
	{
		float gaugeRatio = GetGaugeRatio(gaugeSprite);
		float num2;
		if (gaugeRatio < trailRatio)
		{
			float num = Time.deltaTime / 5f;
			num2 = Mathf.Max(trailRatio - num, gaugeRatio);
		}
		else
		{
			num2 = gaugeRatio;
		}
		trailRatio = num2;
		SetGaugeSpriteRatio(trailSprite, num2);
	}

	private void SetGaugeRatio([NotNull] UISprite baseGauge, UISprite[] exGauges, float current, float max)
	{
		int num = 1;
		if (exGauges != null)
		{
			num += exGauges.Length;
		}
		float num2 = max / (float)num;
		float num3 = current / num2;
		float num4 = 0f;
		for (int num5 = num - 1; num5 > 0; num5--)
		{
			UISprite uISprite = exGauges[num5 - 1];
			if (uISprite != null)
			{
				num4 = Mathf.Clamp01(num3 - (float)num5);
				SetGaugeSpriteRatio(uISprite, num4);
			}
		}
		num4 = Mathf.Clamp01(num3);
		SetGaugeSpriteRatio(baseGauge, num4);
	}

	private void Update()
	{
		if (_current.Count == 0)
		{
			return;
		}
		Gauge.CurrentValueAndVelocity(_current, Gauge.CurrentTime, out var value, out var velocity);
		Gauge.CurrentValueAndVelocity(_max, Gauge.CurrentTime, out var _, out var velocity2);
		float y = _range.y;
		if (_mainUpper != null)
		{
			if (KUtility.GetSize(_lifeGaugeRatio) > 0 && _extraGauge != null && _extraGaugeBackground != null && _extraGaugeTrail != null)
			{
				if (_extraGaugeColors == null || _extraGaugeTrailColors == null)
				{
					return;
				}
				float num = y;
				float num2 = -1f;
				float num3 = 0f;
				Color color = new Color(0f, 0f, 0f, 0f);
				Color color2 = new Color(0f, 0f, 0f, 0f);
				Color color3 = new Color(0f, 0f, 0f, 0f);
				int num4 = 1;
				for (int num5 = _lifeGaugeRatio.Length - 1; num5 >= 0; num5--)
				{
					float num6 = y * _lifeGaugeRatio[num5];
					if (value > num6)
					{
						num4 = num5 + 2;
						num2 = num - num6;
						num3 = value - num6;
						color = _extraGaugeColors[num5 % _extraGaugeColors.Length];
						color2 = _extraGaugeTrailColors[num5 % _extraGaugeTrailColors.Length];
						color3 = ((num5 != 0) ? _extraGaugeColors[(num5 - 1) % _extraGaugeColors.Length] : _mainUpper.color);
						break;
					}
					num = num6;
				}
				float ratio = ((num4 != 1) ? 1f : (value / num));
				float ratio2 = ((num4 != 1) ? (num3 / num2) : 0f);
				SetGaugeSpriteRatio(_mainUpper, ratio);
				SetGaugeSpriteRatio(_extraGauge, ratio2);
				_extraGauge.color = color;
				_extraGaugeBackground.color = color3;
				_extraGaugeTrail.color = color2;
				if (_lastLifeGaugePhase < 0)
				{
					_lastLifeGaugePhase = num4;
				}
				if (_lastLifeGaugePhase > num4)
				{
					_extraTrailRatio = 1f;
				}
				else if (_lastLifeGaugePhase < num4)
				{
					_extraTrailRatio = 0f;
				}
				if (num4 == 1 && _mainTrailSprite != null)
				{
					SetTrailGaugeSprite(_mainUpper, _mainTrailSprite, ref _mainTrailRatio);
				}
				else if (num4 > 1)
				{
					SetTrailGaugeSprite(_extraGauge, _extraGaugeTrail, ref _extraTrailRatio);
				}
				if (_phaseLabel != null)
				{
					_phaseLabel.text = $"x[size=20]{num4}[/size]";
				}
				_lastLifeGaugePhase = num4;
			}
			else
			{
				float ratio3 = Mathf.Clamp01(value / y);
				SetGaugeSpriteRatio(_mainUpper, ratio3);
				if (_mainTrailSprite != null)
				{
					SetTrailGaugeSprite(_mainUpper, _mainTrailSprite, ref _mainTrailRatio);
				}
				SetGaugeSpriteRatio(_extraGauge, 0f);
				SetGaugeSpriteRatio(_extraGaugeTrail, 0f);
				if (_extraGaugeBackground != null)
				{
					_extraGaugeBackground.alpha = 0f;
				}
				if (_phaseLabel != null)
				{
					_phaseLabel.text = string.Empty;
				}
			}
		}
		if (_max.Count > 0 && _subUpper != null)
		{
			Gauge.CurrentValueAndVelocity(_max, Gauge.CurrentTime, out var value3, out var _);
			float ratio4 = Mathf.Clamp01(value3 / _range.y);
			SetGaugeSpriteRatio(_subUpper, ratio4);
			if (_subTrailSprite != null)
			{
				SetTrailGaugeSprite(_subUpper, _subTrailSprite, ref _subTrailRatio);
			}
		}
		if (_label != null)
		{
			if (ToIntFunction == null)
			{
				ToIntFunction = Mathf.FloorToInt;
			}
			int num7 = ToIntFunction(value * _gaugeScale);
			int num8 = ToIntFunction(y * _gaugeScale);
			if (num7 != _lastGaugeCur || num8 != _lastGaugeMax)
			{
				Color color4 = Color.clear;
				if (_labelColors != null && _labelColorRatios != null)
				{
					float num9 = value / y;
					int num10 = Mathf.Min(_labelColors.Length, _labelColorRatios.Length);
					for (int i = 0; i < num10; i++)
					{
						if (_labelColorRatios[i] >= num9)
						{
							color4 = _labelColors[i];
							break;
						}
					}
					if (color4 == Color.clear && _labelColors.Length > num10)
					{
						color4 = _labelColors[num10];
					}
				}
				if (color4 == Color.clear)
				{
					if (_labelMax != null)
					{
						_label.text = $"{num7}";
						_labelMax.text = $" / {num8}";
					}
					else
					{
						_label.text = $"{num7} [b7b7b7]/[-] {num8}";
					}
				}
				else if (_labelMax != null)
				{
					_label.text = string.Format("{1}{0}[-]", num7, UIManager.ColorBBCode(color4));
					_labelMax.text = $" / {num8}";
				}
				else
				{
					_label.text = string.Format("{2}{0}[-] [b7b7b7]/[-] {1}", num7, num8, UIManager.ColorBBCode(color4));
				}
				_lastGaugeCur = num7;
				_lastGaugeMax = num8;
			}
		}
		if (_velocityArrows == null || _velocityPoints == null)
		{
			return;
		}
		float f = ((_max.Count != 0) ? velocity2 : velocity);
		int num11 = (int)Mathf.Sign(f);
		float num12 = Mathf.Abs(f);
		int num13 = 0;
		int num14 = Mathf.Min(_velocityArrows.Length, _velocityPoints.Length);
		for (int j = 0; j < num14 && !(_velocityPoints[j] >= num12); j++)
		{
			num13++;
		}
		for (int k = 0; k < _velocityArrows.Length; k++)
		{
			_velocityArrows[k].alpha = ((k >= num13) ? 0f : 1f);
			if (k > 0)
			{
				_velocityArrows[k].depth = _velocityArrows[k - 1].depth - num11;
			}
			_velocityArrows[k].transform.localEulerAngles = Vector3.forward * 180f * ((num11 <= 0) ? 1 : 0);
		}
	}
}
