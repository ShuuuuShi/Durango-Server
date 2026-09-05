using MsgPack;

namespace Messages;

public struct Home
{
	public int Capacity;

	public string[] ResidentEntityIds;

	public static void Pack(Packer packer, Home val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.Capacity);
		if (val.ResidentEntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ResidentEntityIds.Length);
		for (int i = 0; i < val.ResidentEntityIds.Length; i++)
		{
			if (val.ResidentEntityIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.ResidentEntityIds[i]);
			}
		}
	}

	public static Home Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Home result = default(Home);
		result.Capacity = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.ResidentEntityIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ResidentEntityIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Home Capacity={Capacity} ResidentEntityIds={ResidentEntityIds}>";
	}
}
