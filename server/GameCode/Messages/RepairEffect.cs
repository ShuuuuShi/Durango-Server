using MsgPack;
using Shared.Item;
using Shared.System;

namespace Messages;

public struct RepairEffect
{
	public const uint TypeCode = 2082u;

	public Shared.System.RewardEffect Type;

	public Result Result;

	public string ItemId;

	public string EntityId;

	public static void Pack(Packer packer, RepairEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2082u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack((int)val.Type);
		packer.Pack((int)val.Result);
		if (val.ItemId == null)
		{
			packer.PackNull();
		}
		else if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
		if (val.EntityId == null)
		{
			packer.PackNull();
		}
		else if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
	}

	public static RepairEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		RepairEffect result = default(RepairEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		if (num2 < 0 || 3 < num2)
		{
			result.Result = Result.Invalid;
		}
		else
		{
			result.Result = (Result)num2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ItemId = null;
		}
		else
		{
			string itemId = unpacker.LastReadData.AsString();
			result.ItemId = itemId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.EntityId = null;
		}
		else
		{
			string entityId = unpacker.LastReadData.AsString();
			result.EntityId = entityId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RepairEffect Type={Type} Result={Result} ItemId={ItemId} EntityId={EntityId}>";
	}
}
