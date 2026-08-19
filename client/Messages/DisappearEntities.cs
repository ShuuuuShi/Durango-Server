using MsgPack;

namespace Messages;

public struct DisappearEntities
{
	public const uint TypeCode = 324987u;

	public string[] EntityIds;

	public static void Pack(Packer packer, DisappearEntities val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(324987u);
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

	public static DisappearEntities Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		DisappearEntities result = default(DisappearEntities);
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
		return string.Format("<DisappearEntities EntityIds={0}>", EntityIds);
	}
}
