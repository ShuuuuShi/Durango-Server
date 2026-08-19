using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Durango.Offline;

public class WebServer
{
	public delegate Response RouteFunction(HttpListenerRequest request, Dictionary<string, string> postData);

	public abstract class Response
	{
		public string ContentType;

		public HttpStatusCode StatusCode;

		public abstract void Write(Stream stream);
	}

	public class TextResponse : Response
	{
		private readonly string _content;

		public TextResponse(string contentType, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			ContentType = contentType + "; charset=utf-8";
			StatusCode = statusCode;
			_content = content;
		}

		public override void Write(Stream stream)
		{
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write(_content);
		}
	}

	public class JsonResponse : TextResponse
	{
		public JsonResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
			: base("json", content, statusCode)
		{
		}
	}

	public class BadRequestResponse : TextResponse
	{
		public BadRequestResponse()
			: base("text/html", "400 Bad Request", HttpStatusCode.BadRequest)
		{
		}
	}

	public class NotFountResponse : TextResponse
	{
		public NotFountResponse()
			: base("text/html", "404 Not Found", HttpStatusCode.NotFound)
		{
		}
	}

	public class BinaryReponse : Response
	{
		public byte[] Content;

		public BinaryReponse()
		{
			ContentType = "application/octet-stream";
			StatusCode = HttpStatusCode.OK;
		}

		public override void Write(Stream stream)
		{
			stream.Write(Content, 0, Content.Length);
		}
	}

	private readonly Queue<HttpListenerContext> _contextQueue = new Queue<HttpListenerContext>();

	private readonly LinkedList<KeyValuePair<HttpListenerContext, Response>> _responseList = new LinkedList<KeyValuePair<HttpListenerContext, Response>>();

	private readonly HttpListener _listener;

	private readonly UdpClient _knockListener;

	private volatile bool _listenReady;

	private volatile bool _listenKnockReady;

	public Dictionary<string, RouteFunction> GetRoute { get; private set; }

	public Dictionary<string, RouteFunction> PostRoute { get; private set; }

	public event Func<string, RouteFunction> UnhandledUrl;

	public string Prefix
	{
		get
		{
			if (_listener == null)
			{
				return null;
			}
			return string.Join(", ", _listener.Prefixes);
		}
	}

	public WebServer(int port)
	{
		GetRoute = new Dictionary<string, RouteFunction>();
		PostRoute = new Dictionary<string, RouteFunction>();
		_listener = new HttpListener();
		// bind wildcard (*) ต้องรันเป็น admin มิฉะนั้น Access denied → fallback เป็น loopback
		// (localhost + 127.0.0.1 เพื่อให้ client ที่พิมพ์ IP 127.0.0.1 ต่อได้)
		try
		{
			_listener.Prefixes.Add($"http://*:{port}/");
			_listener.Start();
		}
		catch (Exception e)
		{
			_listener = new HttpListener();
			_listener.Prefixes.Add($"http://localhost:{port}/");
			_listener.Prefixes.Add($"http://127.0.0.1:{port}/");
			_listener.Start();
			Console.WriteLine("[webserver] wildcard bind denied ({0}), falling back to loopback", e.Message);
		}
		Listen();
		// UDP knock: port+1 (8191) — client broadcast "Knock:<id>" แล้วเอา hostname ไปแสดงในรายชื่อ server
		_knockListener = new UdpClient(port + 1);
		ListenKnock();
	}

	public void Close()
	{
		try
		{
			_listener.Close();
		}
		catch (Exception)
		{
		}
		try
		{
			_knockListener.Close();
		}
		catch (Exception)
		{
		}
	}

	private void Listen()
	{
		try
		{
			_listenReady = false;
			if (_listener.IsListening)
			{
				_listener.BeginGetContext(ListenerCallback, null);
			}
		}
		catch (Exception)
		{
			_listenReady = true;
		}
	}

	private void ListenerCallback(IAsyncResult result)
	{
		try
		{
			if (_listener.IsListening)
			{
				HttpListenerContext item = _listener.EndGetContext(result);
				lock (_contextQueue)
				{
					_contextQueue.Enqueue(item);
					return;
				}
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			_listenReady = true;
		}
	}

	private void ListenKnock()
	{
		try
		{
			_listenKnockReady = false;
			_knockListener.BeginReceive(KnockListenerCallback, null);
		}
		catch (Exception)
		{
			_listenKnockReady = true;
		}
	}

	private void KnockListenerCallback(IAsyncResult result)
	{
		if (_knockListener == null)
		{
			return;
		}
		try
		{
			IPEndPoint remoteEP = null;
			byte[] bytes = _knockListener.EndReceive(result, ref remoteEP);
			string text = Encoding.UTF8.GetString(bytes);
			if (text.StartsWith("Knock"))
			{
				string text2 = ((text.Length <= 6) ? string.Empty : text.Substring(6));
				if (text2 != "DurangoServerKnock")
				{
					string playerName = DurangoServer.Core.ServerKnock.HostName;
					if (string.IsNullOrEmpty(playerName))
					{
						playerName = "DurangoServer";
					}
					byte[] bytes2 = Encoding.UTF8.GetBytes(playerName);
					_knockListener.Send(bytes2, bytes2.Length, remoteEP);
				}
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			_listenKnockReady = true;
		}
	}

	public void Process()
	{
		if (_contextQueue.Count != 0)
		{
			lock (_contextQueue)
			{
				while (_contextQueue.Count != 0)
				{
					HttpListenerContext httpListenerContext = _contextQueue.Dequeue();
					try
					{
						Response value = Process(httpListenerContext);
						_responseList.AddLast(new KeyValuePair<HttpListenerContext, Response>(httpListenerContext, value));
					}
					catch (Exception ex)
					{
						Console.WriteLine("[web] route error: " + ex);
					}
				}
			}
		}
		LinkedListNode<KeyValuePair<HttpListenerContext, Response>> linkedListNode = _responseList.First;
		while (linkedListNode != null)
		{
			KeyValuePair<HttpListenerContext, Response> value2 = linkedListNode.Value;
			HttpListenerContext key = value2.Key;
			Response value3 = value2.Value;
			// ⚠️ ห้ามให้ exception หลุดออกจากลูปนี้ — มันวิ่งอยู่ใน main loop ของเซิร์ฟ
			// ถ้า client ตัดการเชื่อมต่อกลางคัน (curl แล้ว Ctrl+C, port scanner, เน็ตหลุด)
			// Write/Close จะโยน HttpListenerException แล้วเซิร์ฟทั้งใบดับ ผู้เล่นหลุดหมด
			try
			{
				key.Response.StatusCode = (int)value3.StatusCode;
				key.Response.ContentType = value3.ContentType;
				value3.Write(key.Response.OutputStream);
				Console.WriteLine("[web] {0} {1} -> {2}", key.Request.HttpMethod, key.Request.Url.PathAndQuery, (int)value3.StatusCode);
				key.Response.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine("[web] ส่ง response ไม่สำเร็จ (client หลุดไปแล้ว?): " + ex.Message);
				try
				{
					key.Response.Abort();
				}
				catch (Exception)
				{
				}
			}
			LinkedListNode<KeyValuePair<HttpListenerContext, Response>> node = linkedListNode;
			linkedListNode = linkedListNode.Next;
			_responseList.Remove(node);
		}
		if (_listenReady)
		{
			Listen();
		}
		if (_listenKnockReady)
		{
			ListenKnock();
		}
	}

	private Response Process(HttpListenerContext context)
	{
		string absolutePath = context.Request.Url.AbsolutePath;
		bool flag = false;
		Dictionary<string, RouteFunction> dictionary = null;
		Dictionary<string, string> dictionary2 = null;
		if (context.Request.HttpMethod.Equals("GET"))
		{
			dictionary = GetRoute;
		}
		else if (context.Request.HttpMethod.Equals("POST"))
		{
			if (context.Request.ContentType != null && context.Request.ContentType.IndexOf("application/x-www-form-urlencoded", StringComparison.Ordinal) == -1)
			{
				flag = true;
			}
			else
			{
				using StreamReader streamReader = new StreamReader(context.Request.InputStream);
				string text = streamReader.ReadToEnd();
				string[] array = text.Split(new char[1] { '&' }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length > 0)
				{
					dictionary2 = new Dictionary<string, string>();
				}
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					string[] array3 = text2.Split(new char[1] { '=' }, StringSplitOptions.RemoveEmptyEntries);
					if (array3.Length == 2)
					{
						string key = Uri.UnescapeDataString(array3[0]);
						string value = Uri.UnescapeDataString(array3[1].Replace("+", " "));
						if (dictionary2 != null)
						{
							dictionary2[key] = value;
						}
					}
				}
			}
			dictionary = PostRoute;
		}
		context.Response.Headers.Add(HttpResponseHeader.CacheControl, "no-cache, no-store, must-revalidate");
		context.Response.Headers.Add("Pragma", "no-cache");
		context.Response.Headers.Add(HttpResponseHeader.Expires, "0");
		if (flag)
		{
			return new BadRequestResponse();
		}
		RouteFunction value2 = null;
		dictionary?.TryGetValue(absolutePath, out value2);
		if (value2 == null && this.UnhandledUrl != null)
		{
			value2 = this.UnhandledUrl(context.Request.Url.PathAndQuery);
		}
		if (value2 == null)
		{
			return new NotFountResponse();
		}
		Response response = value2(context.Request, dictionary2);
		return (response == null) ? new NotFountResponse() : response;
	}
}
