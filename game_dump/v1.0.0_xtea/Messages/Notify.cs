using MsgPack;
using MsgPack.Serialization;

namespace Messages;

public struct Notify
{
	public const uint TypeCode = 503u;

	public int Method;

	public MessagePackObjectDictionary Data;

	public static void Pack(Packer packer, Notify val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(503u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Method);
		MessagePackSerializer<MessagePackObjectDictionary> val2 = MessagePackSerializer.Get<MessagePackObjectDictionary>();
		val2.PackTo(packer, val.Data);
	}

	public static Notify Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Notify result = default(Notify);
		result.Method = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackSerializer<MessagePackObjectDictionary> serializer = SerializationContext.Default.GetSerializer<MessagePackObjectDictionary>();
		result.Data = serializer.UnpackFrom(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<Notify Method={Method} Data={Data}>";
	}
}
