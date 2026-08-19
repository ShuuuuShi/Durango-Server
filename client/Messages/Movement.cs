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
		unpacker.Read();
		Movement result = default(Movement);
		result.MotionName = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.MotionOption = unpacker.LastReadData.AsByte();
		unpacker.Read();
		result.PlaybackRate = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.RotSpeed = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
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
