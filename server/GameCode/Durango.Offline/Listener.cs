using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Durango.Offline;

public class Listener
{
	private Socket _listenSocket;

	private SocketAsyncEventArgs _acceptArgs;

	private volatile Socket _acceptedSocket;

	private volatile bool _acceptWaiting;

	// GP-15: bind สำเร็จหรือยัง — ถ้าไม่สำเร็จต้องไม่แตะ socket อีกเลย
	// ไม่งั้น Process() จะเรียก Accept() ทุก tick แล้วพ่น ArgumentNullException รัว ๆ
	private volatile bool _started;

	// GP-15: กัน callback ที่ยิงหลัง Close() ไปแตะของที่ถูกทิ้งแล้ว
	private volatile bool _closing;

	public bool IsListening => _started;

	public event Action<Socket> ClientAccepted;

	/// <summary>เปิดฟังพอร์ต คืน false ถ้า bind ไม่สำเร็จ (พอร์ตไม่ว่าง/ไม่มีสิทธิ์)</summary>
	public bool Start(int port)
	{
		Close();
		_closing = false;
		_listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		IPEndPoint local_end = new IPEndPoint(IPAddress.Any, port);
		try
		{
			// เปิดเซิร์ฟซ้ำสองตัวต้อง "ตัวที่สอง bind ไม่ผ่าน" ให้รู้ตัวทันที
			// ไม่งั้น Windows ยอมให้สอง process ฟังพอร์ตเดียวกันได้ แล้วแบ่งกันรับ connection
			// ⇒ ผู้เล่นครึ่งหนึ่งไปโผล่อีกโลกหนึ่งที่ไม่มีเซฟร่วมกัน (เคยหลอกตอนเทสมาแล้ว)
			_listenSocket.ExclusiveAddressUse = true;
			_listenSocket.Bind(local_end);
			_listenSocket.Listen(10);
			_acceptArgs = new SocketAsyncEventArgs();
			_acceptArgs.Completed += Accept_Completed;
			_started = true;
			Accept();
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			_started = false;
			try
			{
				_listenSocket?.Close();
			}
			catch (Exception)
			{
			}
			_listenSocket = null;
			return false;
		}
	}

	public void Close()
	{
		_closing = true;
		_started = false;
		if (_listenSocket != null)
		{
			try
			{
				_listenSocket.Close();
			}
			catch (Exception)
			{
			}
			_listenSocket = null;
		}
		if (_acceptedSocket != null)
		{
			// GP-15: เดิมเป็น try/finally ไม่มี catch — client ที่หลุดไปก่อนจะทำให้
			// Shutdown throw ทะลุออกจาก Close() แล้วตัวเรียกไม่ได้ปิดอย่างอื่นต่อ
			try
			{
				_acceptedSocket.Shutdown(SocketShutdown.Both);
			}
			catch (Exception)
			{
			}
			try
			{
				_acceptedSocket.Close();
			}
			catch (Exception)
			{
			}
			_acceptedSocket = null;
		}
		if (_acceptArgs != null)
		{
			_acceptArgs.Completed -= Accept_Completed;
			_acceptArgs.Dispose();
			_acceptArgs = null;
		}
	}

	public void Process()
	{
		if (!_started)
		{
			return;
		}
			Socket accepted = _acceptedSocket;
			if (accepted != null && ReferenceEquals(Interlocked.CompareExchange(ref _acceptedSocket, null, accepted), accepted))
			{
				try
				{
					ClientAccepted?.Invoke(accepted);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					try { accepted.Close(); } catch (Exception) { }
				}
			}
		if (!_acceptWaiting)
		{
			Accept();
		}
	}

	private void Accept()
	{
		if (!_started || _closing || _listenSocket == null || _acceptArgs == null)
		{
			return;
		}
		_acceptWaiting = true;
		try
		{
			if (!_listenSocket.AcceptAsync(_acceptArgs))
			{
				Accept_Completed(_listenSocket, _acceptArgs);
			}
		}
		catch (Exception exception)
		{
			// socket ถูกปิดระหว่างนี้ (ObjectDisposedException) หรือ error อื่น — หยุดรับ ไม่วนพ่น error
			Debug.LogException(exception);
			_started = false;
			_acceptWaiting = false;
		}
	}

	private void Accept_Completed(object sender, SocketAsyncEventArgs e)
	{
		if (_closing)
		{
			// GP-15: callback ที่ค้างมาจาก AcceptAsync ตอนกำลังปิด — เก็บกวาดแล้วจบ
			e.AcceptSocket?.Close();
			e.AcceptSocket = null;
			_acceptWaiting = false;
			return;
		}
		if (e.SocketError == SocketError.Success)
		{
			_acceptedSocket = e.AcceptSocket;
		}
		else if (e.SocketError != SocketError.OperationAborted)
		{
			Debug.LogWarning("[listener] accept failed: " + e.SocketError);
		}
		e.AcceptSocket = null;
		_acceptWaiting = false;
	}
}
