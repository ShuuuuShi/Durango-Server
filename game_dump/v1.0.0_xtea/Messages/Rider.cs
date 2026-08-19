using MsgPack;

namespace Messages;

public struct Rider
{
	public const uint TypeCode = 800u;

	public ulong EntityId;

	public ulong? VehicleId;

	public ushort? VehicleEntityType;

	public string VehicleName;

	public bool VehicleSpawned;

	public bool IsBoarding;

	public ushort? Speed;

	public float? PlaybackRate;

	public static void Pack(Packer packer, Rider val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(9);
			packer.Pack(800u);
		}
		else
		{
			packer.PackArrayHeader(8);
		}
		packer.Pack(val.EntityId);
		if (!val.VehicleId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.VehicleId.Value);
		}
		if (!val.VehicleEntityType.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.VehicleEntityType.Value);
		}
		if (val.VehicleName == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.VehicleName);
		}
		packer.Pack(val.VehicleSpawned);
		packer.Pack(val.IsBoarding);
		if (!val.Speed.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Speed.Value);
		}
		if (!val.PlaybackRate.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.PlaybackRate.Value);
		}
	}

	public static Rider Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Rider result = default(Rider);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.VehicleId = null;
		}
		else
		{
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			ulong value = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
			result.VehicleId = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.VehicleEntityType = null;
		}
		else
		{
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			ushort value2 = ((MessagePackObject)(ref lastReadData5)).AsUInt16();
			result.VehicleEntityType = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData6)).IsNil)
		{
			result.VehicleName = null;
		}
		else
		{
			string vehicleName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.VehicleName = vehicleName;
		}
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.VehicleSpawned = ((MessagePackObject)(ref lastReadData7)).AsBoolean();
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		result.IsBoarding = ((MessagePackObject)(ref lastReadData8)).AsBoolean();
		unpacker.Read();
		MessagePackObject lastReadData9 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData9)).IsNil)
		{
			result.Speed = null;
		}
		else
		{
			MessagePackObject lastReadData10 = unpacker.LastReadData;
			ushort value3 = ((MessagePackObject)(ref lastReadData10)).AsUInt16();
			result.Speed = value3;
		}
		unpacker.Read();
		MessagePackObject lastReadData11 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData11)).IsNil)
		{
			result.PlaybackRate = null;
		}
		else
		{
			MessagePackObject lastReadData12 = unpacker.LastReadData;
			float value4 = ((MessagePackObject)(ref lastReadData12)).AsSingle();
			result.PlaybackRate = value4;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Rider EntityId={EntityId} VehicleId={VehicleId} VehicleEntityType={VehicleEntityType} VehicleName={VehicleName} VehicleSpawned={VehicleSpawned} IsBoarding={IsBoarding} Speed={Speed} PlaybackRate={PlaybackRate}>";
	}
}
