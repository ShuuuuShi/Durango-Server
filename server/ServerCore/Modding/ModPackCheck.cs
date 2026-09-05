namespace DurangoServer.Core;

/// <summary>Non-invasive M3 acceptance check for CI/deployment scripts.</summary>
public static class ModPackCheck
{
    public static int Run(string modsDirectory)
    {
        if (!Directory.Exists(modsDirectory)) { Console.WriteLine($"[mod-pack-check] {modsDirectory}: no directory (ok)"); return 0; }
        int failures = 0;
        foreach (string directory in Directory.GetDirectories(modsDirectory).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            string manifestPath = Path.Combine(directory, "mod.json");
            if (!File.Exists(manifestPath)) continue;
            if (!ModManifest.TryRead(manifestPath, out ModManifest manifest, out string error))
            { Console.WriteLine($"[mod-pack-check] FAIL {manifestPath}: {error}"); failures++; continue; }
            ModContentPack pack = ModContentPack.Load(directory, manifest.Id);
            if (pack.Errors.Count != 0) { Console.WriteLine($"[mod-pack-check] FAIL {manifest.Id}: {string.Join("; ", pack.Errors)}"); failures++; continue; }
            if (!string.IsNullOrWhiteSpace(manifest.ContentSha256) && !string.Equals(manifest.ContentSha256, pack.ContentHash, StringComparison.OrdinalIgnoreCase))
            { Console.WriteLine($"[mod-pack-check] FAIL {manifest.Id}: content hash mismatch"); failures++; continue; }
            Console.WriteLine($"[mod-pack-check] OK {manifest.Id} v{manifest.Version} entries={pack.Entries.Count} content_sha256={pack.ContentHash}");
        }
        return failures == 0 ? 0 : 1;
    }
}
