using MsgPack;

namespace Messages;

public struct AnimalDebugStatus
{
	public const uint TypeCode = 9898980u;

	public string EntityId;

	public string[] Properties;

	public static void Pack(Packer packer, AnimalDebugStatus val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(9898980u);
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
		if (val.Properties == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Properties.Length);
		for (int i = 0; i < val.Properties.Length; i++)
		{
			if (val.Properties[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Properties[i]);
			}
		}
	}

	public static AnimalDebugStatus Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AnimalDebugStatus result = default(AnimalDebugStatus);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Properties = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Properties[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AnimalDebugStatus EntityId={EntityId} Properties={Properties}>";
	}
}
