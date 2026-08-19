using MsgPack;
using Shared.Clan;

namespace Messages;

public struct AllySlot
{
	public string ClanId;

	public bool IsAlly;

	public double? AllySince;

	public AllySlotState State;

	public double? StateExpiresAt;

	public static void Pack(Packer packer, AllySlot val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		if (val.ClanId == null)
		{
			packer.PackNull();
		}
		else if (val.ClanId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClanId);
		}
		packer.Pack(val.IsAlly);
		if (!val.AllySince.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.AllySince.Value);
		}
		packer.Pack((int)val.State);
		if (!val.StateExpiresAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.StateExpiresAt.Value);
		}
	}

	public static AllySlot Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AllySlot result = default(AllySlot);
		if (unpacker.LastReadData.IsNil)
		{
			result.ClanId = null;
		}
		else
		{
			string clanId = unpacker.LastReadData.AsString();
			result.ClanId = clanId;
		}
		unpacker.Read();
		result.IsAlly = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.AllySince = null;
		}
		else
		{
			double value = unpacker.LastReadData.AsDouble();
			result.AllySince = value;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 3 < num)
		{
			result.State = AllySlotState.Invalid;
		}
		else
		{
			result.State = (AllySlotState)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.StateExpiresAt = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.StateExpiresAt = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AllySlot ClanId={ClanId} IsAlly={IsAlly} AllySince={AllySince} State={State} StateExpiresAt={StateExpiresAt}>";
	}
}
