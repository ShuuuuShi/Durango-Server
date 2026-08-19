using MsgPack;

namespace Messages;

public struct InventoryState
{
	public string[] StorableTags;

	public string[] UnstorableTags;

	public float ReduceDurabilityVelocity;

	public static void Pack(Packer packer, InventoryState val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.StorableTags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.StorableTags.Length);
			for (int i = 0; i < val.StorableTags.Length; i++)
			{
				if (val.StorableTags[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.StorableTags[i]);
				}
			}
		}
		if (val.UnstorableTags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.UnstorableTags.Length);
			for (int j = 0; j < val.UnstorableTags.Length; j++)
			{
				if (val.UnstorableTags[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.UnstorableTags[j]);
				}
			}
		}
		packer.Pack(val.ReduceDurabilityVelocity);
	}

	public static InventoryState Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		InventoryState result = default(InventoryState);
		result.StorableTags = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.StorableTags[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.UnstorableTags = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.UnstorableTags[j] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		result.ReduceDurabilityVelocity = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<InventoryState StorableTags={StorableTags} UnstorableTags={UnstorableTags} ReduceDurabilityVelocity={ReduceDurabilityVelocity}>";
	}
}
