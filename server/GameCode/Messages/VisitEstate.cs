using MsgPack;
using Shared.Economy;
using Shared.Estate;

namespace Messages;

public struct VisitEstate
{
	public const uint TypeCode = 2104u;

	public string OwnerId;

	public string RegionId;

	public OwnerType OwnerType;

	public Money? Cost;

	public static void Pack(Packer packer, VisitEstate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2104u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.OwnerId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.OwnerId);
		}
		if (val.RegionId == null)
		{
			packer.PackNull();
		}
		else if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		packer.Pack((int)val.OwnerType);
		if (!val.Cost.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.Cost.Value.Amount);
		packer.Pack((int)val.Cost.Value.Currency);
	}

	public static VisitEstate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		VisitEstate result = default(VisitEstate);
		result.OwnerId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RegionId = null;
		}
		else
		{
			string regionId = unpacker.LastReadData.AsString();
			result.RegionId = regionId;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 4 < num)
		{
			result.OwnerType = OwnerType.Invalid;
		}
		else
		{
			result.OwnerType = (OwnerType)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Cost = null;
		}
		else
		{
			unpacker.ReadInt32(out var result2);
			unpacker.ReadInt32(out var result3);
			Money value = new Money(result2, (Currency)result3);
			result.Cost = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<VisitEstate OwnerId={OwnerId} RegionId={RegionId} OwnerType={OwnerType} Cost={Cost}>";
	}
}
