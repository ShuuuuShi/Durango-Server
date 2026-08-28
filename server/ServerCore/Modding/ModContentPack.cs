using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DurangoServer.Core;

/// <summary>M3: immutable, validated content supplied by one mod package.</summary>
internal sealed class ModContentPack
{
    private const int MaxFiles = 256;
    private const int MaxFileBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> Kinds = new(StringComparer.OrdinalIgnoreCase)
        { "item", "recipe", "loot", "buildable", "quest" };

    public string ModId { get; }
    public string ContentHash { get; }
    public IReadOnlyDictionary<string, JsonElement> Entries => _entries;
    public IReadOnlyList<string> Errors => _errors;
    private readonly Dictionary<string, JsonElement> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _errors = new();

    private ModContentPack(string modId, string hash) { ModId = modId; ContentHash = hash; }

    public static ModContentPack Load(string packageDirectory, string modId)
    {
        string contentDirectory = Path.Combine(packageDirectory, "content");
        List<string> files = Directory.Exists(contentDirectory)
            ? Directory.GetFiles(contentDirectory, "*.json", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList()
            : new List<string>();
        ModContentPack pack = new(modId, "");
        if (files.Count > MaxFiles) { pack._errors.Add($"content pack has more than {MaxFiles} files"); return pack; }
        using MemoryStream canonical = new();
        foreach (string file in files)
        {
            string relative = Path.GetRelativePath(contentDirectory, file).Replace('\\', '/');
            if (relative.Contains("../", StringComparison.Ordinal) || relative.StartsWith("../", StringComparison.Ordinal))
            { pack._errors.Add($"content path escapes package: {relative}"); continue; }
            byte[] bytes;
            try { bytes = File.ReadAllBytes(file); } catch (Exception e) { pack._errors.Add($"cannot read {relative}: {e.Message}"); continue; }
            if (bytes.Length > MaxFileBytes) { pack._errors.Add($"content file too large: {relative}"); continue; }
            try
            {
                using JsonDocument doc = JsonDocument.Parse(bytes);
                ValidateEntry(pack, doc.RootElement, relative);
                canonical.Write(Encoding.UTF8.GetBytes(relative)); canonical.WriteByte(0);
                using Utf8JsonWriter writer = new(canonical, new JsonWriterOptions { Indented = false });
                WriteCanonical(writer, doc.RootElement); writer.Flush(); canonical.WriteByte(0);
            }
            catch (Exception e) { pack._errors.Add($"invalid JSON in {relative}: {e.Message}"); }
        }
        ModContentPack result = new(modId, Convert.ToHexString(SHA256.HashData(canonical.ToArray())).ToLowerInvariant());
        foreach (KeyValuePair<string, JsonElement> item in pack._entries) result._entries[item.Key] = item.Value;
        result._errors.AddRange(pack._errors);
        return result;
    }

    private static void ValidateEntry(ModContentPack pack, JsonElement root, string file)
    {
        if (root.ValueKind != JsonValueKind.Object) { pack._errors.Add($"{file}: root must be an object"); return; }
        if (!root.TryGetProperty("kind", out JsonElement kind) || kind.ValueKind != JsonValueKind.String || !Kinds.Contains(kind.GetString() ?? ""))
        { pack._errors.Add($"{file}: kind must be item/recipe/loot/buildable/quest"); return; }
        if (!root.TryGetProperty("id", out JsonElement idElement) || idElement.ValueKind != JsonValueKind.String)
        { pack._errors.Add($"{file}: id is required"); return; }
        string id = idElement.GetString() ?? "";
        string local = id.StartsWith(pack.ModId + ":", StringComparison.OrdinalIgnoreCase) ? id[(pack.ModId.Length + 1)..] : "";
        if (string.IsNullOrWhiteSpace(local) || !RegexLikeId(local)) { pack._errors.Add($"{file}: id must be namespaced as {pack.ModId}:<name>"); return; }
        if (!pack._entries.TryAdd(id, root.Clone())) pack._errors.Add($"duplicate content id: {id}");
    }

    private static bool RegexLikeId(string id) => id.Length <= 120 && id.All(c => char.IsLetterOrDigit(c) || c is '_' or '-' or '.');

    private static void WriteCanonical(Utf8JsonWriter writer, JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (JsonProperty p in value.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal)) { writer.WritePropertyName(p.Name); WriteCanonical(writer, p.Value); }
                writer.WriteEndObject(); break;
            case JsonValueKind.Array:
                writer.WriteStartArray(); foreach (JsonElement child in value.EnumerateArray()) WriteCanonical(writer, child); writer.WriteEndArray(); break;
            default: value.WriteTo(writer); break;
        }
    }
}
