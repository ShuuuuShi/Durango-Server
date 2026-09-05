using MsgPack;
using Shared.Estate;

namespace Messages;

public struct EstateLicense
{
	public const uint TypeCode = 2430u;

	public string EstateId;

	public OwnerType Type;

	public string OwnerId;

	public double ActivatedAt;

	public double? DepositRunsOutAt;

	public double? ExpiresAt;

	public Pair<int, double>? Deposit;

	public AccessRights? AccessRights;

	public int Size;

	public string RegionId;

	public Point2 Tile;

	public double? ProtectedUntil;

	public double? CycleStartsAt;

	public double? CycleEndsAt;

	public double? RemovableAt;

	public static void Pack(Packer packer, EstateLicense val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(16);
			packer.Pack(2430u);
		}
		else
		{
			packer.PackArrayHeader(15);
		}
		if (val.EstateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EstateId);
		}
		packer.Pack((int)val.Type);
		if (val.OwnerId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.OwnerId);
		}
		packer.Pack(val.ActivatedAt);
		if (!val.DepositRunsOutAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.DepositRunsOutAt.Value);
		}
		if (!val.ExpiresAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ExpiresAt.Value);
		}
		if (!val.Deposit.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.Deposit.Value.Item1);
			packer.Pack(val.Deposit.Value.Item2);
		}
		if (!val.AccessRights.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.AccessRights.Pack(packer, val.AccessRights.Value);
		}
		packer.Pack(val.Size);
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (!val.ProtectedUntil.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ProtectedUntil.Value);
		}
		if (!val.CycleStartsAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.CycleStartsAt.Value);
		}
		if (!val.CycleEndsAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.CycleEndsAt.Value);
		}
		if (!val.RemovableAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.RemovableAt.Value);
		}
	}

	public static EstateLicense Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		EstateLicense result = default(EstateLicense);
		result.EstateId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 4 < num)
		{
			result.Type = OwnerType.Invalid;
		}
		else
		{
			result.Type = (OwnerType)num;
		}
		unpacker.Read();
		result.OwnerId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ActivatedAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.DepositRunsOutAt = null;
		}
		else
		{
			double value = unpacker.LastReadData.AsDouble();
			result.DepositRunsOutAt = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ExpiresAt = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.ExpiresAt = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Deposit = null;
		}
		else
		{
			unpacker.Read();
			int item = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			double item2 = unpacker.LastReadData.AsDouble();
			Pair<int, double> value3 = new Pair<int, double>(item, item2);
			result.Deposit = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.AccessRights = null;
		}
		else
		{
			AccessRights value4 = Messages.AccessRights.Unpack(unpacker);
			result.AccessRights = value4;
		}
		unpacker.Read();
		result.Size = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ProtectedUntil = null;
		}
		else
		{
			double value5 = unpacker.LastReadData.AsDouble();
			result.ProtectedUntil = value5;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.CycleStartsAt = null;
		}
		else
		{
			double value6 = unpacker.LastReadData.AsDouble();
			result.CycleStartsAt = value6;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.CycleEndsAt = null;
		}
		else
		{
			double value7 = unpacker.LastReadData.AsDouble();
			result.CycleEndsAt = value7;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RemovableAt = null;
		}
		else
		{
			double value8 = unpacker.LastReadData.AsDouble();
			result.RemovableAt = value8;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<EstateLicense EstateId={EstateId} Type={Type} OwnerId={OwnerId} ActivatedAt={ActivatedAt} DepositRunsOutAt={DepositRunsOutAt} ExpiresAt={ExpiresAt} Deposit={Deposit} AccessRights={AccessRights} Size={Size} RegionId={RegionId} Tile={Tile} ProtectedUntil={ProtectedUntil} CycleStartsAt={CycleStartsAt} CycleEndsAt={CycleEndsAt} RemovableAt={RemovableAt}>";
	}
}
