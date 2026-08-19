using MsgPack;

namespace Messages;

public struct ClaimedDlc
{
	public const uint TypeCode = 841262u;

	public string[] DlcIds;

	public static void Pack(Packer packer, ClaimedDlc val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(841262u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.DlcIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.DlcIds.Length);
		for (int i = 0; i < val.DlcIds.Length; i++)
		{
			if (val.DlcIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.DlcIds[i]);
			}
		}
	}

	public static ClaimedDlc Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ClaimedDlc result = default(ClaimedDlc);
		result.DlcIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.DlcIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		object[] dlcIds = DlcIds;
		return string.Format("<ClaimedDlc DlcIds={0}>", dlcIds);
	}
}
