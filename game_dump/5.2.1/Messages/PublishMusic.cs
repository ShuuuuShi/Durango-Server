using MsgPack;

namespace Messages;

public struct PublishMusic
{
	public const uint TypeCode = 47852557u;

	public int Slot;

	public static void Pack(Packer packer, PublishMusic val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(47852557u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Slot);
	}

	public static PublishMusic Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PublishMusic result = default(PublishMusic);
		result.Slot = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<PublishMusic Slot={Slot}>";
	}
}
