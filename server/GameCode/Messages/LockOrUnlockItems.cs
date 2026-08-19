using MsgPack;

namespace Messages;

public struct LockOrUnlockItems
{
	public const uint TypeCode = 3497u;

	public bool Lock;

	public string[] ItemIds;

	public static void Pack(Packer packer, LockOrUnlockItems val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3497u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Lock);
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

	public static LockOrUnlockItems Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		LockOrUnlockItems result = default(LockOrUnlockItems);
		result.Lock = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
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
		return $"<LockOrUnlockItems Lock={Lock} ItemIds={ItemIds}>";
	}
}
