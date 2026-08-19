using MsgPack;

namespace Messages;

public struct CurrentArchipelagoTodos
{
	public const uint TypeCode = 240001u;

	public ArchipelagoTodos? CurrentTodos;

	public bool IsClearedRegion;

	public string ActiveRegionId;

	public bool IsEpic;

	public static void Pack(Packer packer, CurrentArchipelagoTodos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(240001u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (!val.CurrentTodos.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			ArchipelagoTodos.Pack(packer, val.CurrentTodos.Value);
		}
		packer.Pack(val.IsClearedRegion);
		if (val.ActiveRegionId == null)
		{
			packer.PackNull();
		}
		else if (val.ActiveRegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ActiveRegionId);
		}
		packer.Pack(val.IsEpic);
	}

	public static CurrentArchipelagoTodos Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CurrentArchipelagoTodos result = default(CurrentArchipelagoTodos);
		if (unpacker.LastReadData.IsNil)
		{
			result.CurrentTodos = null;
		}
		else
		{
			ArchipelagoTodos value = ArchipelagoTodos.Unpack(unpacker);
			result.CurrentTodos = value;
		}
		unpacker.Read();
		result.IsClearedRegion = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ActiveRegionId = null;
		}
		else
		{
			string activeRegionId = unpacker.LastReadData.AsString();
			result.ActiveRegionId = activeRegionId;
		}
		unpacker.Read();
		result.IsEpic = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<CurrentArchipelagoTodos CurrentTodos={CurrentTodos} IsClearedRegion={IsClearedRegion} ActiveRegionId={ActiveRegionId} IsEpic={IsEpic}>";
	}
}
