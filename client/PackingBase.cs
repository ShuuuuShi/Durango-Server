using System;
using MsgPack;

internal abstract class PackingBase
{
	private uint _typeCode;

	public PackingBase(uint typeCode)
	{
		_typeCode = typeCode;
	}

	public uint GetTypeCode()
	{
		return _typeCode;
	}

	public abstract bool HandleMsgPack(Unpacker unpacker);

	public abstract Type GetMsgType();
}
