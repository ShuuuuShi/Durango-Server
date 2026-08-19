using MsgPack;

namespace Messages;

public struct S02PVPDead
{
	public const uint TypeCode = 222209u;

	public string VictimName;

	public int VictimRank;

	public int VictimKillCount;

	public float VictimSurvivedTime;

	public string KillerName;

	public string[] WeaponTags;

	public static void Pack(Packer packer, S02PVPDead val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(222209u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		if (val.VictimName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.VictimName);
		}
		packer.Pack(val.VictimRank);
		packer.Pack(val.VictimKillCount);
		packer.Pack(val.VictimSurvivedTime);
		if (val.KillerName == null)
		{
			packer.PackNull();
		}
		else if (val.KillerName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.KillerName);
		}
		if (val.WeaponTags == null)
		{
			packer.PackNull();
			return;
		}
		if (val.WeaponTags == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.WeaponTags.Length);
		for (int i = 0; i < val.WeaponTags.Length; i++)
		{
			if (val.WeaponTags[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.WeaponTags[i]);
			}
		}
	}

	public static S02PVPDead Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		S02PVPDead result = default(S02PVPDead);
		result.VictimName = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.VictimRank = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.VictimKillCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.VictimSurvivedTime = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.KillerName = null;
		}
		else
		{
			string killerName = unpacker.LastReadData.AsString();
			result.KillerName = killerName;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.WeaponTags = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				array[i] = unpacker.LastReadData.AsString();
			}
			result.WeaponTags = array;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<S02PVPDead VictimName={VictimName} VictimRank={VictimRank} VictimKillCount={VictimKillCount} VictimSurvivedTime={VictimSurvivedTime} KillerName={KillerName} WeaponTags={WeaponTags}>";
	}
}
