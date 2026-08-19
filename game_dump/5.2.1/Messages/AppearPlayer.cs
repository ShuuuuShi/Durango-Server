using MsgPack;

namespace Messages;

public struct AppearPlayer
{
	public const uint TypeCode = 90u;

	public string EntityId;

	public ushort EntityType;

	public bool IsAlive;

	public string Name;

	public int Freq;

	public int Level;

	public Title Title;

	public Member Member;

	public PlayerDisplay Display;

	public Move Move;

	public Survival Survival;

	public Musician? Musician;

	public bool RescueRequested;

	public static void Pack(Packer packer, AppearPlayer val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(14);
			packer.Pack(90u);
		}
		else
		{
			packer.PackArrayHeader(13);
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
		if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
		packer.Pack(val.Freq);
		packer.Pack(val.Level);
		Title.Pack(packer, val.Title);
		Member.Pack(packer, val.Member);
		PlayerDisplay.Pack(packer, val.Display);
		Move.Pack(packer, val.Move);
		Survival.Pack(packer, val.Survival);
		if (!val.Musician.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Musician.Pack(packer, val.Musician.Value);
		}
		packer.Pack(val.RescueRequested);
	}

	public static AppearPlayer Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AppearPlayer result = default(AppearPlayer);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.IsAlive = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.Name = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Freq = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
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
		if (unpacker.LastReadData.IsNil)
		{
			result.Musician = null;
		}
		else
		{
			Musician value = Messages.Musician.Unpack(unpacker);
			result.Musician = value;
		}
		unpacker.Read();
		result.RescueRequested = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<AppearPlayer EntityId={EntityId} EntityType={EntityType} IsAlive={IsAlive} Name={Name} Freq={Freq} Level={Level} Title={Title} Member={Member} Display={Display} Move={Move} Survival={Survival} Musician={Musician} RescueRequested={RescueRequested}>";
	}
}
