using MsgPack;
using Shared.Faction;

namespace Messages;

public struct MissionToDo
{
	public string Id;

	public MissionTodoType Type;

	public string Label;

	public MissionTodoOrder Order;

	public int Progress;

	public int GoalCount;

	public string Tooltip;

	public Point2? Destination;

	public static void Pack(Packer packer, MissionToDo val, bool hint = false)
	{
		packer.PackArrayHeader(8);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		packer.Pack((int)val.Type);
		packer.PackString(val.Label);
		packer.Pack((int)val.Order);
		packer.Pack(val.Progress);
		packer.Pack(val.GoalCount);
		if (val.Tooltip == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.Tooltip);
		}
		if (!val.Destination.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Destination.Value.x);
		packer.Pack((ushort)val.Destination.Value.y);
	}

	public static MissionToDo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MissionToDo result = default(MissionToDo);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 7 < num)
		{
			result.Type = MissionTodoType.Invalid;
		}
		else
		{
			result.Type = (MissionTodoType)num;
		}
		unpacker.Read();
		result.Label = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		if (num2 < 0 || 1 < num2)
		{
			result.Order = MissionTodoOrder.Invalid;
		}
		else
		{
			result.Order = (MissionTodoOrder)num2;
		}
		unpacker.Read();
		result.Progress = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.GoalCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Tooltip = null;
		}
		else
		{
			string tooltip = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.Tooltip = tooltip;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Destination = null;
		}
		else
		{
			unpacker.ReadUInt16(out var result2);
			Point2 value = default(Point2);
			value.x = result2;
			unpacker.ReadUInt16(out result2);
			value.y = result2;
			result.Destination = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<MissionToDo Id={Id} Type={Type} Label={Label} Order={Order} Progress={Progress} GoalCount={GoalCount} Tooltip={Tooltip} Destination={Destination}>";
	}
}
