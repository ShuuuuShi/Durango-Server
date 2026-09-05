using MsgPack;

namespace Messages;

public struct CheckSequenceMissionCleared
{
	public const uint TypeCode = 3631u;

	public string MissionId;

	public static void Pack(Packer packer, CheckSequenceMissionCleared val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3631u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.MissionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.MissionId);
		}
	}

	public static CheckSequenceMissionCleared Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CheckSequenceMissionCleared result = default(CheckSequenceMissionCleared);
		result.MissionId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<CheckSequenceMissionCleared MissionId={MissionId}>";
	}
}
