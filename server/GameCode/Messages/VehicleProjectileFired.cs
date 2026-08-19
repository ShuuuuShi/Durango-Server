using MsgPack;
using UnityEngine;

namespace Messages;

public struct VehicleProjectileFired
{
	public const uint TypeCode = 243178u;

	public string EntityId;

	public WorldPosition TargetPosition;

	public float ProjectileSpeed;

	public double PrepareAnimAt;

	public double FireAnimAt;

	public double ShootAt;

	public double WarnSince;

	public double WarnUntil;

	public double DmgAt;

	public float DmgRadius;

	public double CoolingUntil;

	public static void Pack(Packer packer, VehicleProjectileFired val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(12);
			packer.Pack(243178u);
		}
		else
		{
			packer.PackArrayHeader(11);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((uint)Mathf.RoundToInt(val.TargetPosition.x));
		packer.Pack((uint)Mathf.RoundToInt(val.TargetPosition.y));
		packer.Pack(val.ProjectileSpeed);
		packer.Pack(val.PrepareAnimAt);
		packer.Pack(val.FireAnimAt);
		packer.Pack(val.ShootAt);
		packer.Pack(val.WarnSince);
		packer.Pack(val.WarnUntil);
		packer.Pack(val.DmgAt);
		packer.Pack(val.DmgRadius);
		packer.Pack(val.CoolingUntil);
	}

	public static VehicleProjectileFired Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		VehicleProjectileFired result = default(VehicleProjectileFired);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadSingle(out result.TargetPosition.x);
		unpacker.ReadSingle(out result.TargetPosition.y);
		unpacker.Read();
		result.ProjectileSpeed = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.PrepareAnimAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.FireAnimAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.ShootAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.WarnSince = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.WarnUntil = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.DmgAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.DmgRadius = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.CoolingUntil = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<VehicleProjectileFired EntityId={EntityId} TargetPosition={TargetPosition} ProjectileSpeed={ProjectileSpeed} PrepareAnimAt={PrepareAnimAt} FireAnimAt={FireAnimAt} ShootAt={ShootAt} WarnSince={WarnSince} WarnUntil={WarnUntil} DmgAt={DmgAt} DmgRadius={DmgRadius} CoolingUntil={CoolingUntil}>";
	}
}
