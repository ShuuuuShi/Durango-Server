using MsgPack;

namespace Messages;

public struct AvailableTask
{
	public const uint TypeCode = 65107u;

	public string[] Tasks;

	public static void Pack(Packer packer, AvailableTask val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(65107u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Tasks == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Tasks.Length);
		for (int i = 0; i < val.Tasks.Length; i++)
		{
			if (val.Tasks[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Tasks[i]);
			}
		}
	}

	public static AvailableTask Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AvailableTask result = default(AvailableTask);
		result.Tasks = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Tasks[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("<AvailableTask Tasks={0}>", Tasks);
	}
}
