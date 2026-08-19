using MsgPack;

namespace Messages;

public struct ClanResearch
{
	public const uint TypeCode = 5987335u;

	public string ResearchId;

	public double Until;

	public double CooltimeUntil;

	public string LabEntityId;

	public static void Pack(Packer packer, ClanResearch val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(5987335u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.ResearchId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ResearchId);
		}
		packer.Pack(val.Until);
		packer.Pack(val.CooltimeUntil);
		if (val.LabEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.LabEntityId);
		}
	}

	public static ClanResearch Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ClanResearch result = default(ClanResearch);
		result.ResearchId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Until = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.CooltimeUntil = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.LabEntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<ClanResearch ResearchId={ResearchId} Until={Until} CooltimeUntil={CooltimeUntil} LabEntityId={LabEntityId}>";
	}
}
