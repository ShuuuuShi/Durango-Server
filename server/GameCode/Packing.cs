using System;
using System.Reflection;
using MsgPack;

internal sealed class Packing<T> : PackingBase
{
	public PackFunc<T> _pack;

	public UnpackFunc<T> _unpack;

	public Handler<T> _handler;

	public MethodInfo _packerInfo;

	public MethodInfo _unpackerInfo;

	public Packing(uint typeCode, PackFunc<T> pack, UnpackFunc<T> unpack, Handler<T> handler)
		: base(typeCode)
	{
		_pack = pack;
		_unpack = unpack;
		_handler = handler;
	}

	public override bool HandleMsgPack(Unpacker unpacker)
	{
		if (_unpack == null || _handler == null)
		{
			return false;
		}
		T msg = _unpack(unpacker);
		_handler(msg);
		return true;
	}

	public override Type GetMsgType()
	{
		return typeof(T);
	}
}
