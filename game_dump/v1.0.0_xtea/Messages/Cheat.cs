using MsgPack;

namespace Messages;

public struct Cheat
{
	public const uint TypeCode = 201u;

	public string _Cheat;

	public static void Pack(Packer packer, Cheat val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(201u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._Cheat == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val._Cheat);
		}
	}

	public static Cheat Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Cheat result = default(Cheat);
		result._Cheat = ((MessagePackObject)(ref lastReadData)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Cheat _Cheat={_Cheat}>";
	}
}
