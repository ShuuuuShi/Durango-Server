using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Item;
using Shared.Region;
using Shared.Economy;
using Shared.Faction;
using Shared.Skill;
using Shared.Social;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// DurangoServer — ไฟล์หลักของ server
// ประกอบด้วย: ServerWorld (โลก), ServerPlayer (ผู้เล่น + handler เกมเพลย์),
// GameServer (TCP 8191), Gateway (HTTP 8190 + UDP knock), RadiotowerServer (แชท 8192)
// โปรโตคอล: MsgPack + Snappy, header 24 ไบต์ (time/seq/replyOf/typeCode/size)
// ============================================================================

// RadiotowerServer — ดูรายละเอียดที่ docs/server/RadiotowerServer.md

public class RadiotowerServer
{
    public const int DefaultPort = 8192;

    /// <summary>พอร์ตที่เปิดฟังจริง</summary>
    public int Port { get; private set; } = DefaultPort;

    private readonly Listener _listener = new Listener();
    private readonly List<Durango.Offline.Connection> _connections = new List<Durango.Offline.Connection>();
    private readonly object _connLock = new object();

    /// <summary>เปิดฟังพอร์ตแชท คืน false ถ้า bind ไม่สำเร็จ (GP-15)</summary>
    public bool Start(int port)
    {
        if (!_listener.Start(port))
        {
            return false;
        }
        Port = port;
        _listener.ClientAccepted += ClientAccepted;
        Console.WriteLine($"[radiotower] listening on 0.0.0.0:{port}");
        return true;
    }

    private void ClientAccepted(Socket socket)
    {
        Durango.Offline.Connection connection = new Durango.Offline.Connection(socket);
        lock (_connLock)
        {
            _connections.Add(connection);
        }
        connection.Recv<Tune>(delegate(Tune tune, PacketHeader header)
        {
            connection.Send(new Conversations { _Conversations = null }, header.Seq);
        });
        connection.Recv<SayInExclusiveChannel>(delegate(SayInExclusiveChannel msg, PacketHeader header)
        {
            Console.WriteLine("[chat] {0}", msg.Message.ToString());
            Broadcast(msg);
        });
        connection.Recv<SayInConversation>(delegate(SayInConversation msg, PacketHeader header)
        {
            Console.WriteLine("[chat] {0}", msg.Message.ToString());
            Broadcast(msg);
        });
        connection.StartReceive();
        Console.WriteLine($"[radiotower] client connected from {socket.RemoteEndPoint}");
    }

    private void Broadcast<T>(T msg) where T : struct
    {
        lock (_connLock)
        {
            for (int i = _connections.Count - 1; i >= 0; i--)
            {
                try
                {
                    _connections[i].Send(msg);
                }
                catch (Exception)
                {
                }
            }
        }
    }

    public void Process()
    {
        _listener.Process();
        lock (_connLock)
        {
            for (int i = _connections.Count - 1; i >= 0; i--)
            {
                Durango.Offline.Connection conn = _connections[i];
                conn.Process();
                if (!conn.Connected())
                {
                    _connections.RemoveAt(i);
                }
            }
        }
    }
}
