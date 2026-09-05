using MsgPack;
using Shared.Economy;
using Shared.Faction;

namespace Messages;

public struct SupportRequest
{
	public const uint TypeCode = 523412u;

	public string RequestId;

	public string Name;

	public FactionType FactionType;

	public Money Fee;

	public int Level;

	public SupportRewards Rewards;

	public SupportRewards RandomRewards;

	public int FriendshipPointReward;

	public int Duration;

	public Pair<Item, int>? RequiredItem;

	public int MaxCount;

	public int RemainCount;

	public static void Pack(Packer packer, SupportRequest val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(13);
			packer.Pack(523412u);
		}
		else
		{
			packer.PackArrayHeader(12);
		}
		if (val.RequestId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RequestId);
		}
		packer.PackString(val.Name);
		packer.Pack((int)val.FactionType);
		packer.PackArrayHeader(2);
		packer.Pack(val.Fee.Amount);
		packer.Pack((int)val.Fee.Currency);
		packer.Pack(val.Level);
		SupportRewards.Pack(packer, val.Rewards);
		SupportRewards.Pack(packer, val.RandomRewards);
		packer.Pack(val.FriendshipPointReward);
		packer.Pack(val.Duration);
		if (!val.RequiredItem.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			Item.Pack(packer, val.RequiredItem.Value.Item1);
			packer.Pack(val.RequiredItem.Value.Item2);
		}
		packer.Pack(val.MaxCount);
		packer.Pack(val.RemainCount);
	}

	public static SupportRequest Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SupportRequest result = default(SupportRequest);
		result.RequestId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 101 < num)
		{
			result.FactionType = FactionType.Invalid;
		}
		else
		{
			result.FactionType = (FactionType)num;
		}
		unpacker.Read();
		unpacker.ReadInt32(out var result2);
		unpacker.ReadInt32(out var result3);
		result.Fee = new Money(result2, (Currency)result3);
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Rewards = SupportRewards.Unpack(unpacker);
		unpacker.Read();
		result.RandomRewards = SupportRewards.Unpack(unpacker);
		unpacker.Read();
		result.FriendshipPointReward = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Duration = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RequiredItem = null;
		}
		else
		{
			unpacker.Read();
			Item item = Item.Unpack(unpacker);
			unpacker.Read();
			int item2 = unpacker.LastReadData.AsInt32();
			Pair<Item, int> value = new Pair<Item, int>(item, item2);
			result.RequiredItem = value;
		}
		unpacker.Read();
		result.MaxCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.RemainCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<SupportRequest RequestId={RequestId} Name={Name} FactionType={FactionType} Fee={Fee} Level={Level} Rewards={Rewards} RandomRewards={RandomRewards} FriendshipPointReward={FriendshipPointReward} Duration={Duration} RequiredItem={RequiredItem} MaxCount={MaxCount} RemainCount={RemainCount}>";
	}
}
