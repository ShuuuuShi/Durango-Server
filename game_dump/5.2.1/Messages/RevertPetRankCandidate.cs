using MsgPack;
using Shared.Animal;

namespace Messages;

public struct RevertPetRankCandidate
{
	public const uint TypeCode = 74015u;

	public PetRank Rank;

	public string Tag;

	public static void Pack(Packer packer, RevertPetRankCandidate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(74015u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Rank);
		if (val.Tag == null)
		{
			packer.PackNull();
		}
		else if (val.Tag == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Tag);
		}
	}

	public static RevertPetRankCandidate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		RevertPetRankCandidate result = default(RevertPetRankCandidate);
		if (num < 10 || 14 < num)
		{
			result.Rank = PetRank.Invalid;
		}
		else
		{
			result.Rank = (PetRank)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Tag = null;
		}
		else
		{
			string tag = unpacker.LastReadData.AsString();
			result.Tag = tag;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RevertPetRankCandidate Rank={Rank} Tag={Tag}>";
	}
}
