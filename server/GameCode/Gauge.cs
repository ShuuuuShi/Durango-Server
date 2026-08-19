using System;
using System.Collections.Generic;
using MsgPack;
using UnityEngine;

public class Gauge
{
	public enum Type
	{
		Round,
		Floor,
		Ceil
	}

	private float _max;

	private float _min;

	public static double CurrentTime => Durango.Utils.Times.UnixTimeNow();

	public Gauge MaxGauge { get; private set; }

	public Gauge MinGauge { get; private set; }

	public GaugeNode[] Determination { get; private set; }

	public Gauge()
	{
	}

	public Gauge(GaugeNode[] determination)
		: this()
	{
		Determination = determination;
	}

	public Gauge(float max, float min, GaugeNode[] determination)
		: this(determination)
	{
		_max = max;
		_min = min;
	}

	public Gauge(Gauge maxGauge, Gauge minGauge, GaugeNode[] determination)
		: this(determination)
	{
		MaxGauge = maxGauge;
		MinGauge = minGauge;
	}

	public Gauge(float max, Gauge minGauge, GaugeNode[] determination)
		: this(determination)
	{
		_max = max;
		MinGauge = minGauge;
	}

	public Gauge(Gauge maxGauge, float min, GaugeNode[] determination)
		: this(determination)
	{
		MaxGauge = maxGauge;
		_min = min;
	}

	public float Max(double at)
	{
		if (MaxGauge != null)
		{
			return MaxGauge.Get(at);
		}
		return _max;
	}

	public float Min(double at)
	{
		if (MinGauge != null)
		{
			return MinGauge.Get(at);
		}
		return _min;
	}

	public float Max()
	{
		return Max(CurrentTime);
	}

	public float Min()
	{
		return Min(CurrentTime);
	}

	public float RealMax()
	{
		if (MaxGauge != null)
		{
			return MaxGauge.RealMax();
		}
		return _max;
	}

	public float RealMin()
	{
		if (MinGauge != null)
		{
			return MinGauge.RealMin();
		}
		return _min;
	}

	private void CurrentValueAndVelocity(double at, out float value, out float velocity)
	{
		CurrentValueAndVelocity(Determination, at, out value, out velocity);
	}

	public static void CurrentValueAndVelocity(IList<GaugeNode> nodes, double at, out float value, out float velocity)
	{
		if (nodes == null)
		{
			value = 0f;
			velocity = 0f;
			return;
		}
		GaugeNode gaugeNode = default(GaugeNode);
		int i = 0;
		for (int count = nodes.Count; i < count; i++)
		{
			gaugeNode = nodes[i];
			if (!(at >= gaugeNode.Time))
			{
				if (i == 0)
				{
					break;
				}
				GaugeNode gaugeNode2 = nodes[i - 1];
				float num = (float)((at - gaugeNode2.Time) / (gaugeNode.Time - gaugeNode2.Time));
				float num2 = gaugeNode.Value - gaugeNode2.Value;
				value = gaugeNode2.Value + num * num2;
				velocity = num2 / (float)(gaugeNode.Time - gaugeNode2.Time);
				return;
			}
		}
		value = gaugeNode.Value;
		velocity = 0f;
	}

	public float Get(double at)
	{
		CurrentValueAndVelocity(at, out var value, out var _);
		return value;
	}

	public float Get()
	{
		return Get(CurrentTime);
	}

	public float Velocity(double at)
	{
		CurrentValueAndVelocity(at, out var _, out var velocity);
		return velocity;
	}

	public float Velocity()
	{
		return Velocity(CurrentTime);
	}

	public float Goal()
	{
		return Determination[Determination.Length - 1].Value;
	}

	public float Ratio(double at)
	{
		float num = Max(at);
		float num2 = Min(at);
		if (num == 0f)
		{
			return 0f;
		}
		return (Get(at) - num2) / (num - num2);
	}

	public float Ratio()
	{
		return Ratio(CurrentTime);
	}

	public double When(float value, double? at = null)
	{
		return When(Determination, value, at);
	}

	public static double When(IList<GaugeNode> nodes, float value, double? at = null)
	{
		if (nodes == null || nodes.Count == 0)
		{
			return 0.0;
		}
		if (Mathf.Approximately(nodes[0].Value, value) && (!at.HasValue || nodes[0].Time > at.Value))
		{
			return nodes[0].Time;
		}
		for (int i = 0; i < nodes.Count - 1; i++)
		{
			GaugeNode gaugeNode = nodes[i];
			GaugeNode gaugeNode2 = nodes[i + 1];
			if ((gaugeNode.Value < value && value <= gaugeNode2.Value) || (gaugeNode.Value > value && value >= gaugeNode2.Value))
			{
				double num = (value - gaugeNode.Value) / (gaugeNode2.Value - gaugeNode.Value);
				num = (gaugeNode2.Time / 100.0 - gaugeNode.Time / 100.0) * num;
				num = gaugeNode.Time + num * 100.0;
				if (!at.HasValue || num > at.Value)
				{
					return num;
				}
			}
		}
		return 0.0;
	}

	public override string ToString()
	{
		return ToString(Type.Floor);
	}

	public string ToString(Type type)
	{
		double currentTime = CurrentTime;
		float num = Max(currentTime);
		float num2 = Min(currentTime);
		float f = Get(currentTime);
		switch (type)
		{
		case Type.Round:
			f = Mathf.Round(f);
			break;
		case Type.Ceil:
			f = Mathf.Ceil(f);
			break;
		case Type.Floor:
			f = Mathf.Floor(f);
			break;
		}
		if (num > num2)
		{
			if (MaxGauge == null)
			{
				return string.Format("{0}/{1}", f.ToString("0"), num.ToString("0"));
			}
			return string.Format("{0}/{1}/{2}", f.ToString("0"), num.ToString("0"), RealMax());
		}
		return string.Format("{0}", f.ToString("0"));
	}

	public static double? GetNextChangedAt(IEnumerable<GaugeNode> nodes, double at, int term = 1)
	{
		if (nodes == null)
		{
			return null;
		}
		GaugeNode? gaugeNode = null;
		foreach (GaugeNode node in nodes)
		{
			if (at >= node.Time)
			{
				gaugeNode = node;
				continue;
			}
			if (!gaugeNode.HasValue)
			{
				return node.Time;
			}
			GaugeNode value = gaugeNode.Value;
			float num = (float)((at - value.Time) / (node.Time - value.Time));
			float num2 = node.Value - value.Value;
			float num3 = value.Value + num * num2;
			float num4 = num2 / (float)(node.Time - value.Time);
			double num5 = node.Time;
			float num6 = Mathf.Sign(num4);
			if (num6 > 0f)
			{
				num5 = Math.Min(num5, at + (double)(((float)((int)num3 + term) - num3) / num4));
			}
			else if (num6 < 0f)
			{
				num5 = Math.Min(num5, at + (double)((num3 - (float)(int)num3 - (float)(term - 1)) / num4));
			}
			return num5;
		}
		return null;
	}

	public static AnimationCurve ToAnimationCurve(Gauge gauge)
	{
		GaugeNode[] determination = gauge.Determination;
		int num = ((determination != null) ? determination.Length : 0);
		Keyframe[] array = new Keyframe[num];
		double currentTime = CurrentTime;
		for (int i = 0; i < num; i++)
		{
			ref Keyframe reference = ref array[i];
			reference = new Keyframe((float)(determination[i].Time - currentTime), determination[i].Value);
		}
		for (int j = 0; j < num - 1; j++)
		{
			float num2 = Mathf.Atan2(array[j + 1].value - array[j].value, array[j + 1].time - array[j].time);
			array[j].outTangent = num2;
			array[j + 1].inTangent = num2;
		}
		return new AnimationCurve(array);
	}

	public static void PackTo(Gauge gauge, Packer packer)
	{
		packer.PackArrayHeader(3);
		if (gauge == null)
		{
			packer.Pack(0);
			packer.Pack(0);
			packer.PackArrayHeader(0);
			return;
		}
		if (gauge.MaxGauge != null)
		{
			PackTo(gauge.MaxGauge, packer);
		}
		else
		{
			packer.Pack(gauge._max);
		}
		if (gauge.MinGauge != null)
		{
			PackTo(gauge.MinGauge, packer);
		}
		else
		{
			packer.Pack(gauge._min);
		}
		int size = KUtility.GetSize(gauge.Determination);
		packer.PackArrayHeader(size);
		for (int i = 0; i < size; i++)
		{
			GaugeNode gaugeNode = gauge.Determination[i];
			packer.PackArrayHeader(2);
			packer.Pack(gaugeNode.Time);
			packer.Pack(gaugeNode.Value);
		}
	}

	public static Gauge UnpackFrom(Unpacker unpacker)
	{
		Gauge gauge = new Gauge();
		unpacker.Read();
		if (unpacker.IsArrayHeader)
		{
			gauge.MaxGauge = UnpackFrom(unpacker);
		}
		else
		{
			gauge._max = unpacker.LastReadData.AsSingle();
		}
		unpacker.Read();
		if (unpacker.IsArrayHeader)
		{
			gauge.MinGauge = UnpackFrom(unpacker);
		}
		else
		{
			gauge._min = unpacker.LastReadData.AsSingle();
		}
		unpacker.ReadArrayLength(out var result);
		gauge.Determination = new GaugeNode[result];
		for (int i = 0; i < result; i++)
		{
			GaugeNode gaugeNode = default(GaugeNode);
			unpacker.Read();
			unpacker.ReadDouble(out gaugeNode.Time);
			unpacker.ReadSingle(out gaugeNode.Value);
			gauge.Determination[i] = gaugeNode;
		}
		return gauge;
	}
}
