using MsgPack;

namespace Messages;

public struct SetResurrectionRewards
{
	public const uint TypeCode = 133u;

	public string[] ItemIds;

	public static void Pack(Packer packer, SetResurrectionRewards val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(133u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ItemIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ItemIds.Length);
		for (int i = 0; i < val.ItemIds.Length; i++)
		{
			if (val.ItemIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.ItemIds[i]);
			}
		}
	}

	public static SetResurrectionRewards Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SetResurrectionRewards result = default(SetResurrectionRewards);
		result.ItemIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ItemIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("<SetResurrectionRewards ItemIds={0}>", ItemIds);
	}
}
