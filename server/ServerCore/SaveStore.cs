using System;
using System.IO;
using Newtonsoft.Json;

namespace DurangoServer.Core;

/// <summary>
/// GP-07: อ่าน/เขียนไฟล์เซฟเป็น JSON
///
/// เขียนแบบ "เขียนลง .tmp ก่อนแล้วค่อยสลับ" เพื่อไม่ให้ไฟล์เซฟพังถ้าเซิร์ฟดับกลางคัน
/// (ถ้าเขียนทับตรง ๆ แล้วไฟดับ จะได้ไฟล์ JSON ที่ไม่ครบ = โหลดกลับไม่ได้เลย)
/// </summary>
public static class SaveStore
{
    private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    /// <summary>โฟลเดอร์รากของไฟล์เซฟ ตั้งจาก --saves ใน Program.cs</summary>
    public static string Root { get; set; } = "saves";

    /// <summary>
    /// โลกของเกาะนี้ — โหมดหลายเกาะเก็บแยกไฟล์ต่อเกาะ (`saves/worlds/<id>.json`)
    /// ส่วน `players/` กับ `accounts/` **ใช้ร่วมกันทุกเกาะ** เพราะตัวละครเดินทางข้ามเกาะได้
    /// </summary>
    public static string WorldPath => IslandRegistry.Current == null
        ? Path.Combine(Root, "world.json")
        : Path.Combine(Root, "worlds", SafeFileName(IslandRegistry.Current.Id) + ".json");

    public static string PlayerPath(string entityId)
    {
        return Path.Combine(Root, "players", SafeFileName(entityId) + ".json");
    }

    /// <summary>entity id เป็น GUID อยู่แล้ว แต่มาจาก client จึงต้องกันอักขระที่ใช้เป็นชื่อไฟล์ไม่ได้</summary>
    private static string SafeFileName(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return "_unknown";
        }
        char[] bad = Path.GetInvalidFileNameChars();
        char[] buf = raw.ToCharArray();
        for (int i = 0; i < buf.Length; i++)
        {
            if (Array.IndexOf(bad, buf[i]) != -1)
            {
                buf[i] = '_';
            }
        }
        string name = new string(buf);
        return name.Length > 120 ? name.Substring(0, 120) : name;
    }

    public static T Load<T>(string path) where T : class
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[save] อ่าน {path} ไม่ได้: {e.Message}");
            return null;
        }
    }

    public static bool Save<T>(string path, T data)
    {
        try
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonConvert.SerializeObject(data, Settings));
            // File.Move(overwrite) เป็น atomic บนโวลุ่มเดียวกัน — ไฟล์เดิมจะอยู่ครบจนกว่าตัวใหม่จะเขียนเสร็จ
            File.Move(tmp, path, overwrite: true);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[save] เขียน {path} ไม่ได้: {e.Message}");
            return false;
        }
    }
}
