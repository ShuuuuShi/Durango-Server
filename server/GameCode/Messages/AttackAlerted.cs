using MsgPack;
using Shared.Battle;
using UnityEngine;

namespace Messages;

public struct AttackAlerted
{
	public const uint TypeCode = 3928u;

	public string EntityId;

	public double EventAt;

	public double AttackTime;

	public DamageType DamageType;

	public float? Radius;

	public Vector2? RectSizeHalves;

	public WorldPosition? Center;

	public float? Yaw;

	public Vector2? Angles;

	public static void Pack(Packer packer, AttackAlerted val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(10);
			packer.Pack(3928u);
		}
		else
		{
			packer.PackArrayHeader(9);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.EventAt);
		packer.Pack(val.AttackTime);
		packer.Pack((int)val.DamageType);
		if (!val.Radius.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Radius.Value);
		}
		if (!val.RectSizeHalves.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.RectSizeHalves.Value.x);
			packer.Pack(val.RectSizeHalves.Value.y);
		}
		if (!val.Center.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack((uint)Mathf.RoundToInt(val.Center.Value.x));
			packer.Pack((uint)Mathf.RoundToInt(val.Center.Value.y));
		}
		if (!val.Yaw.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Yaw.Value);
		}
		if (!val.Angles.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.Angles.Value.x);
		packer.Pack(val.Angles.Value.y);
	}

	public static AttackAlerted Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AttackAlerted result = default(AttackAlerted);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EventAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.AttackTime = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 3 < num)
		{
			result.DamageType = DamageType.Invalid;
		}
		else
		{
			result.DamageType = (DamageType)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Radius = null;
		}
		else
		{
			float value = unpacker.LastReadData.AsSingle();
			result.Radius = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RectSizeHalves = null;
		}
		else
		{
			Vector2 value2 = default(Vector2);
			unpacker.ReadSingle(out value2.x);
			unpacker.ReadSingle(out value2.y);
			result.RectSizeHalves = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Center = null;
		}
		else
		{
			WorldPosition value3 = default(WorldPosition);
			unpacker.ReadSingle(out value3.x);
			unpacker.ReadSingle(out value3.y);
			result.Center = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Yaw = null;
		}
		else
		{
			float value4 = unpacker.LastReadData.AsSingle();
			result.Yaw = value4;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Angles = null;
		}
		else
		{
			Vector2 value5 = default(Vector2);
			unpacker.ReadSingle(out value5.x);
			unpacker.ReadSingle(out value5.y);
			result.Angles = value5;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AttackAlerted EntityId={EntityId} EventAt={EventAt} AttackTime={AttackTime} DamageType={DamageType} Radius={Radius} RectSizeHalves={RectSizeHalves} Center={Center} Yaw={Yaw} Angles={Angles}>";
	}
}
