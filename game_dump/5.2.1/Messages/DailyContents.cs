using MsgPack;

namespace Messages;

public struct DailyContents
{
	public float ReceiveAt;

	public double ValidUntil;

	public static void Pack(Packer packer, DailyContents val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.ReceiveAt);
		packer.Pack(val.ValidUntil);
	}

	public static DailyContents Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DailyContents result = default(DailyContents);
		result.ReceiveAt = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.ValidUntil = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<DailyContents ReceiveAt={ReceiveAt} ValidUntil={ValidUntil}>";
	}
}
