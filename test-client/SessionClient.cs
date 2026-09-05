using System;
using System.Net.Http;
using System.Text;

namespace DurangoTestClient;

/// <summary>
/// GP-12: ขอ session token จาก gateway (HTTP) เหมือนที่ตัวเกมจริงทำ ก่อนจะ Auth เข้าพอร์ตเกม
///
/// server ไม่รับ Auth ที่ไม่มี token แล้ว (เว้นแต่รันด้วย --insecure-auth)
/// ตัวเกมจริงยิง POST /sessions พร้อมฟิลด์ "player" = JSON ของเกาะตัวเอง
/// ตัวทดสอบส่งแค่ player_info เท่าที่ server ต้องใช้ (id/ชื่อ/เลเวล)
/// </summary>
public static class SessionClient
{
    /// <summary>พอร์ต gateway เริ่มต้น (พอร์ตเกม 8191 - 1)</summary>
    public const int DefaultGatewayPort = 8190;

    /// <summary>user_id ที่ gateway ออกให้ในคำขอ Fetch ล่าสุด — ใช้เป็น EntityId ตอน Auth</summary>
    public static string LastUserId;

    /// <summary>
    /// คืน session token ถ้าขอสำเร็จ, คืน null ถ้าติดต่อ gateway ไม่ได้
    /// (เรียกต่อได้ — server ที่รัน --insecure-auth ยังรับ Auth ที่ไม่มี token)
    /// </summary>
    public static string Fetch(string host, int gatewayPort, string entityId, string name, int level = 1)
    {
        string playerJson =
            "{\"player_info\":{\"player_entity_id\":\"" + Escape(entityId) + "\"," +
            "\"player_name\":\"" + Escape(name) + "\"," +
            "\"player_level\":" + level + "}}";
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var body = new StringContent(
                "player=" + Uri.EscapeDataString(playerJson),
                Encoding.UTF8,
                "application/x-www-form-urlencoded");
            string url = $"http://{host}:{gatewayPort}/sessions";
            string reply = http.PostAsync(url, body).GetAwaiter().GetResult()
                .Content.ReadAsStringAsync().GetAwaiter().GetResult();
            string token = ReadJsonString(reply, "session_token");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("[session] gateway ไม่ได้ส่ง session_token กลับมา: " + reply);
                return null;
            }
            // [4 ก.ย. 2026] gateway ผูก token กับ user_id ที่มันออกให้ (id ที่ไม่มีเซฟ = ตัวละคร local ⇒ ได้ id ใหม่)
            // ตัวเทสต้อง Auth ด้วย id นี้ ไม่ใช่ id ที่ตัวเองตั้งมา ไม่งั้นโดน "token เป็นของคนอื่น"
            LastUserId = ReadJsonString(reply, "user_id");
            Console.WriteLine($"[session] ได้ token จาก {url} (…{token.Substring(Math.Max(0, token.Length - 6))}) user_id={LastUserId ?? "(ไม่มี)"}");
            return token;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[session] ขอ token จาก gateway {host}:{gatewayPort} ไม่ได้: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// ขอ token โดยส่ง player JSON ดิบ ๆ ที่เตรียมเองมา — ใช้เลียนแบบ client ที่เพิ่งสร้างตัวละคร
    /// ซึ่งส่งมาแค่ entity id (ชื่อ/หน้าตาต้องมาจากไฟล์เซฟฝั่ง server)
    /// </summary>
    public static string FetchRaw(string host, int gatewayPort, string playerJson)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var body = new StringContent(
                "player=" + Uri.EscapeDataString(playerJson),
                Encoding.UTF8,
                "application/x-www-form-urlencoded");
            string reply = http.PostAsync($"http://{host}:{gatewayPort}/sessions", body)
                .GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return ReadJsonString(reply, "session_token");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[session] ขอ token (raw) ไม่ได้: {e.Message}");
            return null;
        }
    }

    /// <summary>อ่านค่า string ของคีย์หนึ่งจาก JSON แบน ๆ (เลี่ยงการลาก Newtonsoft เข้ามาในตัวทดสอบ)</summary>
    private static string ReadJsonString(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }
        int at = json.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
        if (at < 0)
        {
            return null;
        }
        int colon = json.IndexOf(':', at);
        if (colon < 0)
        {
            return null;
        }
        int open = json.IndexOf('"', colon);
        if (open < 0)
        {
            return null;
        }
        int close = json.IndexOf('"', open + 1);
        return close > open ? json.Substring(open + 1, close - open - 1) : null;
    }

    private static string Escape(string s)
    {
        return (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
