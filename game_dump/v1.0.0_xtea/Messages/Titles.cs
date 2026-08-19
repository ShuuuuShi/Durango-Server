using MsgPack;

namespace Messages;

public struct Titles
{
	public const uint TypeCode = 2045u;

	public string[] TitleIds;

	public static void Pack(Packer packer, Titles val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2045u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.TitleIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.TitleIds.Length);
		for (int i = 0; i < val.TitleIds.Length; i++)
		{
			if (val.TitleIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.TitleIds[i]);
			}
		}
	}

	public static Titles Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Titles result = default(Titles);
		result.TitleIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string[] titleIds = result.TitleIds;
			int num2 = i;
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			titleIds[num2] = ((MessagePackObject)(ref lastReadData2)).AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("<Titles TitleIds={0}>", TitleIds);
	}
}
