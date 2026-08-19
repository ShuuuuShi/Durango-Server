using MsgPack;

namespace Messages;

public struct RadioId
{
	public const uint TypeCode = 2600u;

	public string Name;

	public int Freq;

	public static void Pack(Packer packer, RadioId val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2600u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
		packer.Pack(val.Freq);
	}

	public static RadioId Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RadioId result = default(RadioId);
		result.Name = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Freq = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<RadioId Name={Name} Freq={Freq}>";
	}
}
