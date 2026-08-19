using MsgPack;
using MsgPack.Serialization;

public class GettextSerializer : MessagePackSerializer<Gettext>
{
	public GettextSerializer(SerializationContext ownerContext)
		: base(ownerContext)
	{
	}

	protected override void PackToCore(Packer packer, Gettext value)
	{
	}

	protected override Gettext UnpackFromCore(Unpacker unpacker)
	{
		return LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
	}
}
