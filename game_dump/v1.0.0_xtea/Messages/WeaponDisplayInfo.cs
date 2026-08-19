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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		WeaponDisplayInfo result = default(WeaponDisplayInfo);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.Projectile = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			string projectile = ((MessagePackObject)(ref lastReadData2)).AsString();
			result.Projectile = projectile;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.ProjectileSpeed = null;
		}
		else
		{
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			float value = ((MessagePackObject)(ref lastReadData4)).AsSingle();
			result.ProjectileSpeed = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData5)).IsNil)
		{
			result.DetonateDelay = null;
		}
		else
		{
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			float value2 = ((MessagePackObject)(ref lastReadData6)).AsSingle();
			result.DetonateDelay = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData7)).IsNil)
		{
			result.WeaponFramework = null;
		}
		else
		{
			MessagePackObject lastReadData8 = unpacker.LastReadData;
			string weaponFramework = ((MessagePackObject)(ref lastReadData8)).AsString();
			result.WeaponFramework = weaponFramework;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<WeaponDisplayInfo Projectile={Projectile} ProjectileSpeed={ProjectileSpeed} DetonateDelay={DetonateDelay} WeaponFramework={WeaponFramework}>";
	}
}
