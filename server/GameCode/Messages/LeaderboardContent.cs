using MsgPack;

namespace Messages;

public struct LeaderboardContent
{
	public string UserId;

	public double At;

	public int? Damage;

	public static void Pack(Packer packer, LeaderboardContent val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.UserId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.UserId);
		}
		packer.Pack(val.At);
		if (!val.Damage.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Damage.Value);
		}
	}

	public static LeaderboardContent Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		LeaderboardContent result = default(LeaderboardContent);
		result.UserId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.At = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Damage = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.Damage = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<LeaderboardContent UserId={UserId} At={At} Damage={Damage}>";
	}
}
