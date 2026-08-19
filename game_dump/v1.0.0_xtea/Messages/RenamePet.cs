using MsgPack;

namespace Messages;

public struct RenamePet
{
	public const uint TypeCode = 804u;

	public ulong PetId;

	public string Name;

	public static void Pack(Packer packer, RenamePet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(804u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.PetId);
		if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
	}

	public static RenamePet Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RenamePet result = default(RenamePet);
		result.PetId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Name = ((MessagePackObject)(ref lastReadData2)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<RenamePet PetId={PetId} Name={Name}>";
	}
}
