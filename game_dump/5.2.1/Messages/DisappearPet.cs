using MsgPack;

namespace Messages;

public struct DisappearPet
{
	public const uint TypeCode = 198246u;

	public string EntityId;

	public string TamerEntityId;

	public static void Pack(Packer packer, DisappearPet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(198246u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.TamerEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TamerEntityId);
		}
	}

	public static DisappearPet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DisappearPet result = default(DisappearPet);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TamerEntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<DisappearPet EntityId=" + EntityId + " TamerEntityId=" + TamerEntityId + ">";
	}
}
