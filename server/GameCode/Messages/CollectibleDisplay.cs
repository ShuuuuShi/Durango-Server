using MsgPack;

namespace Messages;

public struct CollectibleDisplay
{
	public const uint TypeCode = 860936u;

	public string EntityId;

	public string[] DistributableEntities;

	public static void Pack(Packer packer, CollectibleDisplay val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(860936u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.DistributableEntities == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.DistributableEntities.Length);
		for (int i = 0; i < val.DistributableEntities.Length; i++)
		{
			if (val.DistributableEntities[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.DistributableEntities[i]);
			}
		}
	}

	public static CollectibleDisplay Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CollectibleDisplay result = default(CollectibleDisplay);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.DistributableEntities = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.DistributableEntities[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CollectibleDisplay EntityId={EntityId} DistributableEntities={DistributableEntities}>";
	}
}
