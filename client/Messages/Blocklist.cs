using MsgPack;

namespace Messages;

public struct Blocklist
{
	public const uint TypeCode = 4019u;

	public string[] EntityIds;

	public static void Pack(Packer packer, Blocklist val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(4019u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.EntityIds.Length);
		for (int i = 0; i < val.EntityIds.Length; i++)
		{
			if (val.EntityIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.EntityIds[i]);
			}
		}
	}

	public static Blocklist Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Blocklist result = default(Blocklist);
		result.EntityIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.EntityIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("<Blocklist EntityIds={0}>", EntityIds);
	}
}
