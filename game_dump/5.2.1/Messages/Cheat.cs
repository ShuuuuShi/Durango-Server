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
		unpacker.Read();
		Cheat result = default(Cheat);
		result._Cheat = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<Cheat _Cheat=" + _Cheat + ">";
	}
}
