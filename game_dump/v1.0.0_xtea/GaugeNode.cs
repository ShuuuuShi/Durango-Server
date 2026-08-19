using System.Collections.Generic;
using MsgPack;

public struct GaugeNode
{
	public double Time;

	public float Value;

	public GaugeNode(double time, float value)
	{
		Time = time;
		Value = value;
	}

	public GaugeNode(IList<MessagePackObject> list)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (list == null || list.Count < 2)
		{
			Time = 0.0;
			Value = 0f;
			return;
		}
		MessagePackObject val = list[0];
		Time = ((MessagePackObject)(ref val)).AsDouble();
		MessagePackObject val2 = list[1];
		Value = ((MessagePackObject)(ref val2)).AsSingle();
	}
}
