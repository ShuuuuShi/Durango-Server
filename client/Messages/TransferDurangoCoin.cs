using MsgPack;

namespace Messages;

public struct TransferDurangoCoin
{
	public const uint TypeCode = 5092384u;

	public string RecipientEntityId;

	public long Amount;

	public static void Pack(Packer packer, TransferDurangoCoin val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(5092384u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.RecipientEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RecipientEntityId);
		}
		packer.Pack(val.Amount);
	}

	public static TransferDurangoCoin Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TransferDurangoCoin result = default(TransferDurangoCoin);
		result.RecipientEntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Amount = unpacker.LastReadData.AsInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<TransferDurangoCoin RecipientEntityId={RecipientEntityId} Amount={Amount}>";
	}
}
