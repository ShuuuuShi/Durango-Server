using MsgPack;

namespace Messages;

public struct _Dirty
{
	public const uint TypeCode = 504u;

	public string EntityId;

	public int Level;

	public static void Pack(Packer packer, _Dirty val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(504u);
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
		packer.Pack(val.Level);
	}

	public static _Dirty Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		_Dirty result = default(_Dirty);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<_Dirty EntityId={EntityId} Level={Level}>";
	}
}
