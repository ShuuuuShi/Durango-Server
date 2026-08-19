using MsgPack;

namespace Messages;

public struct SequenceMissionCleared
{
	public const uint TypeCode = 3632u;

	public string MissionId;

	public bool Cleared;

	public static void Pack(Packer packer, SequenceMissionCleared val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3632u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.MissionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.MissionId);
		}
		packer.Pack(val.Cleared);
	}

	public static SequenceMissionCleared Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SequenceMissionCleared result = default(SequenceMissionCleared);
		result.MissionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Cleared = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<SequenceMissionCleared MissionId={MissionId} Cleared={Cleared}>";
	}
}
