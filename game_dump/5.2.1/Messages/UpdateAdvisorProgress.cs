using MsgPack;

namespace Messages;

public struct UpdateAdvisorProgress
{
	public const uint TypeCode = 3505u;

	public Pair<int, int> Progress;

	public static void Pack(Packer packer, UpdateAdvisorProgress val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3505u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.Progress.Item1);
		packer.Pack(val.Progress.Item2);
	}

	public static UpdateAdvisorProgress Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		unpacker.Read();
		int item = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int item2 = unpacker.LastReadData.AsInt32();
		UpdateAdvisorProgress result = default(UpdateAdvisorProgress);
		result.Progress = new Pair<int, int>(item, item2);
		return result;
	}

	public override string ToString()
	{
		return $"<UpdateAdvisorProgress Progress={Progress}>";
	}
}
