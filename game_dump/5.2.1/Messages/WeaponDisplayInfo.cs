using MsgPack;

namespace Messages;

public struct WeaponDisplayInfo
{
	public string Projectile;

	public float? ProjectileSpeed;

	public float? DetonateDelay;

	public string WeaponFramework;

	public static void Pack(Packer packer, WeaponDisplayInfo val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		if (val.Projectile == null)
		{
			packer.PackNull();
		}
		else if (val.Projectile == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Projectile);
		}
		if (!val.ProjectileSpeed.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ProjectileSpeed.Value);
		}
		if (!val.DetonateDelay.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.DetonateDelay.Value);
		}
		if (val.WeaponFramework == null)
		{
			packer.PackNull();
		}
		else if (val.WeaponFramework == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.WeaponFramework);
		}
	}

	public static WeaponDisplayInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		WeaponDisplayInfo result = default(WeaponDisplayInfo);
		if (unpacker.LastReadData.IsNil)
		{
			result.Projectile = null;
		}
		else
		{
			string projectile = unpacker.LastReadData.AsString();
			result.Projectile = projectile;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ProjectileSpeed = null;
		}
		else
		{
			float value = unpacker.LastReadData.AsSingle();
			result.ProjectileSpeed = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.DetonateDelay = null;
		}
		else
		{
			float value2 = unpacker.LastReadData.AsSingle();
			result.DetonateDelay = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.WeaponFramework = null;
		}
		else
		{
			string weaponFramework = unpacker.LastReadData.AsString();
			result.WeaponFramework = weaponFramework;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<WeaponDisplayInfo Projectile={Projectile} ProjectileSpeed={ProjectileSpeed} DetonateDelay={DetonateDelay} WeaponFramework={WeaponFramework}>";
	}
}
