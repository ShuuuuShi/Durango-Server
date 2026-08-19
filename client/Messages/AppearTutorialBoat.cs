using MsgPack;
using Shared.Etc;

namespace Messages;

public struct AppearTutorialBoat
{
	public const uint TypeCode = 95u;

	public string EntityId;

	public ushort EntityType;

	public bool IsAlive;

	public Point2 Tile;

	public Point2 Size;

	public int Height;

	public int? Floor;

	public int? Stories;

	public bool? HasRoof;

	public Rotation Rotation;

	public ArtifactDisplay Display;

	public Tags Tags;

	public ArtifactState States;

	public string FounderEntityId;

	public TutorialBoatSessions Status;

	public static void Pack(Packer packer, AppearTutorialBoat val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(16);
			packer.Pack(95u);
		}
		else
		{
			packer.PackArrayHeader(15);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.EntityType);
		packer.Pack(val.IsAlive);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Size.x);
		packer.Pack((ushort)val.Size.y);
		packer.Pack(val.Height);
		if (!val.Floor.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Floor.Value);
		}
		if (!val.Stories.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Stories.Value);
		}
		if (!val.HasRoof.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.HasRoof.Value);
		}
		packer.Pack((int)val.Rotation);
		ArtifactDisplay.Pack(packer, val.Display);
		Tags.Pack(packer, val.Tags);
		ArtifactState.Pack(packer, val.States);
		if (val.FounderEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.FounderEntityId);
		}
		TutorialBoatSessions.Pack(packer, val.Status);
	}

	public static AppearTutorialBoat Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AppearTutorialBoat result = default(AppearTutorialBoat);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.IsAlive = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		unpacker.ReadUInt16(out var result3);
		result.Size.x = result3;
		unpacker.ReadUInt16(out result3);
		result.Size.y = result3;
		unpacker.Read();
		result.Height = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Floor = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.Floor = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Stories = null;
		}
		else
		{
			int value2 = unpacker.LastReadData.AsInt32();
			result.Stories = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.HasRoof = null;
		}
		else
		{
			bool value3 = unpacker.LastReadData.AsBoolean();
			result.HasRoof = value3;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 3 < num)
		{
			result.Rotation = Rotation.Invalid;
		}
		else
		{
			result.Rotation = (Rotation)num;
		}
		unpacker.Read();
		result.Display = ArtifactDisplay.Unpack(unpacker);
		unpacker.Read();
		result.Tags = Tags.Unpack(unpacker);
		unpacker.Read();
		result.States = ArtifactState.Unpack(unpacker);
		unpacker.Read();
		result.FounderEntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Status = TutorialBoatSessions.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<AppearTutorialBoat EntityId={EntityId} EntityType={EntityType} IsAlive={IsAlive} Tile={Tile} Size={Size} Height={Height} Floor={Floor} Stories={Stories} HasRoof={HasRoof} Rotation={Rotation} Display={Display} Tags={Tags} States={States} FounderEntityId={FounderEntityId} Status={Status}>";
	}
}
