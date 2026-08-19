using System.Collections.Generic;
using MsgPack;
using Shared.Etc;

namespace Messages;

public struct AppearTutorialBoat
{
	public const uint TypeCode = 95u;

	public ulong EntityId;

	public ushort EntityType;

	public Point2 Tile;

	public KeyValuePair<int, int> Size;

	public int Height;

	public Rotation Rotation;

	public ArtifactDisplay Display;

	public Tags Tags;

	public ArtifactState States;

	public ulong FounderEntityId;

	public TutorialBoatSessions Status;

	public static void Pack(Packer packer, AppearTutorialBoat val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(12);
			packer.Pack(95u);
		}
		else
		{
			packer.PackArrayHeader(11);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.EntityType);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.PackArrayHeader(2);
		packer.Pack(val.Size.Key);
		packer.Pack(val.Size.Value);
		packer.Pack(val.Height);
		packer.Pack((int)val.Rotation);
		ArtifactDisplay.Pack(packer, val.Display);
		Tags.Pack(packer, val.Tags);
		ArtifactState.Pack(packer, val.States);
		packer.Pack(val.FounderEntityId);
		TutorialBoatSessions.Pack(packer, val.Status);
	}

	public static AppearTutorialBoat Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		AppearTutorialBoat result = default(AppearTutorialBoat);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EntityType = ((MessagePackObject)(ref lastReadData2)).AsUInt16();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int key = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int value = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.Size = new KeyValuePair<int, int>(key, value);
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.Height = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
		if (num2 < 0 || 3 < num2)
		{
			result.Rotation = Rotation.Invalid;
		}
		else
		{
			result.Rotation = (Rotation)num2;
		}
		unpacker.Read();
		result.Display = ArtifactDisplay.Unpack(unpacker);
		unpacker.Read();
		result.Tags = Tags.Unpack(unpacker);
		unpacker.Read();
		result.States = ArtifactState.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.FounderEntityId = ((MessagePackObject)(ref lastReadData7)).AsUInt64();
		unpacker.Read();
		result.Status = TutorialBoatSessions.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<AppearTutorialBoat EntityId={EntityId} EntityType={EntityType} Tile={Tile} Size={Size} Height={Height} Rotation={Rotation} Display={Display} Tags={Tags} States={States} FounderEntityId={FounderEntityId} Status={Status}>";
	}
}
