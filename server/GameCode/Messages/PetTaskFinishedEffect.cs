using MsgPack;
using Shared.System;

namespace Messages;

public struct PetTaskFinishedEffect
{
	public const uint TypeCode = 2087u;

	public Shared.System.RewardEffect Type;

	public string TaskId;

	public int PetExp;

	public static void Pack(Packer packer, PetTaskFinishedEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2087u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Type);
		if (val.TaskId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TaskId);
		}
		packer.Pack(val.PetExp);
	}

	public static PetTaskFinishedEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		PetTaskFinishedEffect result = default(PetTaskFinishedEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.TaskId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.PetExp = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<PetTaskFinishedEffect Type={Type} TaskId={TaskId} PetExp={PetExp}>";
	}
}
