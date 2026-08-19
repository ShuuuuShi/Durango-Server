using System;
using System.Collections.Generic;
using UnityEngine;

public class HyperGaugeViewer : MonoBehaviour
{
	public delegate int ToIntDelegate(float value);

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _mainUpper;

	[SerializeField]
	private UISprite _subUpper;

	[SerializeField]
	private UILabel _label;

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

	private readonly List<GaugeNode> _current = new List<GaugeNode>();

	private readonly List<GaugeNode> _max = new List<GaugeNode>();

	private Vector2 _range;

	private int _lastGaugeCur = int.MaxValue;

	private int _lastGaugeMax = int.MaxValue;

	private float _gaugeScale;

	public ToIntDelegate ToIntFunction;

	private void Awake()
	{
		if (_velocityPoints != null)
		{
			Array.Sort(_velocityPoints);
		}
	}

	public void Set(Gauge gauge, bool smooth = true, float gaugeScale = 1f)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
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
			if ((Object)(object)_mainUpper != (Object)null)
			{
				((Component)_mainUpper).gameObject.SetActive(false);
			}
			if ((Object)(object)_subUpper != (Object)null)
			{
				((Component)_subUpper).gameObject.SetActive(false);
			}
		}
		else
		{
			if ((Object)(object)_mainUpper != (Object)null)
			{
				((Component)_mainUpper).gameObject.SetActive(true);
			}
			if ((Object)(object)_subUpper != (Object)null)
			{
				((Component)_subUpper).gameObject.SetActive(_max != null);
			}
		}
		_gaugeScale = gaugeScale;
		Update();
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

	private void Update()
	{
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		if (_current.Count == 0)
		{
			return;
		}
		Gauge.CurrentValueAndVelocity(_current, Gauge.CurrentTime, out var value, out var velocity);
		Gauge.CurrentValueAndVelocity(_max, Gauge.CurrentTime, out var _, out var velocity2);
		float y = _range.y;
		if ((Object)(object)_mainUpper != (Object)null)
		{
			float num = Mathf.Clamp01(value / y);
			if (_mainUpper.type == UIBasicSprite.Type.Filled)
			{
				_mainUpper.fillAmount = num;
			}
			else if (num > 0f)
			{
				_mainUpper.alpha = 1f;
				_mainUpper.width = (int)((float)_background.width * num);
			}
			else
			{
				_mainUpper.alpha = 0f;
			}
		}
		if (_max.Count > 0 && (Object)(object)_subUpper != (Object)null)
		{
			Gauge.CurrentValueAndVelocity(_max, Gauge.CurrentTime, out var value3, out var _);
			float num2 = Mathf.Clamp01(value3 / _range.y);
			if (_subUpper.type == UIBasicSprite.Type.Filled)
			{
				_subUpper.fillAmount = num2;
			}
			else if (num2 > 0f)
			{
				_subUpper.alpha = 1f;
				_subUpper.width = (int)((float)_background.width * num2);
			}
			else
			{
				_subUpper.alpha = 0f;
			}
		}
		if ((Object)(object)_label != (Object)null)
		{
			if (ToIntFunction == null)
			{
				ToIntFunction = Mathf.FloorToInt;
			}
			int num3 = ToIntFunction(value * _gaugeScale);
			int num4 = ToIntFunction(y * _gaugeScale);
			if (num3 != _lastGaugeCur || num4 != _lastGaugeMax)
			{
				Color val = Color.clear;
				if (_labelColors != null && _labelColorRatios != null)
				{
					float num5 = value / y;
					int num6 = Mathf.Min(_labelColors.Length, _labelColorRatios.Length);
					for (int i = 0; i < num6; i++)
					{
						if (_labelColorRatios[i] >= num5)
						{
							val = _labelColors[i];
							break;
						}
					}
					if (val == Color.clear && _labelColors.Length > num6)
					{
						val = _labelColors[num6];
					}
				}
				if (val == Color.clear)
				{
					_label.text = $"{num3} [b7b7b7]/[-] {num4}";
				}
				else
				{
					_label.text = string.Format("{2}{0}[-] [b7b7b7]/[-] {1}", num3, num4, UIManager.ColorBBCode(val));
				}
				_lastGaugeCur = num3;
				_lastGaugeMax = num4;
			}
		}
		if (_velocityArrows == null || _velocityPoints == null)
		{
			return;
		}
		float num7 = ((_max.Count != 0) ? velocity2 : velocity);
		int num8 = (int)Mathf.Sign(num7);
		float num9 = Mathf.Abs(num7);
		int num10 = 0;
		int num11 = Mathf.Min(_velocityArrows.Length, _velocityPoints.Length);
		for (int j = 0; j < num11 && !(_velocityPoints[j] >= num9); j++)
		{
			num10++;
		}
		for (int k = 0; k < _velocityArrows.Length; k++)
		{
			_velocityArrows[k].alpha = ((k >= num10) ? 0f : 1f);
			if (k > 0)
			{
				_velocityArrows[k].depth = _velocityArrows[k - 1].depth - num8;
			}
			((Component)_velocityArrows[k]).transform.localEulerAngles = Vector3.forward * 180f * (float)((num8 <= 0) ? 1 : 0);
		}
	}
}
