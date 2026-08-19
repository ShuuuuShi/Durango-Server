using MsgPack;

namespace Messages;

public struct Welcome
{
	public const uint TypeCode = 22u;

	public ulong? UserId;

	public string Name;

	public Region Region;

	public Storage Storage;

	public static void Pack(Packer packer, Welcome val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(22u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (!val.UserId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.UserId.Value);
		}
		if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
		Region.Pack(packer, val.Region);
		Storage.Pack(packer, val.Storage);
	}

	public static Welcome Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Welcome result = default(Welcome);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.UserId = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			ulong value = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
			result.UserId = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Name = ((MessagePackObject)(ref lastReadData3)).AsString();
		unpacker.Read();
		result.Region = Region.Unpack(unpacker);
		unpacker.Read();
		result.Storage = Storage.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<Welcome UserId={UserId} Name={Name} Region={Region} Storage={Storage}>";
	}
}
