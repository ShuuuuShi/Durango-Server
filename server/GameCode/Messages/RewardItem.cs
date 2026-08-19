using MsgPack;

namespace Messages;

public struct RewardItem
{
	public string PrototypeId;

	public int Level;

	public int Count;

	public string NameGettext;

	public string ColorR;

	public string ColorG;

	public string ColorB;

	public static void Pack(Packer packer, RewardItem val, bool hint = false)
	{
		packer.PackArrayHeader(7);
		if (val.PrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PrototypeId);
		}
		packer.Pack(val.Level);
		packer.Pack(val.Count);
		if (val.NameGettext == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.NameGettext);
		}
		if (val.ColorR == null)
		{
			packer.PackNull();
		}
		else if (val.ColorR == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ColorR);
		}
		if (val.ColorG == null)
		{
			packer.PackNull();
		}
		else if (val.ColorG == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ColorG);
		}
		if (val.ColorB == null)
		{
			packer.PackNull();
		}
		else if (val.ColorB == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ColorB);
		}
	}

	public static RewardItem Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RewardItem result = default(RewardItem);
		result.PrototypeId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Count = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.NameGettext = null;
		}
		else
		{
			string nameGettext = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.NameGettext = nameGettext;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ColorR = null;
		}
		else
		{
			string colorR = unpacker.LastReadData.AsString();
			result.ColorR = colorR;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ColorG = null;
		}
		else
		{
			string colorG = unpacker.LastReadData.AsString();
			result.ColorG = colorG;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ColorB = null;
		}
		else
		{
			string colorB = unpacker.LastReadData.AsString();
			result.ColorB = colorB;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RewardItem PrototypeId={PrototypeId} Level={Level} Count={Count} NameGettext={NameGettext} ColorR={ColorR} ColorG={ColorG} ColorB={ColorB}>";
	}
}
