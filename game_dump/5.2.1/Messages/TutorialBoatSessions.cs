using MsgPack;

namespace Messages;

public struct TutorialBoatSessions
{
	public const uint TypeCode = 2308u;

	public TutorialSession[] Sessions;

	public static void Pack(Packer packer, TutorialBoatSessions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2308u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Sessions == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Sessions.Length);
		for (int i = 0; i < val.Sessions.Length; i++)
		{
			TutorialSession.Pack(packer, val.Sessions[i]);
		}
	}

	public static TutorialBoatSessions Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		TutorialBoatSessions result = default(TutorialBoatSessions);
		result.Sessions = new TutorialSession[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref TutorialSession reference = ref result.Sessions[i];
			reference = TutorialSession.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TutorialBoatSessions Sessions={Sessions}>";
	}
}
