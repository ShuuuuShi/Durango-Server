using MsgPack;

namespace Messages;

public struct NotifyRegionCoOpTodoProceed
{
	public const uint TypeCode = 241003u;

	public string CoOpId;

	public Pair<int, int> Progress;

	public static void Pack(Packer packer, NotifyRegionCoOpTodoProceed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(241003u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.CoOpId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CoOpId);
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.Progress.Item1);
		packer.Pack(val.Progress.Item2);
	}

	public static NotifyRegionCoOpTodoProceed Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		NotifyRegionCoOpTodoProceed result = default(NotifyRegionCoOpTodoProceed);
		result.CoOpId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.Read();
		int item = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int item2 = unpacker.LastReadData.AsInt32();
		result.Progress = new Pair<int, int>(item, item2);
		return result;
	}

	public override string ToString()
	{
		return $"<NotifyRegionCoOpTodoProceed CoOpId={CoOpId} Progress={Progress}>";
	}
}
