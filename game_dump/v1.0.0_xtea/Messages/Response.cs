using MsgPack;
using MsgPack.Serialization;

namespace Messages;

public struct Response
{
	public const uint TypeCode = 501u;

	public bool Success;

	public MessagePackObjectDictionary Data;

	public static void Pack(Packer packer, Response val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(501u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Success);
		MessagePackSerializer<MessagePackObjectDictionary> val2 = MessagePackSerializer.Get<MessagePackObjectDictionary>();
		val2.PackTo(packer, val.Data);
	}

	public static Response Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Response result = default(Response);
		result.Success = ((MessagePackObject)(ref lastReadData)).AsBoolean();
		unpacker.Read();
		MessagePackSerializer<MessagePackObjectDictionary> serializer = SerializationContext.Default.GetSerializer<MessagePackObjectDictionary>();
		result.Data = serializer.UnpackFrom(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<Response Success={Success} Data={Data}>";
	}
}
