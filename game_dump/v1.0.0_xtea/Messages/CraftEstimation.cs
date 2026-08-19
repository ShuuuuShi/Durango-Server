using MsgPack;
using UnityEngine;

namespace Messages;

public struct CraftEstimation
{
	public const uint TypeCode = 8u;

	public string PrototypeId;

	public int Level;

	public string Name;

	public Vector2 Durability;

	public int ModifiableCount;

	public float SuccessRate;

	public static void Pack(Packer packer, CraftEstimation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(8u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		if (val.PrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PrototypeId);
		}
		packer.Pack(val.Level);
		packer.PackString(val.Name);
		packer.PackArrayHeader(2);
		packer.Pack(val.Durability.x);
		packer.Pack(val.Durability.y);
		packer.Pack(val.ModifiableCount);
		packer.Pack(val.SuccessRate);
	}

	public static CraftEstimation Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		CraftEstimation result = default(CraftEstimation);
		result.PrototypeId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		unpacker.ReadSingle(ref result.Durability.x);
		unpacker.ReadSingle(ref result.Durability.y);
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.ModifiableCount = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.SuccessRate = ((MessagePackObject)(ref lastReadData4)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return $"<CraftEstimation PrototypeId={PrototypeId} Level={Level} Name={Name} Durability={Durability} ModifiableCount={ModifiableCount} SuccessRate={SuccessRate}>";
	}
}
