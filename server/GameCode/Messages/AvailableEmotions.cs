using MsgPack;

namespace Messages;

public struct AvailableEmotions
{
	public const uint TypeCode = 9592635u;

	public string[] Motions;

	public string[] Emoticons;

	public static void Pack(Packer packer, AvailableEmotions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(9592635u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Motions == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Motions.Length);
			for (int i = 0; i < val.Motions.Length; i++)
			{
				if (val.Motions[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Motions[i]);
				}
			}
		}
		if (val.Emoticons == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Emoticons.Length);
		for (int j = 0; j < val.Emoticons.Length; j++)
		{
			if (val.Emoticons[j] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Emoticons[j]);
			}
		}
	}

	public static AvailableEmotions Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AvailableEmotions result = default(AvailableEmotions);
		result.Motions = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Motions[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Emoticons = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.Emoticons[j] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AvailableEmotions Motions={Motions} Emoticons={Emoticons}>";
	}
}
