using MsgPack;
using Shared.Economy;

namespace Messages;

public struct SupportRewards
{
	public const uint TypeCode = 731497u;

	public ItemSupportReward[] Items;

	public Money[] Moneys;

	public static void Pack(Packer packer, SupportRewards val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(731497u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Items == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Items.Length);
			for (int i = 0; i < val.Items.Length; i++)
			{
				ItemSupportReward.Pack(packer, val.Items[i]);
			}
		}
		if (val.Moneys == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Moneys.Length);
		for (int j = 0; j < val.Moneys.Length; j++)
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.Moneys[j].Amount);
			packer.Pack((int)val.Moneys[j].Currency);
		}
	}

	public static SupportRewards Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SupportRewards result = default(SupportRewards);
		result.Items = new ItemSupportReward[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref ItemSupportReward reference = ref result.Items[i];
			reference = ItemSupportReward.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Moneys = new Money[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			unpacker.ReadInt32(out var result2);
			unpacker.ReadInt32(out var result3);
			ref Money reference2 = ref result.Moneys[j];
			reference2 = new Money(result2, (Currency)result3);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SupportRewards Items={Items} Moneys={Moneys}>";
	}
}
