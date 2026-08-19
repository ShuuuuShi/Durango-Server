using MsgPack;

namespace Messages;

public struct UseBattleAction
{
	public const uint TypeCode = 3440u;

	public string ActionId;

	public double StartAt;

	public string TargetEntityId;

	public Point2? TargetTile;

	public static void Pack(Packer packer, UseBattleAction val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3440u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.ActionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ActionId);
		}
		packer.Pack(val.StartAt);
		if (val.TargetEntityId == null)
		{
			packer.PackNull();
		}
		else if (val.TargetEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TargetEntityId);
		}
		if (!val.TargetTile.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.TargetTile.Value.x);
		packer.Pack((ushort)val.TargetTile.Value.y);
	}

	public static UseBattleAction Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		UseBattleAction result = default(UseBattleAction);
		result.ActionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.StartAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TargetEntityId = null;
		}
		else
		{
			string targetEntityId = unpacker.LastReadData.AsString();
			result.TargetEntityId = targetEntityId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TargetTile = null;
		}
		else
		{
			unpacker.ReadUInt16(out var result2);
			Point2 value = default(Point2);
			value.x = result2;
			unpacker.ReadUInt16(out result2);
			value.y = result2;
			result.TargetTile = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<UseBattleAction ActionId={ActionId} StartAt={StartAt} TargetEntityId={TargetEntityId} TargetTile={TargetTile}>";
	}
}
