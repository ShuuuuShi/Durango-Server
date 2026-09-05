using System.Text;
using DurangoServer.Modding;

namespace DurangoServer.Core;

/// <summary>Namespaced JSON storage for one trusted server mod. Writes are atomic and failures are isolated.</summary>
internal sealed class FileModStorage : IModStorage
{
    private readonly string _root;

    public FileModStorage(string root)
    {
        _root = root;
        Directory.CreateDirectory(_root);
    }

    private string PathFor(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Contains("..", StringComparison.Ordinal) ||
            key.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("storage key must be a simple file name", nameof(key));
        }
        return System.IO.Path.Combine(_root, key.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? key : key + ".json");
    }

    public bool Exists(string key)
    {
        try { return File.Exists(PathFor(key)); }
        catch { return false; }
    }

    public string? LoadJson(string key)
    {
        try
        {
            string path = PathFor(key);
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[mods] storage read failed ({key}): {e.Message}");
            return null;
        }
    }

    public bool SaveJson(string key, string json)
    {
        if (json == null) return false;
        try
        {
            string path = PathFor(key);
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, json, Encoding.UTF8);
            File.Move(tmp, path, true);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[mods] storage write failed ({key}): {e.Message}");
            return false;
        }
    }

    public bool Delete(string key)
    {
        try
        {
            string path = PathFor(key);
            if (File.Exists(path)) File.Delete(path);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[mods] storage delete failed ({key}): {e.Message}");
            return false;
        }
    }

    public bool Flush() => true;
}
