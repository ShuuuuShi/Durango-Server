using MsgPack;
using Shared.Animal;

namespace Messages;

public struct Pet
{
	public const uint TypeCode = 1097965u;

	public string EntityId;

	public ushort EntityType;

	public string TamerEntityId;

	public string Name;

	public PetRank Rank;

	public ushort Generation;

	public bool IsBoarding;

	public bool IsSpawned;

	public PetStats Stat;

	public PetStatistics Statistics;

	public CageInfo? CageInfo;

	public static void Pack(Packer packer, Pet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(12);
			packer.Pack(1097965u);
		}
		else
		{
			packer.PackArrayHeader(11);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.EntityType);
		if (val.TamerEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TamerEntityId);
		}
		packer.PackString(val.Name);
		packer.Pack((int)val.Rank);
		packer.Pack(val.Generation);
		packer.Pack(val.IsBoarding);
		packer.Pack(val.IsSpawned);
		PetStats.Pack(packer, val.Stat);
		PetStatistics.Pack(packer, val.Statistics);
		if (!val.CageInfo.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.CageInfo.Pack(packer, val.CageInfo.Value);
		}
	}

	public static Pet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Pet result = default(Pet);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.TamerEntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 10 || 14 < num)
		{
			result.Rank = PetRank.Invalid;
		}
		else
		{
			result.Rank = (PetRank)num;
		}
		unpacker.Read();
		result.Generation = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.IsBoarding = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.IsSpawned = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.Stat = PetStats.Unpack(unpacker);
		unpacker.Read();
		result.Statistics = PetStatistics.Unpack(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.CageInfo = null;
		}
		else
		{
			CageInfo value = Messages.CageInfo.Unpack(unpacker);
			result.CageInfo = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Pet EntityId={EntityId} EntityType={EntityType} TamerEntityId={TamerEntityId} Name={Name} Rank={Rank} Generation={Generation} IsBoarding={IsBoarding} IsSpawned={IsSpawned} Stat={Stat} Statistics={Statistics} CageInfo={CageInfo}>";
	}
}
