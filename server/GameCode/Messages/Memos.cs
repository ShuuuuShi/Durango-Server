using System.Collections;
using System.Collections.Generic;
using MsgPack;
using Shared.Memo;

namespace Messages;

public struct Memos
{
	public const uint TypeCode = 2440u;

	public Dictionary<MemoType, BitArray> CollectedMemos;

	public static void Pack(Packer packer, Memos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2440u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.CollectedMemos == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.CollectedMemos.Count);
		foreach (KeyValuePair<MemoType, BitArray> collectedMemo in val.CollectedMemos)
		{
			packer.Pack((int)collectedMemo.Key);
			BitArray bitArray = new BitArray(collectedMemo.Value);
			for (int i = 0; i < bitArray.Length; i += 8)
			{
				for (int j = 0; j < 4; j++)
				{
					bool value = bitArray[i + j];
					bitArray[i + j] = bitArray[i + 8 - j - 1];
					bitArray[i + 8 - j - 1] = value;
				}
			}
			byte[] array = new byte[bitArray.Count / 8];
			bitArray.CopyTo(array, 0);
			packer.PackBinary(array);
		}
	}

	public static Memos Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Memos result = default(Memos);
		result.CollectedMemos = new Dictionary<MemoType, BitArray>(num, default(MemoTypeComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			MemoType key = ((num2 >= 0 && 1 >= num2) ? ((MemoType)num2) : MemoType.Invalid);
			unpacker.Read();
			byte[] bytes = unpacker.LastReadData.AsBinary();
			BitArray bitArray = new BitArray(bytes);
			for (int j = 0; j < bitArray.Length; j += 8)
			{
				for (int k = 0; k < 4; k++)
				{
					bool value = bitArray[j + k];
					bitArray[j + k] = bitArray[j + 8 - k - 1];
					bitArray[j + 8 - k - 1] = value;
				}
			}
			result.CollectedMemos.Add(key, bitArray);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Memos CollectedMemos={CollectedMemos}>";
	}
}
