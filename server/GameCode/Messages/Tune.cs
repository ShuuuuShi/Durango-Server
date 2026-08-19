using MsgPack;

namespace Messages;

public struct Tune
{
	public const uint TypeCode = 2400u;

	public string EntityId;

	public string SessionToken;

	public double SyncedAt;

	public static void Pack(Packer packer, Tune val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2400u);
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
		if (val.SessionToken == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SessionToken);
		}
		packer.Pack(val.SyncedAt);
	}

	public static Tune Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Tune result = default(Tune);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.SessionToken = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.SyncedAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Tune EntityId={EntityId} SessionToken={SessionToken} SyncedAt={SyncedAt}>";
	}
}
