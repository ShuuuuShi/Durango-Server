using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
        if (name.Length <= 120)
        {
            return name;
        }
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        string suffix = Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
        return name.Substring(0, 120 - suffix.Length - 1) + "_" + suffix;
    }

    public static T Load<T>(string path) where T : class
    {
        if (!File.Exists(path) && !RecoverTempFile(path))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            T save = JsonConvert.DeserializeObject<T>(json, Settings);
            if (save == null)
            {
                throw new InvalidDataException("ไฟล์เซฟว่างหรือไม่ใช่ JSON object");
            }
            if (save is SaveEnvelope envelope)
            {
                Migrate(envelope, path);
            }
            return save;
        }
        catch (Exception e)
        {
            Quarantine(path, e.Message);
            return null;
        }
    }

    /// <summary>อ่านสำหรับ display/lookup เท่านั้น; ไม่ recover, migrate หรือ quarantine ไฟล์</summary>
    public static T Peek<T>(string path) where T : class
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path), Settings);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[save] lookup อ่าน {path} ไม่ได้: {e.Message}");
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
            if (data is SaveEnvelope envelope)
            {
                envelope.Version = SaveEnvelope.CurrentVersion;
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

    private static void Migrate(SaveEnvelope save, string path)
    {
        if (save.Version > SaveEnvelope.CurrentVersion)
        {
            throw new InvalidDataException($"schema v{save.Version} ใหม่กว่า server นี้ (รองรับถึง v{SaveEnvelope.CurrentVersion})");
        }
        if (save.Version < 0)
        {
            throw new InvalidDataException($"schema version {save.Version} ไม่ถูกต้อง");
        }
        if (save.Version < SaveEnvelope.CurrentVersion)
        {
            Console.WriteLine($"[save] migrate {path}: v{save.Version} → v{SaveEnvelope.CurrentVersion}");
            save.Version = SaveEnvelope.CurrentVersion;
        }
    }

    private static bool RecoverTempFile(string path)
    {
        string tmp = path + ".tmp";
        if (!File.Exists(tmp))
        {
            return false;
        }
        try
        {
            string json = File.ReadAllText(tmp);
            if (JsonConvert.DeserializeObject(json, Settings) == null)
            {
                throw new InvalidDataException("ไฟล์ชั่วคราวว่างหรือไม่ใช่ JSON");
            }
            File.Move(tmp, path, overwrite: false);
            Console.WriteLine($"[save] กู้ไฟล์ชั่วคราว {tmp} เป็น {path}");
            return true;
        }
        catch (Exception e)
        {
            Quarantine(tmp, "กู้ไฟล์ชั่วคราวไม่สำเร็จ: " + e.Message);
            return false;
        }
    }

    private static void Quarantine(string path, string reason)
    {
        try
        {
            if (File.Exists(path))
            {
                string quarantined = path + ".rejected-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                File.Move(path, quarantined, overwrite: false);
                Console.WriteLine($"[save] กักกัน {path} → {quarantined}: {reason}");
                return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[save] กักกัน {path} ไม่สำเร็จ: {e.Message}");
        }
        Console.WriteLine($"[save] อ่าน {path} ไม่ได้: {reason}");
    }
}
