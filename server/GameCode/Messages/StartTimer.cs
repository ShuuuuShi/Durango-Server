using MsgPack;

namespace Messages;

public struct StartTimer
{
	public const uint TypeCode = 124u;

	public string EntityId;

	public string Subject;

	public float Current;

	public float Time;

	public float AdditionalTime;

	public static void Pack(Packer packer, StartTimer val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(124u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.Subject == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Subject);
		}
		packer.Pack(val.Current);
		packer.Pack(val.Time);
		packer.Pack(val.AdditionalTime);
	}

	public static StartTimer Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		StartTimer result = default(StartTimer);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Subject = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Current = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.Time = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.AdditionalTime = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<StartTimer EntityId={EntityId} Subject={Subject} Current={Current} Time={Time} AdditionalTime={AdditionalTime}>";
	}
}
