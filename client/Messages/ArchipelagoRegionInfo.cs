using MsgPack;

namespace Messages;

public struct ArchipelagoRegionInfo
{
	public string Id;

	public int Progess;

	public RegionCoOpTodo[] CoOpList;

	public static void Pack(Packer packer, ArchipelagoRegionInfo val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		packer.Pack(val.Progess);
		if (val.CoOpList == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.CoOpList.Length);
		for (int i = 0; i < val.CoOpList.Length; i++)
		{
			RegionCoOpTodo.Pack(packer, val.CoOpList[i]);
		}
	}

	public static ArchipelagoRegionInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArchipelagoRegionInfo result = default(ArchipelagoRegionInfo);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Progess = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.CoOpList = new RegionCoOpTodo[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref RegionCoOpTodo reference = ref result.CoOpList[i];
			reference = RegionCoOpTodo.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArchipelagoRegionInfo Id={Id} Progess={Progess} CoOpList={CoOpList}>";
	}
}
