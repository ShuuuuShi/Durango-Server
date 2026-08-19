using MsgPack;

namespace Messages;

public struct CancelMission
{
	public const uint TypeCode = 3624u;

	public string MissionId;

	public static void Pack(Packer packer, CancelMission val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3624u);
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

	public static CancelMission Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CancelMission result = default(CancelMission);
		result.MissionId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<CancelMission MissionId={MissionId}>";
	}
}
