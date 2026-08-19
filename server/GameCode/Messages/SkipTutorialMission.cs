using MsgPack;

namespace Messages;

public struct SkipTutorialMission
{
	public const uint TypeCode = 3633u;

	public string MissionId;

	public static void Pack(Packer packer, SkipTutorialMission val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3633u);
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

	public static SkipTutorialMission Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SkipTutorialMission result = default(SkipTutorialMission);
		result.MissionId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SkipTutorialMission MissionId={MissionId}>";
	}
}
