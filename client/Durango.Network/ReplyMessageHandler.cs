using System;
using System.Collections.Generic;

namespace Durango.Network;

public struct ReplyMessageHandler
{
	public Dictionary<uint, Connection.PacketHandler> Dictionary;

	public Action<Packet> Handler;

	public bool AllowReplied;

	public Action<bool> Sequence;
}
