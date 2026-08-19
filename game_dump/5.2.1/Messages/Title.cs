using MsgPack;

namespace Messages;

public struct Title
{
	public const uint TypeCode = 334u;

	public string EntityId;

	public string TitleId;

	public string _Title;

	public static void Pack(Packer packer, Title val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(334u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.TitleId == null)
		{
			packer.PackNull();
		}
		else if (val.TitleId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TitleId);
		}
		packer.PackString(val._Title);
	}

	public static Title Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Title result = default(Title);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TitleId = null;
		}
		else
		{
			string titleId = unpacker.LastReadData.AsString();
			result.TitleId = titleId;
		}
		unpacker.Read();
		result._Title = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return "<Title EntityId=" + EntityId + " TitleId=" + TitleId + " _Title=" + _Title + ">";
	}
}
