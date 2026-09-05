using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DurangoServer.Core;

/// <summary>M5: deterministic server/client mod handshake policy and validation.</summary>
public sealed class ModNegotiationPolicy
{
    public bool RequireHello { get; set; }
    public bool AllowUnknownOptional { get; set; } = true;
    public bool RequireSignatures { get; set; }
    public int MaxManifestBytes { get; set; } = 64 * 1024;
    public string? TrustedPublicKey { get; set; }
    public IReadOnlyList<ClientModRequirement>? ClientAllowlist { get; set; }
}

public sealed class ClientModRequirement
{
    public string Id { get; set; } = "";
    public string Version { get; set; } = "";
    public string Sha256 { get; set; } = "";
}

public sealed class ModNegotiationResult
{
    public bool Accepted { get; init; }
    public string Reason { get; init; } = "";
    public string CatalogHash { get; init; } = "";
    public int MatchedRequired { get; init; }
}

internal sealed class ModNegotiationEntry
{
    public string Id { get; set; } = "";
    public string Version { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public bool Required { get; set; }
    public string Signature { get; set; } = "";
    public string PublicKey { get; set; } = "";
}

internal static class ModNegotiation
{
    private sealed class ClientEnvelope { public int Protocol { get; set; } = 1; public List<ModNegotiationEntry> Mods { get; set; } = new(); }

    /// <summary>
    /// client มี mod ตัวนี้โหลดอยู่ไหม (อ่านจาก manifest ที่มากับ ModHello)
    ///
    /// ใช้ตัดสินว่า client เครื่องนั้น **ถือ chunk ไว้กว้างแค่ไหน** — `DurangoClientCore`
    /// ขยาย `TerrainBase.ChunkPool` เป็น `world_chunk_range` ตอน runtime ถ้าไม่มีมอด
    /// client จะเป็น retail แท้ ๆ ที่ `_visibleRange = 1` (ดู HandleSetChunk)
    /// </summary>
    public static bool HasMod(string? json, string id)
    {
        if (string.IsNullOrWhiteSpace(json)) { return false; }
        try
        {
            ClientEnvelope? envelope = JsonSerializer.Deserialize<ClientEnvelope>(json);
            return envelope?.Mods?.Any(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase)) == true;
        }
        catch
        {
            return false;
        }
    }

    public static string BuildServerCatalog(IReadOnlyList<PluginManager.LoadedModInfo> mods)
    {
        List<ModNegotiationEntry> entries = mods.Where(x => x.Loaded && !string.IsNullOrWhiteSpace(x.Id)).Select(x => new ModNegotiationEntry
        { Id = x.Id, Version = x.Version, Sha256 = x.AssemblySha256, Required = x.Required }).OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase).ToList();
        return CanonicalHash(entries);
    }

    public static ModNegotiationResult Validate(string? json, string? clientCatalogHash, IReadOnlyList<PluginManager.LoadedModInfo> mods, ModNegotiationPolicy policy)
    {
        List<ClientModRequirement> expected = policy.ClientAllowlist?.ToList() ?? mods
            .Where(x => x.Loaded && !string.IsNullOrWhiteSpace(x.Id))
            .Select(x => new ClientModRequirement { Id = x.Id, Version = x.Version, Sha256 = x.AssemblySha256 })
            .ToList();
        string catalog = CanonicalClientHash(expected);
        if (string.IsNullOrWhiteSpace(json)) return new ModNegotiationResult { Accepted = !policy.RequireHello, Reason = policy.RequireHello ? "mod handshake is required" : "optional handshake missing", CatalogHash = catalog };
        if (Encoding.UTF8.GetByteCount(json) > policy.MaxManifestBytes) return Reject("mod manifest is too large", catalog);
        ClientEnvelope? envelope;
        try { envelope = JsonSerializer.Deserialize<ClientEnvelope>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); }
        catch (Exception e) { return Reject("invalid mod manifest: " + e.Message, catalog); }
        if (envelope == null || envelope.Protocol != 1) return Reject("unsupported mod handshake protocol", catalog);
        envelope.Mods ??= new();
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (ModNegotiationEntry client in envelope.Mods)
        {
            if (!ModManifest.IdRx.IsMatch(client.Id ?? "") || !seen.Add(client.Id)) return Reject("invalid or duplicate client mod id", catalog);
            if (!string.IsNullOrWhiteSpace(client.Sha256) && !ModManifest.IsSha256(client.Sha256)) return Reject("invalid client mod hash", catalog);
        }
        Dictionary<string, ModNegotiationEntry> clientMap = envelope.Mods.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        HashSet<string> expectedIds = expected.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!policy.AllowUnknownOptional)
        {
            ModNegotiationEntry? unknown = envelope.Mods.FirstOrDefault(x => !expectedIds.Contains(x.Id));
            if (unknown != null) return Reject($"unknown client mod: {unknown.Id}", catalog);
        }
        int matched = 0;
        foreach (ClientModRequirement server in expected)
        {
            if (!clientMap.TryGetValue(server.Id, out ModNegotiationEntry? client))
            {
                if (policy.RequireHello && !policy.AllowUnknownOptional) return Reject($"required mod missing: {server.Id}", catalog);
                continue;
            }
            if (!string.Equals(client.Version, server.Version, StringComparison.Ordinal) || (!string.IsNullOrWhiteSpace(server.Sha256) && !string.Equals(client.Sha256, server.Sha256, StringComparison.OrdinalIgnoreCase)))
                return Reject($"mod mismatch: {server.Id}", catalog);
            if (policy.RequireSignatures && !VerifySignature(client, policy)) return Reject($"invalid signature: {server.Id}", catalog);
            matched++;
        }
        // The client hash describes the client's installed set, while the server hash describes
        // the authoritative required/optional set. They are intentionally different views.
        // Compatibility is checked per required mod above; the client hash is retained for logs/metrics.
        return new ModNegotiationResult { Accepted = true, Reason = "ok", CatalogHash = catalog, MatchedRequired = matched };
    }

    public static IReadOnlyList<ClientModRequirement> LoadClientAllowlist(string path)
    {
        string json = File.ReadAllText(path);
        List<ClientModRequirement>? entries = JsonSerializer.Deserialize<List<ClientModRequirement>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (entries == null) throw new InvalidDataException("client mod allowlist is empty");
        HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);
        foreach (ClientModRequirement entry in entries)
        {
            if (!ModManifest.IdRx.IsMatch(entry.Id ?? "") || !ids.Add(entry.Id))
                throw new InvalidDataException("invalid or duplicate client mod id in allowlist");
            if (!ModManifest.IsSha256(entry.Sha256 ?? ""))
                throw new InvalidDataException($"invalid sha256 for client mod {entry.Id}");
        }
        return entries;
    }

    private static ModNegotiationResult Reject(string reason, string catalog) => new() { Accepted = false, Reason = reason, CatalogHash = catalog };

    private static string CanonicalHash(IEnumerable<ModNegotiationEntry> entries)
    {
        StringBuilder data = new();
        foreach (ModNegotiationEntry x in entries.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase))
            data.Append(x.Id).Append('\n').Append(x.Version).Append('\n').Append(x.Sha256).Append('\n').Append(x.Required ? '1' : '0').Append('\n');
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(data.ToString()))).ToLowerInvariant();
    }

    private static string CanonicalClientHash(IEnumerable<ClientModRequirement> entries)
    {
        StringBuilder data = new();
        foreach (ClientModRequirement x in entries.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase))
            data.Append(x.Id).Append('\n').Append(x.Version).Append('\n').Append(x.Sha256).Append('\n');
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(data.ToString()))).ToLowerInvariant();
    }

    private static bool VerifySignature(ModNegotiationEntry entry, ModNegotiationPolicy policy)
    {
        if (string.IsNullOrWhiteSpace(entry.Signature) || string.IsNullOrWhiteSpace(entry.PublicKey) || string.IsNullOrWhiteSpace(policy.TrustedPublicKey)) return false;
        if (!string.Equals(entry.PublicKey.Trim(), policy.TrustedPublicKey.Trim(), StringComparison.Ordinal)) return false;
        try
        {
            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(policy.TrustedPublicKey);
            byte[] payload = Encoding.UTF8.GetBytes(entry.Id + "\n" + entry.Version + "\n" + entry.Sha256);
            return rsa.VerifyData(payload, Convert.FromBase64String(entry.Signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch { return false; }
    }
}
