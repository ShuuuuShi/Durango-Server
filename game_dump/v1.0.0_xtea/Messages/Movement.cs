using MsgPack;

namespace Messages;

public struct Movement
{
	public string MotionName;

	public byte MotionOption;

	public float PlaybackRate;

	public float RotSpeed;

	public Location[] Path;

	public static void Pack(Packer packer, Movement val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		if (val.MotionName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.MotionName);
		}
		packer.Pack(val.MotionOption);
		packer.Pack(val.PlaybackRate);
		packer.Pack(val.RotSpeed);
		if (val.Path == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Path.Length);
		for (int i = 0; i < val.Path.Length; i++)
		{
			Location.Pack(packer, val.Path[i]);
		}
	}

	public static Movement Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Movement result = default(Movement);
		result.MotionName = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.MotionOption = ((MessagePackObject)(ref lastReadData2)).AsByte();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.PlaybackRate = ((MessagePackObject)(ref lastReadData3)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.RotSpeed = ((MessagePackObject)(ref lastReadData4)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		result.Path = new Location[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Location reference = ref result.Path[i];
			reference = Location.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Movement MotionName={MotionName} MotionOption={MotionOption} PlaybackRate={PlaybackRate} RotSpeed={RotSpeed} Path={Path}>";
	}
}
