using MsgPack;
using Shared.Guide;

namespace Messages;

public struct RequestReturnerGuideAction
{
	public const uint TypeCode = 3450984u;

	public ReturnerGuideAction Action;

	public static void Pack(Packer packer, RequestReturnerGuideAction val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3450984u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Action);
	}

	public static RequestReturnerGuideAction Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		RequestReturnerGuideAction result = default(RequestReturnerGuideAction);
		if (num < 0 || 2 < num)
		{
			result.Action = ReturnerGuideAction.Invalid;
		}
		else
		{
			result.Action = (ReturnerGuideAction)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RequestReturnerGuideAction Action={Action}>";
	}
}
