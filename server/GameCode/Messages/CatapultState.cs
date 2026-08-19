using MsgPack;

namespace Messages;

public struct CatapultState
{
	public float RotSpeed;

	public string[] RequiredProjectileTags;

	public int Atk;

	public float AtkRangeMin;

	public float AtkRangeMax;

	public float DmgRadius;

	public float Cooltime;

	public int RemainedProjectilesSize;

	public int MaxProjectilesSize;

	public static void Pack(Packer packer, CatapultState val, bool hint = false)
	{
		packer.PackArrayHeader(9);
		packer.Pack(val.RotSpeed);
		if (val.RequiredProjectileTags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.RequiredProjectileTags.Length);
			for (int i = 0; i < val.RequiredProjectileTags.Length; i++)
			{
				if (val.RequiredProjectileTags[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.RequiredProjectileTags[i]);
				}
			}
		}
		packer.Pack(val.Atk);
		packer.Pack(val.AtkRangeMin);
		packer.Pack(val.AtkRangeMax);
		packer.Pack(val.DmgRadius);
		packer.Pack(val.Cooltime);
		packer.Pack(val.RemainedProjectilesSize);
		packer.Pack(val.MaxProjectilesSize);
	}

	public static CatapultState Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CatapultState result = default(CatapultState);
		result.RotSpeed = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.RequiredProjectileTags = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.RequiredProjectileTags[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		result.Atk = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.AtkRangeMin = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.AtkRangeMax = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.DmgRadius = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.Cooltime = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.RemainedProjectilesSize = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.MaxProjectilesSize = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<CatapultState RotSpeed={RotSpeed} RequiredProjectileTags={RequiredProjectileTags} Atk={Atk} AtkRangeMin={AtkRangeMin} AtkRangeMax={AtkRangeMax} DmgRadius={DmgRadius} Cooltime={Cooltime} RemainedProjectilesSize={RemainedProjectilesSize} MaxProjectilesSize={MaxProjectilesSize}>";
	}
}
