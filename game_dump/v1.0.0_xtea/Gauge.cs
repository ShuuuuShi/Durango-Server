using System.Collections.Generic;
using ItemSystem;
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

	public delegate double WhenDelegate(Gauge gauge);

	private float _max;

	private float _min;

	public static double CurrentTime => Connections.Frontend.GetPredictedServerTime();

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

	public Gauge(GaugeJson json)
	{
		_max = json.max;
		_min = json.min;
		Determination = new GaugeNode[1]
		{
			new GaugeNode(0.0, json.current)
		};
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
		float num3 = Get(currentTime);
		switch (type)
		{
		case Type.Round:
			num3 = Mathf.Round(num3);
			break;
		case Type.Ceil:
			num3 = Mathf.Ceil(num3);
			break;
		case Type.Floor:
			num3 = Mathf.Floor(num3);
			break;
		}
		if (num > num2)
		{
			if (MaxGauge == null)
			{
				return string.Format("{0}/{1}", num3.ToString("0"), num.ToString("0"));
			}
			return string.Format("{0}/{1}/{2}", num3.ToString("0"), num.ToString("0"), RealMax());
		}
		return string.Format("{0}", num3.ToString("0"));
	}

	public static AnimationCurve ToAnimationCurve(Gauge gauge)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		GaugeNode[] determination = gauge.Determination;
		int num = ((determination != null) ? determination.Length : 0);
		Keyframe[] array = (Keyframe[])(object)new Keyframe[num];
		double currentTime = CurrentTime;
		for (int i = 0; i < num; i++)
		{
			ref Keyframe reference = ref array[i];
			reference = new Keyframe((float)(determination[i].Time - currentTime), determination[i].Value);
		}
		for (int j = 0; j < num - 1; j++)
		{
			float num2 = Mathf.Atan2(((Keyframe)(ref array[j + 1])).value - ((Keyframe)(ref array[j])).value, ((Keyframe)(ref array[j + 1])).time - ((Keyframe)(ref array[j])).time);
			((Keyframe)(ref array[j])).outTangent = num2;
			((Keyframe)(ref array[j + 1])).inTangent = num2;
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
		PackerUnpackerExtensions.PackArray<GaugeNode>(packer, (IEnumerable<GaugeNode>)gauge.Determination);
	}

	public static Gauge UnpackFrom(Unpacker unpacker)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Gauge gauge = new Gauge();
		unpacker.Read();
		if (unpacker.IsArrayHeader)
		{
			gauge.MaxGauge = UnpackFrom(unpacker);
		}
		else
		{
			MessagePackObject lastReadData = unpacker.LastReadData;
			gauge._max = ((MessagePackObject)(ref lastReadData)).AsSingle();
		}
		unpacker.Read();
		if (unpacker.IsArrayHeader)
		{
			gauge.MinGauge = UnpackFrom(unpacker);
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			gauge._min = ((MessagePackObject)(ref lastReadData2)).AsSingle();
		}
		long num = default(long);
		unpacker.ReadArrayLength(ref num);
		gauge.Determination = new GaugeNode[num];
		for (int i = 0; i < num; i++)
		{
			GaugeNode gaugeNode = default(GaugeNode);
			unpacker.Read();
			unpacker.ReadDouble(ref gaugeNode.Time);
			unpacker.ReadSingle(ref gaugeNode.Value);
			gauge.Determination[i] = gaugeNode;
		}
		return gauge;
	}

	public static Gauge UnpackFromMap(MessagePackObjectDictionary map)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		Gauge gauge = new Gauge();
		if (map.ContainsKey(MessagePackObject.op_Implicit("max_gauge")))
		{
			MessagePackObject val = map[MessagePackObject.op_Implicit("max_gauge")];
			gauge.MaxGauge = UnpackFromMap(((MessagePackObject)(ref val)).AsDictionary());
		}
		else
		{
			MessagePackObject val2 = map[MessagePackObject.op_Implicit("max")];
			gauge._max = ((MessagePackObject)(ref val2)).AsSingle();
		}
		if (map.ContainsKey(MessagePackObject.op_Implicit("min_gauge")))
		{
			MessagePackObject val3 = map[MessagePackObject.op_Implicit("min_gauge")];
			gauge.MinGauge = UnpackFromMap(((MessagePackObject)(ref val3)).AsDictionary());
		}
		else
		{
			MessagePackObject val4 = map[MessagePackObject.op_Implicit("min")];
			gauge._min = ((MessagePackObject)(ref val4)).AsSingle();
		}
		MessagePackObject val5 = map[MessagePackObject.op_Implicit("determination")];
		IList<MessagePackObject> list = ((MessagePackObject)(ref val5)).AsList();
		gauge.Determination = new GaugeNode[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			ref GaugeNode reference = ref gauge.Determination[i];
			MessagePackObject val6 = list[i];
			reference = new GaugeNode(((MessagePackObject)(ref val6)).AsList());
		}
		return gauge;
	}
}
