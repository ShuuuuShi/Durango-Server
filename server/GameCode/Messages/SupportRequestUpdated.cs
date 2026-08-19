using MsgPack;
using Shared.Faction;

namespace Messages;

public struct SupportRequestUpdated
{
	public const uint TypeCode = 978254u;

	public string RequestId;

	public int RemainCount;

	public FactionType FactionType;

	public double AvailableAt;

	public static void Pack(Packer packer, SupportRequestUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(978254u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.RequestId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RequestId);
		}
		packer.Pack(val.RemainCount);
		packer.Pack((int)val.FactionType);
		packer.Pack(val.AvailableAt);
	}

	public static SupportRequestUpdated Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SupportRequestUpdated result = default(SupportRequestUpdated);
		result.RequestId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RemainCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 101 < num)
		{
			result.FactionType = FactionType.Invalid;
		}
		else
		{
			result.FactionType = (FactionType)num;
		}
		unpacker.Read();
		result.AvailableAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<SupportRequestUpdated RequestId={RequestId} RemainCount={RemainCount} FactionType={FactionType} AvailableAt={AvailableAt}>";
	}
}
