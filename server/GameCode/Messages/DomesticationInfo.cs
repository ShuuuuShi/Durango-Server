using MsgPack;
using Shared.Animal;

namespace Messages;

public struct DomesticationInfo
{
	public string ItemId;

	public ushort EntityType;

	public ushort PetEntityType;

	public int Level;

	public ushort Generation;

	public PetRank? Rank;

	public double TotalTime;

	public double DomesticateSince;

	public double DomesticateUntil;

	public float DomesticateSuccessRate;

	public float DomesticationSuccessMaxRate;

	public string[] EatableTags;

	public bool Domesticated;

	public bool DomesticationInProgress;

	public static void Pack(Packer packer, DomesticationInfo val, bool hint = false)
	{
		packer.PackArrayHeader(14);
		if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
		packer.Pack(val.EntityType);
		packer.Pack(val.PetEntityType);
		packer.Pack(val.Level);
		packer.Pack(val.Generation);
		if (!val.Rank.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack((int)val.Rank.Value);
		}
		packer.Pack(val.TotalTime);
		packer.Pack(val.DomesticateSince);
		packer.Pack(val.DomesticateUntil);
		packer.Pack(val.DomesticateSuccessRate);
		packer.Pack(val.DomesticationSuccessMaxRate);
		if (val.EatableTags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.EatableTags.Length);
			for (int i = 0; i < val.EatableTags.Length; i++)
			{
				if (val.EatableTags[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.EatableTags[i]);
				}
			}
		}
		packer.Pack(val.Domesticated);
		packer.Pack(val.DomesticationInProgress);
	}

	public static DomesticationInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DomesticationInfo result = default(DomesticationInfo);
		result.ItemId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.PetEntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Generation = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Rank = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			PetRank value = ((num >= 10 && 14 >= num) ? ((PetRank)num) : PetRank.Invalid);
			result.Rank = value;
		}
		unpacker.Read();
		result.TotalTime = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.DomesticateSince = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.DomesticateUntil = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.DomesticateSuccessRate = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.DomesticationSuccessMaxRate = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.EatableTags = new string[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			result.EatableTags[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		result.Domesticated = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.DomesticationInProgress = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<DomesticationInfo ItemId={ItemId} EntityType={EntityType} PetEntityType={PetEntityType} Level={Level} Generation={Generation} Rank={Rank} TotalTime={TotalTime} DomesticateSince={DomesticateSince} DomesticateUntil={DomesticateUntil} DomesticateSuccessRate={DomesticateSuccessRate} DomesticationSuccessMaxRate={DomesticationSuccessMaxRate} EatableTags={EatableTags} Domesticated={Domesticated} DomesticationInProgress={DomesticationInProgress}>";
	}
}
