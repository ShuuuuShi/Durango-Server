using MsgPack;
using UnityEngine;

namespace Messages;

public struct Location
{
	public WorldPosition Position;

	public float Yaw;

	public double Time;

	public byte Floor;

	public static void Pack(Packer packer, Location val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		packer.PackArrayHeader(2);
		packer.Pack((uint)Mathf.RoundToInt(val.Position.x));
		packer.Pack((uint)Mathf.RoundToInt(val.Position.y));
		packer.Pack(val.Yaw);
		packer.Pack(val.Time);
		packer.Pack(val.Floor);
	}

	public static Location Unpack(Unpacker unpacker)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		Location result = default(Location);
		unpacker.ReadSingle(ref result.Position.x);
		unpacker.ReadSingle(ref result.Position.y);
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.Yaw = ((MessagePackObject)(ref lastReadData)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Time = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Floor = ((MessagePackObject)(ref lastReadData3)).AsByte();
		return result;
	}

	public override string ToString()
	{
		return $"<Location Position={Position} Yaw={Yaw} Time={Time} Floor={Floor}>";
	}
}
