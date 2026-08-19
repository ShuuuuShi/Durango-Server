using MsgPack;

namespace Messages;

public struct AvailablePersonalResearch
{
	public const uint TypeCode = 5987337u;

	public string[] AvailableResearchIds;

	public Pair<string, int>[] UnavailableResearchIds;

	public string ResearchingId;

	public double? AvailableResearchAt;

	public static void Pack(Packer packer, AvailablePersonalResearch val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(5987337u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.AvailableResearchIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
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
		if (val.UnavailableResearchIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.UnavailableResearchIds.Length);
			for (int j = 0; j < val.UnavailableResearchIds.Length; j++)
			{
				packer.PackArrayHeader(2);
				if (val.UnavailableResearchIds[j].Item1 == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.UnavailableResearchIds[j].Item1);
				}
				packer.Pack(val.UnavailableResearchIds[j].Item2);
			}
		}
		if (val.ResearchingId == null)
		{
			packer.PackNull();
		}
		else if (val.ResearchingId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ResearchingId);
		}
		if (!val.AvailableResearchAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.AvailableResearchAt.Value);
		}
	}

	public static AvailablePersonalResearch Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AvailablePersonalResearch result = default(AvailablePersonalResearch);
		result.AvailableResearchIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.AvailableResearchIds[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.UnavailableResearchIds = new Pair<string, int>[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			unpacker.Read();
			string item = unpacker.LastReadData.AsString();
			unpacker.Read();
			int item2 = unpacker.LastReadData.AsInt32();
			ref Pair<string, int> reference = ref result.UnavailableResearchIds[j];
			reference = new Pair<string, int>(item, item2);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ResearchingId = null;
		}
		else
		{
			string researchingId = unpacker.LastReadData.AsString();
			result.ResearchingId = researchingId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.AvailableResearchAt = null;
		}
		else
		{
			double value = unpacker.LastReadData.AsDouble();
			result.AvailableResearchAt = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AvailablePersonalResearch AvailableResearchIds={AvailableResearchIds} UnavailableResearchIds={UnavailableResearchIds} ResearchingId={ResearchingId} AvailableResearchAt={AvailableResearchAt}>";
	}
}
