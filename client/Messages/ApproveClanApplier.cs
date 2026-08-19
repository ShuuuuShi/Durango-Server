using MsgPack;

namespace Messages;

public struct ApproveClanApplier
{
	public const uint TypeCode = 3657u;

	public string EntityId;

	public static void Pack(Packer packer, ApproveClanApplier val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3657u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
	}

	public static ApproveClanApplier Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ApproveClanApplier result = default(ApproveClanApplier);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<ApproveClanApplier EntityId={EntityId}>";
	}
}
