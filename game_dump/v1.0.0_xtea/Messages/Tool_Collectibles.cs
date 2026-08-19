using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Tool_Collectibles
{
	public const uint TypeCode = 328u;

	public KeyValuePair<string, string>[] Collectibles;

	public static void Pack(Packer packer, Tool_Collectibles val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(328u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Collectibles == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Collectibles.Length);
		for (int i = 0; i < val.Collectibles.Length; i++)
		{
			packer.PackArrayHeader(2);
			if (val.Collectibles[i].Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Collectibles[i].Key);
			}
			packer.PackString(val.Collectibles[i].Value);
		}
	}

	public static Tool_Collectibles Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Tool_Collectibles result = default(Tool_Collectibles);
		result.Collectibles = new KeyValuePair<string, string>[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData2)).AsString();
			unpacker.Read();
			string value = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			ref KeyValuePair<string, string> reference = ref result.Collectibles[i];
			reference = new KeyValuePair<string, string>(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Tool_Collectibles Collectibles={Collectibles}>";
	}
}
