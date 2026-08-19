using MsgPack;

namespace Messages;

public struct ResurrectionReady
{
	public const uint TypeCode = 56238473u;

	public string HelperEntityId;

	public double ValidUntil;

	public string[] RewardItemIds;

	public static void Pack(Packer packer, ResurrectionReady val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(56238473u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.HelperEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.HelperEntityId);
		}
		packer.Pack(val.ValidUntil);
		if (val.RewardItemIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.RewardItemIds.Length);
		for (int i = 0; i < val.RewardItemIds.Length; i++)
		{
			if (val.RewardItemIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.RewardItemIds[i]);
			}
		}
	}

	public static ResurrectionReady Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ResurrectionReady result = default(ResurrectionReady);
		result.HelperEntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ValidUntil = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.RewardItemIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.RewardItemIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ResurrectionReady HelperEntityId={HelperEntityId} ValidUntil={ValidUntil} RewardItemIds={RewardItemIds}>";
	}
}
