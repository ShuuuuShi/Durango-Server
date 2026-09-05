using MsgPack;

namespace Messages;

public struct VisualEffects
{
	public const uint TypeCode = 123049871u;

	public string EntityId;

	public Pair<string, string>[] Effects;

	public string SkinEffect;

	public static void Pack(Packer packer, VisualEffects val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(123049871u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.Effects == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Effects.Length);
			for (int i = 0; i < val.Effects.Length; i++)
			{
				packer.PackArrayHeader(2);
				if (val.Effects[i].Item1 == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Effects[i].Item1);
				}
				if (val.Effects[i].Item2 == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Effects[i].Item2);
				}
			}
		}
		if (val.SkinEffect == null)
		{
			packer.PackNull();
		}
		else if (val.SkinEffect == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SkinEffect);
		}
	}

	public static VisualEffects Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		VisualEffects result = default(VisualEffects);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Effects = new Pair<string, string>[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			string item = unpacker.LastReadData.AsString();
			unpacker.Read();
			string item2 = unpacker.LastReadData.AsString();
			ref Pair<string, string> reference = ref result.Effects[i];
			reference = new Pair<string, string>(item, item2);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SkinEffect = null;
		}
		else
		{
			string skinEffect = unpacker.LastReadData.AsString();
			result.SkinEffect = skinEffect;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<VisualEffects EntityId={EntityId} Effects={Effects} SkinEffect={SkinEffect}>";
	}
}
