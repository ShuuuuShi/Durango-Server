using MsgPack;

namespace Messages;

public struct Recipes
{
	public const uint TypeCode = 120u;

	public string[] Ids;

	public static void Pack(Packer packer, Recipes val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(120u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Ids == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Ids.Length);
		for (int i = 0; i < val.Ids.Length; i++)
		{
			if (val.Ids[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Ids[i]);
			}
		}
	}

	public static Recipes Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Recipes result = default(Recipes);
		result.Ids = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string[] ids = result.Ids;
			int num2 = i;
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			ids[num2] = ((MessagePackObject)(ref lastReadData2)).AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("<Recipes Ids={0}>", Ids);
	}
}
