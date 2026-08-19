using System.Collections.Generic;
using MsgPack;
using Shared.Survival;

namespace Messages;

public struct FatigueVelocities
{
	public const uint TypeCode = 318u;

	public Dictionary<FatigueCategory, float> Velocities;

	public string FatigueEffect;

	public static void Pack(Packer packer, FatigueVelocities val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(318u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Velocities == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Velocities.Count);
			foreach (KeyValuePair<FatigueCategory, float> velocity in val.Velocities)
			{
				packer.Pack((int)velocity.Key);
				packer.Pack(velocity.Value);
			}
		}
		if (val.FatigueEffect == null)
		{
			packer.PackNull();
		}
		else if (val.FatigueEffect == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.FatigueEffect);
		}
	}

	public static FatigueVelocities Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		FatigueVelocities result = default(FatigueVelocities);
		result.Velocities = new Dictionary<FatigueCategory, float>(num, default(FatigueCategoryComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
			FatigueCategory key = ((num2 >= 0 && 8 >= num2) ? ((FatigueCategory)num2) : FatigueCategory.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			float value = ((MessagePackObject)(ref lastReadData3)).AsSingle();
			result.Velocities.Add(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.FatigueEffect = null;
		}
		else
		{
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			string fatigueEffect = ((MessagePackObject)(ref lastReadData5)).AsString();
			result.FatigueEffect = fatigueEffect;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FatigueVelocities Velocities={Velocities} FatigueEffect={FatigueEffect}>";
	}
}
