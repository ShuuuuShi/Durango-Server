using MsgPack;

namespace Messages;

public struct TargetChanged
{
	public const uint TypeCode = 3787u;

	public string EntityId;

	public double EventAt;

	public string TargetId;

	public static void Pack(Packer packer, TargetChanged val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3787u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.EventAt);
		if (val.TargetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TargetId);
		}
	}

	public static TargetChanged Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TargetChanged result = default(TargetChanged);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EventAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.TargetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<TargetChanged EntityId={EntityId} EventAt={EventAt} TargetId={TargetId}>";
	}
}
