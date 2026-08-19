using MsgPack;

namespace Messages;

public struct AvailableClanResearch
{
	public const uint TypeCode = 5987334u;

	public string[] AvailableResearchIds;

	public static void Pack(Packer packer, AvailableClanResearch val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5987334u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.AvailableResearchIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.AvailableResearchIds.Length);
		for (int i = 0; i < val.AvailableResearchIds.Length; i++)
		{
			if (val.AvailableResearchIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.AvailableResearchIds[i]);
			}
		}
	}

	public static AvailableClanResearch Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AvailableClanResearch result = default(AvailableClanResearch);
		result.AvailableResearchIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.AvailableResearchIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("<AvailableClanResearch AvailableResearchIds={0}>", AvailableResearchIds);
	}
}
