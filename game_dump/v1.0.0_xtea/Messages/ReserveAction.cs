using MsgPack;
using UnityEngine;

namespace Messages;

public struct ReserveAction
{
	public const uint TypeCode = 600u;

	public string ActionSet;

	public ulong EntityId;

	public WorldPosition Pos;

	public bool CancelMove;

	public static void Pack(Packer packer, ReserveAction val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(600u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.ActionSet == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ActionSet);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((uint)Mathf.RoundToInt(val.Pos.x));
		packer.Pack((uint)Mathf.RoundToInt(val.Pos.y));
		packer.Pack(val.CancelMove);
	}

	public static ReserveAction Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ReserveAction result = default(ReserveAction);
		result.ActionSet = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EntityId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		unpacker.ReadSingle(ref result.Pos.x);
		unpacker.ReadSingle(ref result.Pos.y);
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.CancelMove = ((MessagePackObject)(ref lastReadData3)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ReserveAction ActionSet={ActionSet} EntityId={EntityId} Pos={Pos} CancelMove={CancelMove}>";
	}
}
