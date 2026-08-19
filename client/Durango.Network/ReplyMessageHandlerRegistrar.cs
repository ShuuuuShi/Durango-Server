using System;

namespace Durango.Network;

public struct ReplyMessageHandlerRegistrar
{
	public static ReplyMessageHandlerRegistrar Empty;

	private readonly Connection _connection;

	private readonly uint _seq;

	public ReplyMessageHandlerRegistrar(Connection connection, uint seq)
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

	public ReplyMessageHandlerRegistrar All(Action<Packet> handler)
	{
		if (_connection != null && handler != null)
		{
			_connection.RegisterReplyMessageHandler(_seq, handler, allowReplied: true);
		}
		return this;
	}

	public ReplyMessageHandlerRegistrar Rest(Action<Packet> handler)
	{
		if (_connection != null && handler != null)
		{
			_connection.RegisterReplyMessageHandler(_seq, handler, allowReplied: false);
		}
		return this;
	}

	public ReplyMessageHandlerRegistrar OnSequence(Action<bool> handler)
	{
		if (_connection != null && handler != null)
		{
			_connection.RegisterReplySequenceHandler(_seq, handler);
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
