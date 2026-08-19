using MsgPack;

namespace Messages;

public struct GetSimilarProducts
{
	public const uint TypeCode = 5008u;

	public string PrototypeId;

	public int Level;

	public string[] MajorTags;

	public int? Limit;

	public static void Pack(Packer packer, GetSimilarProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(5008u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.PrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PrototypeId);
		}
		packer.Pack(val.Level);
		if (val.MajorTags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.MajorTags.Length);
			for (int i = 0; i < val.MajorTags.Length; i++)
			{
				if (val.MajorTags[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.MajorTags[i]);
				}
			}
		}
		if (!val.Limit.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Limit.Value);
		}
	}

	public static GetSimilarProducts Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetSimilarProducts result = default(GetSimilarProducts);
		result.PrototypeId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.MajorTags = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string[] majorTags = result.MajorTags;
			int num2 = i;
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			majorTags[num2] = ((MessagePackObject)(ref lastReadData4)).AsString();
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData5)).IsNil)
		{
			result.Limit = null;
		}
		else
		{
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData6)).AsInt32();
			result.Limit = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetSimilarProducts PrototypeId={PrototypeId} Level={Level} MajorTags={MajorTags} Limit={Limit}>";
	}
}
