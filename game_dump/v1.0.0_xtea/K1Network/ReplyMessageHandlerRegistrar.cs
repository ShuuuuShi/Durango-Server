namespace K1Network;

public struct ReplyMessageHandlerRegistrar
{
	public static ReplyMessageHandlerRegistrar Empty;

	private readonly Connection _connection;

	private readonly ulong _seq;

	public ReplyMessageHandlerRegistrar(Connection connection, ulong seq)
	{
		_connection = connection;
		_seq = seq;
	}

	public ReplyMessageHandlerRegistrar On<T>(Connection.MessageHandler<T> messageHandler)
	{
		if (_connection != null && messageHandler != null)
		{
			_connection.RegisterReplyMessageHandler(_seq, messageHandler);
		}
		return this;
	}

	public ReplyMessageHandlerRegistrar Ignore<T>()
	{
		return On<T>(delegate
		{
		});
	}

	public bool IsEmpty()
	{
		return _connection == null;
	}

	public static bool operator true(ReplyMessageHandlerRegistrar r)
	{
		return !r.IsEmpty();
	}

	public static bool operator false(ReplyMessageHandlerRegistrar r)
	{
		return r.IsEmpty();
	}
}
