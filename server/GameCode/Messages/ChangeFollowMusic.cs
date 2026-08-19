using MsgPack;

namespace Messages;

public struct ChangeFollowMusic
{
	public const uint TypeCode = 47852459u;

	public string SharedSheetId;

	public bool WantFollow;

	public static void Pack(Packer packer, ChangeFollowMusic val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(47852459u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.SharedSheetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SharedSheetId);
		}
		packer.Pack(val.WantFollow);
	}

	public static ChangeFollowMusic Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ChangeFollowMusic result = default(ChangeFollowMusic);
		result.SharedSheetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.WantFollow = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ChangeFollowMusic SharedSheetId={SharedSheetId} WantFollow={WantFollow}>";
	}
}
