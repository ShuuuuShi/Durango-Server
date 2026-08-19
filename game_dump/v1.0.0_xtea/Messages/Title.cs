using MsgPack;

namespace Messages;

public struct Title
{
	public const uint TypeCode = 334u;

	public ulong EntityId;

	public string _Title;

	public static void Pack(Packer packer, Title val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(334u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		packer.PackString(val._Title);
	}

	public static Title Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Title result = default(Title);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		result._Title = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<Title EntityId={EntityId} _Title={_Title}>";
	}
}
