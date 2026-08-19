using MsgPack;

namespace Messages;

public struct Appliers
{
	public string[] ApplierEntityIds;

	public static void Pack(Packer packer, Appliers val, bool hint = false)
	{
		packer.PackArrayHeader(1);
		if (val.ApplierEntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ApplierEntityIds.Length);
		for (int i = 0; i < val.ApplierEntityIds.Length; i++)
		{
			if (val.ApplierEntityIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.ApplierEntityIds[i]);
			}
		}
	}

	public static Appliers Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Appliers result = default(Appliers);
		result.ApplierEntityIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ApplierEntityIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		object[] applierEntityIds = ApplierEntityIds;
		return string.Format("<Appliers ApplierEntityIds={0}>", applierEntityIds);
	}
}
