using MsgPack;

namespace Messages;

public struct AttachableAccessories
{
	public const uint TypeCode = 9823458u;

	public string[] Accessories;

	public static void Pack(Packer packer, AttachableAccessories val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(9823458u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Accessories == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Accessories.Length);
		for (int i = 0; i < val.Accessories.Length; i++)
		{
			if (val.Accessories[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Accessories[i]);
			}
		}
	}

	public static AttachableAccessories Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AttachableAccessories result = default(AttachableAccessories);
		result.Accessories = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Accessories[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		object[] accessories = Accessories;
		return string.Format("<AttachableAccessories Accessories={0}>", accessories);
	}
}
