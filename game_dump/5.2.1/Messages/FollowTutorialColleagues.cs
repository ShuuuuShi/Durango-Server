using MsgPack;

namespace Messages;

public struct FollowTutorialColleagues
{
	public const uint TypeCode = 2419u;

	public string[] Colleagues;

	public static void Pack(Packer packer, FollowTutorialColleagues val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2419u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Colleagues == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Colleagues.Length);
		for (int i = 0; i < val.Colleagues.Length; i++)
		{
			if (val.Colleagues[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Colleagues[i]);
			}
		}
	}

	public static FollowTutorialColleagues Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		FollowTutorialColleagues result = default(FollowTutorialColleagues);
		result.Colleagues = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Colleagues[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		object[] colleagues = Colleagues;
		return string.Format("<FollowTutorialColleagues Colleagues={0}>", colleagues);
	}
}
