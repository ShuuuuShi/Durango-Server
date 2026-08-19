using MsgPack;

namespace Messages;

public struct Tool_Collectibles
{
	public const uint TypeCode = 328u;

	public Pair<string, string>[] Collectibles;

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
			if (val.Collectibles[i].Item1 == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Collectibles[i].Item1);
			}
			packer.PackString(val.Collectibles[i].Item2);
		}
	}

	public static Tool_Collectibles Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Tool_Collectibles result = default(Tool_Collectibles);
		result.Collectibles = new Pair<string, string>[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			string item = unpacker.LastReadData.AsString();
			unpacker.Read();
			string item2 = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			ref Pair<string, string> reference = ref result.Collectibles[i];
			reference = new Pair<string, string>(item, item2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Tool_Collectibles Collectibles={Collectibles}>";
	}
}
