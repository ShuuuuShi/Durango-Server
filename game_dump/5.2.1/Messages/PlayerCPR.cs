using MsgPack;

namespace Messages;

public struct PlayerCPR
{
	public const uint TypeCode = 1001u;

	public double SentAt;

	public string RescuerId;

	public string TargetId;

	public string State;

	public static void Pack(Packer packer, PlayerCPR val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(1001u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.SentAt);
		if (val.RescuerId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RescuerId);
		}
		if (val.TargetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TargetId);
		}
		if (val.State == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.State);
		}
	}

	public static PlayerCPR Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlayerCPR result = default(PlayerCPR);
		result.SentAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.RescuerId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TargetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.State = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerCPR SentAt={SentAt} RescuerId={RescuerId} TargetId={TargetId} State={State}>";
	}
}
