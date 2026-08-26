using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Durango.Modding;
using Durango.Render.Screen;
using UnityEngine;

namespace DurangoMemoryBot
{

internal sealed class MemoryBotPending
{
    public MemoryBotRequest Request;
    public string Reply;
    public ManualResetEvent Done = new ManualResetEvent(false);
}

public sealed class MemoryBotRuntime : MonoBehaviour
{
    private const int DefaultPort = 8193;
    private const int MaxQueue = 32;
    private static readonly object Sync = new object();
    private static readonly Queue<MemoryBotPending> Queue = new Queue<MemoryBotPending>();
    private static MemoryBotRuntime _instance;
    private static IClientModApi _api;
    private static TcpListener _listener;
    private static Thread _listenerThread;
    private static bool _started;
    private static int _port;
    private static string _token;
    private static string _lastCommand = "";
    private static string _lastResult = "";

    public static void Start(IClientModApi api)
    {
        if (_started) return;
        _started = true;
        _api = api;
        _port = ReadPort();
        _token = Environment.GetEnvironmentVariable("DURANGO_MEMORYBOT_TOKEN") ?? "";
        GameObject go = new GameObject("__DurangoMemoryBot");
        UnityEngine.Object.DontDestroyOnLoad(go);
        _instance = go.AddComponent<MemoryBotRuntime>();
        _listenerThread = new Thread(ListenLoop);
        _listenerThread.IsBackground = true;
        _listenerThread.Start();
        api.Log("MemoryBot listening on 127.0.0.1:" + _port + " token=" + (_token.Length == 0 ? "off" : "on"));
    }

    private static int ReadPort()
    {
        int value;
        string text = Environment.GetEnvironmentVariable("DURANGO_MEMORYBOT_PORT");
        return int.TryParse(text, out value) && value > 0 && value < 65536 ? value : DefaultPort;
    }

    private static void ListenLoop()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start(1);
            while (true)
            {
                using (TcpClient client = _listener.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                {
                    stream.ReadTimeout = 1000;
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string response = HandleLine(line);
                        byte[] bytes = Encoding.UTF8.GetBytes(response + "\n");
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                    }
                }
            }
        }
        catch (Exception e)
        {
            if (_api != null) _api.Log("MemoryBot listener stopped: " + e.Message);
        }
    }

    private static string HandleLine(string line)
    {
        MemoryBotRequest request;
        string error;
        if (!MemoryBotProtocol.TryParse(line, out request, out error))
            return MemoryBotProtocol.Error("0", error);
        if (_token.Length > 0 && request.Token != _token)
            return MemoryBotProtocol.Error(request.RequestId, "unauthorized");
        MemoryBotPending pending = new MemoryBotPending { Request = request };
        lock (Sync)
        {
            if (Queue.Count >= MaxQueue) return MemoryBotProtocol.Error(request.RequestId, "queue_full");
            Queue.Enqueue(pending);
        }
        if (!pending.Done.WaitOne(5000)) return MemoryBotProtocol.Error(request.RequestId, "timeout");
        return pending.Reply ?? MemoryBotProtocol.Error(request.RequestId, "empty_reply");
    }

    private void Update()
    {
        MemoryBotPending pending = null;
        lock (Sync)
        {
            if (Queue.Count > 0) pending = Queue.Dequeue();
        }
        if (pending == null) return;
        _lastCommand = pending.Request.Op + (pending.Request.Path.Length > 0 ? " " + pending.Request.Path : " " + pending.Request.Name);
        try
        {
            string data;
            if (pending.Request.Op == "ping")
            {
                data = "{\"t\":" + F(Time.time) + ",\"port\":" + _port + ",\"main_thread\":true}";
            }
            else if (pending.Request.Op == "read")
            {
                int limit = 100;
                data = MemoryBotState.Read(pending.Request.Path, limit);
            }
            else if (pending.Request.Op == "command")
            {
                data = MemoryBotCommands.Execute(pending.Request);
            }
            else if (pending.Request.Op == "capture")
            {
                data = BeginCapture(pending.Request);
                if (data == null) data = "{\"status\":\"accepted\"}";
            }
            else
            {
                data = null;
                pending.Reply = MemoryBotProtocol.Error(pending.Request.RequestId, "unknown_op");
            }
            if (pending.Reply == null)
                pending.Reply = MemoryBotProtocol.Success(pending.Request.RequestId, "{\"captured_at\":" + F(Time.time) + ",\"data\":" + data + "}");
            _lastResult = pending.Reply;
        }
        catch (Exception e)
        {
            pending.Reply = MemoryBotProtocol.Error(pending.Request.RequestId, e.GetType().Name + ":" + e.Message);
            _lastResult = pending.Reply;
        }
        finally
        {
            pending.Done.Set();
        }
    }

    private string BeginCapture(MemoryBotRequest request)
    {
        string filename = request.Filename;
        if (string.IsNullOrEmpty(filename)) filename = "capture-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png";
        if (filename.IndexOf("..", StringComparison.Ordinal) >= 0 || filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidOperationException("invalid_capture_filename");
        if (!filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) filename += ".png";
        string dir = Path.Combine(Path.Combine(Application.dataPath, ".."), "MemoryBotCaptures");
        Directory.CreateDirectory(dir);
        string full = Path.Combine(dir, filename);
        MemoryBotPending pending = null;
        lock (Sync)
        {
            foreach (MemoryBotPending item in Queue) { pending = item; break; }
        }
        return CaptureNow(full);
    }

    private string CaptureNow(string full)
    {
        if (!GameManager.IsMainScene) return "{\"status\":\"rejected\",\"reason\":\"game_not_in_main_scene\"}";
        UnityEngine.ScreenCapture.CaptureScreenshot(full);
        return "{\"status\":\"accepted\",\"filename\":" + MemoryBotProtocol.Quote(full) + ",\"pending\":true}";
    }

    private void OnGUI()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("DURANGO_MEMORYBOT_OVERLAY"), "1", StringComparison.Ordinal)) return;
        GUI.color = Color.white;
        GUI.Label(new Rect(12f, 12f, 500f, 24f), "MemoryBot " + _port + " | " + _lastCommand);
        GUI.Label(new Rect(12f, 34f, 500f, 24f), "Last: " + _lastResult);
    }

    private static string F(float value) { return value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture); }
}
}
