using MsgPack;

namespace Messages;

public struct AppearPlayer
{
	public const uint TypeCode = 90u;

	public ulong EntityId;

	public ushort EntityType;

	public string Name;

	public Title Title;

	public Member Member;

	public PlayerDisplay Display;

	public Move Move;

	public Survival Survival;

	public Rider? Rider;

	public static void Pack(Packer packer, AppearPlayer val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(10);
			packer.Pack(90u);
		}
		else
		{
			packer.PackArrayHeader(9);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.EntityType);
		if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
		Title.Pack(packer, val.Title);
		Member.Pack(packer, val.Member);
		PlayerDisplay.Pack(packer, val.Display);
		Move.Pack(packer, val.Move);
		Survival.Pack(packer, val.Survival);
		if (!val.Rider.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Rider.Pack(packer, val.Rider.Value);
		}
	}

	public static AppearPlayer Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		AppearPlayer result = default(AppearPlayer);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EntityType = ((MessagePackObject)(ref lastReadData2)).AsUInt16();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Name = ((MessagePackObject)(ref lastReadData3)).AsString();
		unpacker.Read();
		result.Title = Title.Unpack(unpacker);
		unpacker.Read();
		result.Member = Member.Unpack(unpacker);
		unpacker.Read();
		result.Display = PlayerDisplay.Unpack(unpacker);
		unpacker.Read();
		result.Move = Move.Unpack(unpacker);
		unpacker.Read();
		result.Survival = Survival.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.Rider = null;
		}
		else
		{
			Rider value = Messages.Rider.Unpack(unpacker);
			result.Rider = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AppearPlayer EntityId={EntityId} EntityType={EntityType} Name={Name} Title={Title} Member={Member} Display={Display} Move={Move} Survival={Survival} Rider={Rider}>";
	}
}
