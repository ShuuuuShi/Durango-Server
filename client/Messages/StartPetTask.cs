using MsgPack;

namespace Messages;

public struct StartPetTask
{
	public const uint TypeCode = 65102u;

	public string EntityId;

	public Point2 Tile;

	public string PetId;

	public string TaskId;

	public static void Pack(Packer packer, StartPetTask val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(65102u);
		}
		else
		{
			packer.PackArrayHeader(4);
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
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.PetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PetId);
		}
		if (val.TaskId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TaskId);
		}
	}

	public static StartPetTask Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		StartPetTask result = default(StartPetTask);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.PetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TaskId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<StartPetTask EntityId={EntityId} Tile={Tile} PetId={PetId} TaskId={TaskId}>";
	}
}
