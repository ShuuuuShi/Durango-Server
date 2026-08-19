using System;
using System.Net;
using System.Net.Sockets;

namespace Durango.Offline;

public class Listener
{
	private Socket _listenSocket;

	private SocketAsyncEventArgs _acceptArgs;

	private volatile Socket _acceptedSocket;

	private volatile bool _acceptWaiting;

	public event Action<Socket> ClientAccepted;

	public void Start(int port)
	{
		Close();
		_listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		IPEndPoint local_end = new IPEndPoint(IPAddress.Any, port);
		try
		{
			_listenSocket.Bind(local_end);
			_listenSocket.Listen(10);
			_acceptArgs = new SocketAsyncEventArgs();
			_acceptArgs.Completed += Accept_Completed;
			Accept();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void Close()
	{
		if (_listenSocket != null)
		{
			_listenSocket.Close();
			_listenSocket = null;
		}
		if (_acceptedSocket != null)
		{
			try
			{
				_acceptedSocket.Shutdown(SocketShutdown.Both);
			}
			finally
			{
				_acceptedSocket.Close();
			}
			_acceptedSocket = null;
		}
	}

	public void Process()
	{
		if (_acceptedSocket != null)
		{
			if (this.ClientAccepted != null)
			{
				this.ClientAccepted(_acceptedSocket);
			}
			_acceptedSocket = null;
		}
		if (!_acceptWaiting)
		{
			Accept();
		}
	}

	private void Accept()
	{
		_acceptWaiting = true;
		if (!_listenSocket.AcceptAsync(_acceptArgs))
		{
			Accept_Completed(_listenSocket, _acceptArgs);
		}
	}

	private void Accept_Completed(object sender, SocketAsyncEventArgs e)
	{
		if (e.SocketError == SocketError.Success)
		{
			_acceptedSocket = e.AcceptSocket;
		}
		e.AcceptSocket = null;
		_acceptWaiting = false;
	}
}
