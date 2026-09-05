using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

		/// <summary>
		/// ตั้งค่านี้ = บอกว่า response นี้ **แคชได้** (ข้อมูลนิ่ง ไม่เปลี่ยนตามเวลา)
		///
		/// 🐛 [แก้เอง] 30 ส.ค. 2026 — ต้นตออาการ "เกมกระตุกเป็นระยะเวลาเดิน"
		/// เดิมทุก response ถูกยัด `Cache-Control: no-cache, no-store` เหมือนกันหมด
		/// `no-store` = สั่ง BestHTTP (ตัว HTTP ของเกม) **ห้ามเก็บแคชเด็ดขาด**
		/// ⇒ ข้อมูล terrain ที่ไม่เคยเปลี่ยนเลย ถูกดาวน์โหลดใหม่ทุกก้อนทุกครั้งที่เดินข้ามขอบ chunk
		/// (ดู Durango.Utils/Http.cs — เกมเรียก `RequestChunk(..., disableCache: false)` ตั้งใจให้แคชอยู่แล้ว)
		///
		/// มีค่า = ส่ง `ETag` + `Cache-Control: public, max-age=..., immutable` แทน
		/// และถ้า client ส่ง `If-None-Match` ตรงกัน จะตอบ 304 ตัวเปล่า ไม่ส่ง body ซ้ำ
		/// </summary>
		public string ETag;

		/// <summary>แคชได้นานกี่วินาที (ใช้เมื่อ <see cref="ETag"/> มีค่า)</summary>
		public int MaxAgeSeconds = 86400;

		/// <summary>
		/// ตั้งค่านี้ = ตอบ redirect ไปที่อยู่นี้ (ใส่หัว Location)
		/// [4 ก.ย. 2026] ใช้ส่งงานโหลด asset bundle ไปให้ nginx แทนที่จะให้ process เกมอ่านไฟล์เอง
		/// </summary>
		public string Location;

		public abstract void Write(Stream stream);
	}

	/// <summary>พา client ไปโหลดที่อื่น (302) — ตัวเกมของ Unity ตาม redirect ให้เอง</summary>
	public class RedirectResponse : Response
	{
		public RedirectResponse(string location)
		{
			Location = location;
			ContentType = "text/plain";
			StatusCode = HttpStatusCode.Found;
		}

		public override void Write(Stream stream) { }
	}

	/// <summary>ตอบ 304 ตัวเปล่า — client ใช้ของในแคชตัวเองต่อได้เลย</summary>
	public class NotModifiedResponse : Response
	{
		public NotModifiedResponse(string etag, int maxAgeSeconds)
		{
			ContentType = null;
			StatusCode = HttpStatusCode.NotModified;
			ETag = etag;
			MaxAgeSeconds = maxAgeSeconds;
		}

		public override void Write(Stream stream) { }
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
			: base("application/json", content, statusCode)
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

	/// <summary>
	/// ส่งไฟล์จากดิสก์แบบ "อ่านทีละก้อน" ไม่โหลดทั้งไฟล์เข้าหน่วยความจำ
	///
	/// 🐛 [4 ก.ย. 2026] ต้นตออาการ "เซิร์ฟหน่วงหนักตอนมือถือหลายเครื่องโหลด bundle"
	///    เดิมใช้ BinaryReponse + File.ReadAllBytes ⇒ ไฟล์ละหลาย MB กลายเป็น byte[] ก้อนใหญ่
	///    ตกไป Large Object Heap ทุกครั้ง ⇒ GC เต็มไปหมด ลูปเกมหยุดเดิน (tps 120 → 2)
	///    วัดจริงบนเซิร์ฟ: ผู้เล่น 13 คน + มือถือโหลด bundle = 2 tps
	///    สตรีมทีละ 64 KB แทน — หน่วยความจำคงที่ ไม่แตะ LOH
	/// </summary>
	public class FileResponse : Response
	{
		private readonly string _path;

		public FileResponse(string path, string etag = null, int maxAgeSeconds = 2592000)
		{
			_path = path;
			ContentType = "application/octet-stream";
			StatusCode = HttpStatusCode.OK;
			ETag = etag;
			MaxAgeSeconds = maxAgeSeconds;
		}

		public override void Write(Stream stream)
		{
			using (FileStream file = new FileStream(_path, FileMode.Open, FileAccess.Read,
				FileShare.Read, 65536, FileOptions.SequentialScan))
			{
				file.CopyTo(stream, 65536);
			}
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
	private const int MaxRequestBodyBytes = 128 * 1024;

	private static string _lastLogLine;
	private static int _lastLogCount;
	private static readonly object _logSync = new object();

	/// <summary>
	/// พิมพ์ log แบบยุบบรรทัดที่ซ้ำติดกัน — บรรทัดเดิมซ้ำจะขึ้นเป็น "… x3" แทนการพิมพ์ซ้ำเรื่อย ๆ
	///
	/// [เพิ่มเอง] 31 ส.ค. 2026 — Server Panel ยิง /admin/status ทุก 3 วินาที
	/// ⇒ log เต็มไปด้วย "[web] GET /admin/status -> 200" จนมองไม่เห็นอย่างอื่นเลย
	/// (ปัญหาเดียวกับ "no handler type=2448" ที่ท่วม 587 บรรทัด/ชม. ซึ่งแก้ไปแล้ว)
	///
	/// ยุบเฉพาะบรรทัดที่ซ้ำ **ติดกัน** เท่านั้น — ถ้ามีอย่างอื่นคั่น จะเริ่มนับใหม่
	/// จึงไม่กลบลำดับเหตุการณ์จริง
	/// </summary>
	private static void LogRepeatable(string line)
	{
		lock (_logSync)
		{
			if (line == _lastLogLine)
			{
				_lastLogCount++;
				return;
			}
			FlushRepeatNoLock();
			_lastLogLine = line;
			_lastLogCount = 1;
			Console.WriteLine(line);
		}
	}

	/// <summary>พิมพ์สรุปจำนวนซ้ำที่ค้างอยู่ (เรียกก่อนพิมพ์บรรทัดอื่น)</summary>
	private static void FlushRepeatNoLock()
	{
		if (_lastLogCount > 1)
		{
			Console.WriteLine("      ↑ ซ้ำอีก x{0}", _lastLogCount - 1);
		}
		_lastLogCount = 0;
		_lastLogLine = null;
	}

	/// <summary>
	/// แกะ body แบบ multipart/form-data ให้เป็น key/value เหมือน url-encoded
	///
	/// [เพิ่มเอง] 31 ส.ค. 2026 — ตัวเกมส่งแบบนี้มาเฉพาะตอน Mode.Online (ดูคอมเมนต์ที่จุดเช็ค content-type)
	/// รับเฉพาะ field ข้อความล้วน ๆ พอ — ตัวเกมไม่เคยอัปโหลดไฟล์มาทาง endpoint พวกนี้
	/// รูปแบบ: --boundary CRLF ส่วนหัว CRLF CRLF ค่า CRLF --boundary ... --boundary--
	/// </summary>
	private static Dictionary<string, string> ParseMultipartForm(string body, string contentType)
	{
		int at = contentType.IndexOf("boundary=", StringComparison.OrdinalIgnoreCase);
		if (at < 0)
		{
			return null;
		}
		string boundary = contentType.Substring(at + "boundary=".Length).Trim().Trim('"');
		int semi = boundary.IndexOf(';');
		if (semi >= 0)
		{
			boundary = boundary.Substring(0, semi).Trim();
		}
		if (boundary.Length == 0)
		{
			return null;
		}
		Dictionary<string, string> fields = new Dictionary<string, string>();
		string[] parts = body.Split(new[] { "--" + boundary }, StringSplitOptions.None);
		foreach (string part in parts)
		{
			int headerEnd = part.IndexOf("\r\n\r\n", StringComparison.Ordinal);
			if (headerEnd < 0)
			{
				continue;   // ชิ้นเปิด/ปิด ไม่มีเนื้อหา
			}
			string headers = part.Substring(0, headerEnd);
			int nameAt = headers.IndexOf("name=\"", StringComparison.OrdinalIgnoreCase);
			if (nameAt < 0)
			{
				continue;
			}
			nameAt += "name=\"".Length;
			int nameEnd = headers.IndexOf('"', nameAt);
			if (nameEnd < 0)
			{
				continue;
			}
			string name = headers.Substring(nameAt, nameEnd - nameAt);
			string value = part.Substring(headerEnd + 4);
			if (value.EndsWith("\r\n", StringComparison.Ordinal))
			{
				value = value.Substring(0, value.Length - 2);
			}
			fields[name] = value;
		}
		return (fields.Count > 0) ? fields : null;
	}
	private const int MaxQueuedContexts = 256;

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
						if (_contextQueue.Count >= MaxQueuedContexts)
						{
							try { item.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable; item.Response.Close(); } catch (Exception) { }
							return;
						}
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
		List<HttpListenerContext> pending = null;
		lock (_contextQueue)
		{
			if (_contextQueue.Count > 0)
			{
				pending = new List<HttpListenerContext>(_contextQueue.Count);
				while (_contextQueue.Count > 0)
				{
					pending.Add(_contextQueue.Dequeue());
				}
			}
		}
		if (pending != null)
		{
			for (int i = 0; i < pending.Count; i++)
			{
				HttpListenerContext httpListenerContext = pending[i];
				try
				{
					Response value = Process(httpListenerContext);
					_responseList.AddLast(new KeyValuePair<HttpListenerContext, Response>(httpListenerContext, value));
				}
				catch (Exception ex)
				{
					Console.WriteLine("[web] route error: " + ex);
					try { httpListenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError; httpListenerContext.Response.Close(); } catch (Exception) { }
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
				// ของที่แคชได้: ถอดหัว no-store ที่ใส่ไว้เป็นค่าปกติออก แล้วใส่ ETag แทน
				// ถ้า client มีของเดิมอยู่แล้ว (If-None-Match ตรง) ตอบ 304 ตัวเปล่าพอ
				if (!string.IsNullOrEmpty(value3.ETag))
				{
					string ifNoneMatch = key.Request.Headers["If-None-Match"];
					if (ifNoneMatch == value3.ETag)
					{
						value3 = new NotModifiedResponse(value3.ETag, value3.MaxAgeSeconds);
					}
					key.Response.Headers.Remove("Pragma");
					key.Response.Headers.Remove("Expires");
					key.Response.Headers["Cache-Control"] = "public, max-age=" + value3.MaxAgeSeconds + ", immutable";
					key.Response.Headers["ETag"] = value3.ETag;
				}
				if (!string.IsNullOrEmpty(value3.Location))
				{
					key.Response.Headers["Location"] = value3.Location;
				}
				key.Response.StatusCode = (int)value3.StatusCode;
				if (value3.ContentType != null) { key.Response.ContentType = value3.ContentType; }
				// [แก้เอง] 4 ก.ย. 2026 — CORS เฉพาะ /admin/* เท่านั้น
				// เซิร์ฟเป็น island-mode แล้ว (1 เกาะ = 1 process = 1 พอร์ต) หน้า admin ของเกาะหนึ่ง
				// ต้อง fetch สถานะของอีกเกาะข้ามพอร์ตได้ ไม่งั้นเบราว์เซอร์บล็อกทิ้งเงียบ ๆ
				// ปลอดภัยเพราะทุก endpoint ใต้ /admin/ ยังต้องมี ?token= ที่ถูกต้องเหมือนเดิม
				// (GET กับ POST แบบ form-urlencoded เป็น "simple request" จึงไม่ต้องรับ preflight OPTIONS)
				try
				{
					string adminPath = key.Request.Url?.AbsolutePath;
					if (adminPath != null && adminPath.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
					{
						key.Response.Headers["Access-Control-Allow-Origin"] = "*";
					}
				}
				catch (Exception) { /* Url พังก็แค่ไม่ใส่หัว CORS */ }
				// [แก้เอง] เขียนลง MemoryStream ก่อนแล้วค่อยตั้ง ContentLength64 — ถ้าไม่ตั้ง
				// HttpListenerResponse จะ fallback เป็น Transfer-Encoding: chunked เสมอ
				// ซึ่ง BestHTTP (เวอร์ชันเก่าที่เกมใช้) parse ไม่ผ่าน ทำให้ IsSuccess เป็น false
				// ทั้งที่ status 200 จริง (เกมค้างที่ title พร้อม error "(Knock)" เสมอ)
				using (MemoryStream buffer = new MemoryStream())
				{
					value3.Write(buffer);
					byte[] bytes = buffer.ToArray();
					// [แก้เอง] 31 ส.ค. 2026 — บีบ gzip ให้ถ้า client ขอมา
					//
					// พอเปิด Mode.Online เกมเลิกอ่านไฟล์ข้อมูลจากดิสก์ในเครื่อง มาโหลดจากเซิร์ฟแทน
					// = 15 MB ต่อการเข้าเกม 1 ครั้ง ทีละไฟล์ ⇒ วัดจริงได้ 75 วินาทีตอนสร้างตัวละคร
					//
					// ตัวเกมส่ง "Accept-Encoding: gzip" มาให้อยู่แล้ว (client Durango.Utils/Http.cs:29)
					// แต่เราไม่เคยบีบให้ — JSON บีบแล้วเหลือ ~7% (2,022 KB → 157 KB)
					//
					// ข้ามไฟล์เล็กกว่า 1 KB (บีบแล้วมักโตกว่าเดิม) และข้าม octet-stream ซึ่งคือ
					// assetbundle (.bundle บีบ LZ4 มาแล้ว บีบซ้ำได้ไม่กี่ % แต่กิน CPU ทุกคำขอ)
					string acceptEncoding = key.Request.Headers["Accept-Encoding"];
					bool wantsGzip = acceptEncoding != null
						&& acceptEncoding.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) >= 0
						&& bytes.Length >= 1024
						&& value3.ContentType != null
						&& value3.ContentType.IndexOf("octet-stream", StringComparison.OrdinalIgnoreCase) < 0;
					if (wantsGzip)
					{
						using (MemoryStream packed = new MemoryStream())
						{
							using (GZipStream gzip = new GZipStream(packed, CompressionLevel.Fastest, leaveOpen: true))
							{
								gzip.Write(bytes, 0, bytes.Length);
							}
							bytes = packed.ToArray();
						}
						key.Response.Headers["Content-Encoding"] = "gzip";
					}
					key.Response.ContentLength64 = bytes.Length;
					key.Response.OutputStream.Write(bytes, 0, bytes.Length);
				}
				LogRepeatable(string.Format("[web] {0} {1} -> {2}", key.Request.HttpMethod, key.Request.Url.PathAndQuery, (int)value3.StatusCode));
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

	/// <summary>
	/// [3 ก.ย. 2026] prefix ของ path ที่ให้ตัดทิ้งก่อน route (เช่น "/p8190") — ตั้งด้วย --url-prefix
	///
	/// ทำไม: เกมมือถือของแท้ประกอบ URL gateway จาก literal "http://127.0.0.1:" + เลขพอร์ต 8190 ที่เป็น int ในโค้ด
	/// (แพตช์ string ไม่ได้) ⇒ จะให้มือถือต่อเซิร์ฟ "ทดสอบ" ที่พอร์ตอื่นบน IP เดียวกัน ทำได้ทางเดียวคือแพตช์ literal
	/// เป็น "http://ip:8290/p" แล้วปล่อยให้เกมต่อท้าย "8190" เอง → ทุก request มาเป็น /p8190/knock, /p8190/entry …
	/// เซิร์ฟตัดคำนำหน้านี้ทิ้งแล้ว route ตามปกติ · request ที่ไม่มี prefix (client PC) ผ่านเหมือนเดิม
	/// </summary>
	public static string PathPrefix;

	private static string StripPrefix(string path)
	{
		string p = PathPrefix;
		if (string.IsNullOrEmpty(p) || path == null || !path.StartsWith(p, StringComparison.Ordinal))
		{
			return path;
		}
		string rest = path.Substring(p.Length);
		return rest.StartsWith("/") ? rest : "/" + rest;
	}

	private Response Process(HttpListenerContext context)
	{
		string absolutePath = StripPrefix(context.Request.Url.AbsolutePath);
		bool flag = false;
		Dictionary<string, RouteFunction> dictionary = null;
		Dictionary<string, string> dictionary2 = null;
		if (context.Request.HttpMethod.Equals("GET"))
		{
			dictionary = GetRoute;
		}
			else if (context.Request.HttpMethod.Equals("POST"))
			{
				// [แก้เอง] 31 ส.ค. 2026 — เดิม 400 เงียบ ๆ ไม่บอกว่าเพราะอะไร
				// ตอนไล่บั๊ก "สร้างตัวละครไม่ได้ (Request Failed)" เห็นแค่ `POST /players -> 400`
				// ในล็อก เดาไม่ถูกเลยว่าเป็นเพราะ content-type หรือ body ใหญ่เกิน ⇒ พิมพ์เหตุผลออกมา
				//
				// 🐛 ต้นตอของบั๊กนั้น: client ตั้ง `FormUsage = UrlEncoded` **เฉพาะตอนไม่ใช่ Mode.Online**
				//    (ดู client Durango.Utils/Http.cs:25 — `if (GameManager.ClusterMode != 0)` และ
				//    `Mode.Online == 0`) พอเปิดโหมด Online เลยไม่ตั้ง ⇒ BestHTTP ส่งเป็น multipart/form-data
				//    แทน ⇒ เซิร์ฟที่รับแต่ url-encoded ตอบ 400 ⇒ "Character could not be created"
				//    รับ multipart เพิ่มตรงนี้แทนการไปแก้ฝั่งเกม จะได้ไม่ต้องบังคับผู้เล่นอัปเดตไคลเอนต์
				string contentType = context.Request.ContentType;
				bool isUrlEncoded = contentType == null
					|| contentType.IndexOf("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) >= 0;
				bool isMultipart = contentType != null
					&& contentType.IndexOf("multipart/form-data", StringComparison.OrdinalIgnoreCase) >= 0;
				if (!isUrlEncoded && !isMultipart)
				{
					Console.WriteLine("[web] 400 {0} — content-type ไม่รองรับ: {1}", absolutePath, contentType);
					flag = true;
				}
				else if (context.Request.ContentLength64 < 0 || context.Request.ContentLength64 > MaxRequestBodyBytes)
				{
					Console.WriteLine("[web] 400 {0} — ขนาด body ผิดพลาด: {1} bytes (ลิมิต {2})",
						absolutePath, context.Request.ContentLength64, MaxRequestBodyBytes);
					flag = true;
				}
				else
				{
					using StreamReader streamReader = new StreamReader(context.Request.InputStream);
					char[] chars = new char[MaxRequestBodyBytes];
					int read = 0;
					while (read < chars.Length)
					{
						int n = streamReader.Read(chars, read, chars.Length - read);
						if (n == 0) break;
						read += n;
					}
					string text = new string(chars, 0, read);
					if (isMultipart)
					{
						dictionary2 = ParseMultipartForm(text, contentType);
					}
					else
					{
						string[] array = text.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
						if (array.Length > 0) dictionary2 = new Dictionary<string, string>();
						foreach (string text2 in array)
						{
							int separator = text2.IndexOf('=');
							if (separator <= 0) continue;
							string key = Uri.UnescapeDataString(text2.Substring(0, separator));
							string value = Uri.UnescapeDataString(text2.Substring(separator + 1).Replace("+", " "));
							dictionary2[key] = value;
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
			value2 = this.UnhandledUrl(StripPrefix(context.Request.Url.PathAndQuery));
		}
		if (value2 == null)
		{
			return new NotFountResponse();
		}
		Response response = value2(context.Request, dictionary2);
		return (response == null) ? new NotFountResponse() : response;
	}
}
