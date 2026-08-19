using System.Collections.Generic;
using MsgPack;
using Shared.Ability;

namespace Messages;

public struct ResistanceExpCaps
{
	public const uint TypeCode = 349378783u;

	public Dictionary<Derived, ResistanceExpCap> Caps;

	public static void Pack(Packer packer, ResistanceExpCaps val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(349378783u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Caps == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Caps.Count);
		foreach (KeyValuePair<Derived, ResistanceExpCap> cap in val.Caps)
		{
			packer.Pack((int)cap.Key);
			ResistanceExpCap.Pack(packer, cap.Value);
		}
	}

	public static ResistanceExpCaps Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ResistanceExpCaps result = default(ResistanceExpCaps);
		result.Caps = new Dictionary<Derived, ResistanceExpCap>(num, default(DerivedComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Derived key = ((num2 >= 0 && 322 >= num2) ? ((Derived)num2) : Derived.Invalid);
			unpacker.Read();
			ResistanceExpCap value = ResistanceExpCap.Unpack(unpacker);
			result.Caps.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ResistanceExpCaps Caps={Caps}>";
	}
}
