using MsgPack;

namespace Messages;

public struct RegionCoOpTodo
{
	public string CoOpId;

	public Pair<string, Point2>? Notice;

	public Pair<int, int> Progress;

	public RewardInfo? Reward;

	public static void Pack(Packer packer, RegionCoOpTodo val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		if (val.CoOpId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CoOpId);
		}
		if (!val.Notice.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			if (val.Notice.Value.Item1 == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Notice.Value.Item1);
			}
			packer.PackArrayHeader(2);
			packer.Pack((ushort)val.Notice.Value.Item2.x);
			packer.Pack((ushort)val.Notice.Value.Item2.y);
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.Progress.Item1);
		packer.Pack(val.Progress.Item2);
		if (!val.Reward.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RewardInfo.Pack(packer, val.Reward.Value);
		}
	}

	public static RegionCoOpTodo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RegionCoOpTodo result = default(RegionCoOpTodo);
		result.CoOpId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Notice = null;
		}
		else
		{
			unpacker.Read();
			string item = unpacker.LastReadData.AsString();
			unpacker.Read();
			unpacker.ReadUInt16(out var result2);
			Point2 item2 = default(Point2);
			item2.x = result2;
			unpacker.ReadUInt16(out result2);
			item2.y = result2;
			Pair<string, Point2> value = new Pair<string, Point2>(item, item2);
			result.Notice = value;
		}
		unpacker.Read();
		unpacker.Read();
		int item3 = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int item4 = unpacker.LastReadData.AsInt32();
		result.Progress = new Pair<int, int>(item3, item4);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Reward = null;
		}
		else
		{
			RewardInfo value2 = RewardInfo.Unpack(unpacker);
			result.Reward = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RegionCoOpTodo CoOpId={CoOpId} Notice={Notice} Progress={Progress} Reward={Reward}>";
	}
}
