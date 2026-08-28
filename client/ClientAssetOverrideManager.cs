using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Durango.Modding;
using UnityEngine;

/// <summary>
/// Owns mod AssetBundles and reversible local-player rendering overrides.
/// The gameplay object, collider and PlayerBehavior are never replaced; a custom
/// visual prefab is attached to them so networking and movement remain vanilla.
/// </summary>
internal sealed class ClientAssetOverrideManager
{
    private enum AppearanceKind
    {
        Model,
        Material,
        Texture
    }

    private sealed class BundleEntry
    {
        public string ModId;
        public string Id;
        public string Path;
        public AssetBundle Bundle;
    }

    private sealed class SpawnEntry
    {
        public string ModId;
        public string Id;
        public string BundleId;
        public GameObject Instance;
    }

    private sealed class RendererSnapshot
    {
        public Renderer Renderer;
        public bool Enabled;
        public Material[] Materials;
    }

    private sealed class AppearanceEntry
    {
        public string ModId;
        public string Id;
        public string BundleId;
        public string AssetName;
        public AppearanceKind Kind;
        public string RendererSelector;
        public int MaterialIndex;
        public string TextureProperty;
        public bool RemapSkeleton;
        public PlayerBehavior Player;
        public GameObject ModelInstance;
        public readonly List<RendererSnapshot> Renderers = new List<RendererSnapshot>();
        public readonly List<UnityEngine.Object> OwnedObjects = new List<UnityEngine.Object>();
    }

    private readonly int _mainThreadId = Thread.CurrentThread.ManagedThreadId;
    private readonly Dictionary<string, BundleEntry> _bundles = new Dictionary<string, BundleEntry>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, SpawnEntry> _spawns = new Dictionary<string, SpawnEntry>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AppearanceEntry> _appearanceByKey = new Dictionary<string, AppearanceEntry>(StringComparer.OrdinalIgnoreCase);
    private readonly List<AppearanceEntry> _appearanceOrder = new List<AppearanceEntry>();
    private PlayerBehavior _lastPlayer;
    private bool _appearanceDirty;

    public bool LoadBundle(string modId, string modRoot, string bundleId, string relativePath,
        string sha256, out string error)
    {
        error = "";
        if (!CheckMainThread(out error) || !ValidId(bundleId))
        {
            if (error.Length == 0) error = "invalid bundle id";
            return false;
        }
        string path;
        if (!ResolveModFile(modRoot, relativePath, out path, out error)) return false;
        if (!String.IsNullOrEmpty(sha256) && !VerifySha256(path, sha256))
        {
            error = "SHA-256 mismatch";
            return false;
        }

        string key = Key(modId, bundleId);
        BundleEntry old;
        if (_bundles.TryGetValue(key, out old))
        {
            if (String.Equals(old.Path, path, StringComparison.OrdinalIgnoreCase)) return true;
            error = "bundle id is already loaded from another file";
            return false;
        }

        try
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null)
            {
                error = "Unity could not load the AssetBundle (platform/version mismatch or invalid bundle)";
                return false;
            }
            _bundles.Add(key, new BundleEntry { ModId = modId, Id = bundleId, Path = path, Bundle = bundle });
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            return false;
        }
    }

    public bool IsBundleLoaded(string modId, string bundleId)
    {
        BundleEntry entry;
        return _bundles.TryGetValue(Key(modId, bundleId), out entry) && entry.Bundle != null;
    }

    public string[] GetLoadedBundles(string modId)
    {
        List<string> result = new List<string>();
        foreach (BundleEntry entry in _bundles.Values)
            if (String.Equals(entry.ModId, modId, StringComparison.OrdinalIgnoreCase)) result.Add(entry.Id);
        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result.ToArray();
    }

    public bool UnloadBundle(string modId, string bundleId, bool unloadLoadedAssets, out string error)
    {
        error = "";
        if (!CheckMainThread(out error)) return false;
        string key = Key(modId, bundleId);
        BundleEntry entry;
        if (!_bundles.TryGetValue(key, out entry)) return false;
        if (BundleInUse(modId, bundleId))
        {
            error = "bundle is still used by an appearance override or spawned asset";
            return false;
        }
        try
        {
            if (entry.Bundle != null) entry.Bundle.Unload(unloadLoadedAssets);
            _bundles.Remove(key);
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            return false;
        }
    }

    public bool SpawnPrefab(string modId, string instanceId, string bundleId, string assetName,
        ClientAssetAnchor anchor, Vector3 position, Vector3 eulerAngles, Vector3 scale, out string error)
    {
        error = "";
        if (!CheckMainThread(out error) || !ValidId(instanceId))
        {
            if (error.Length == 0) error = "invalid instance id";
            return false;
        }
        GameObject prefab;
        if (!LoadAsset(modId, bundleId, assetName, typeof(GameObject), out prefab, out error)) return false;
        PlayerBehavior player = PlayerBehavior.LocalPlayer;
        if (anchor == ClientAssetAnchor.LocalPlayer && player == null)
        {
            error = "local player is not ready";
            return false;
        }

        DestroySpawn(modId, instanceId);
        try
        {
            GameObject instance = (GameObject)UnityEngine.Object.Instantiate(prefab);
            instance.name = "__durango_mod_asset_" + SafeName(modId) + "_" + SafeName(instanceId);
            if (anchor == ClientAssetAnchor.LocalPlayer)
            {
                instance.transform.SetParent(player.transform, false);
                instance.transform.localPosition = position;
                instance.transform.localRotation = Quaternion.Euler(eulerAngles);
            }
            else
            {
                instance.transform.SetParent(null, false);
                instance.transform.position = position;
                instance.transform.rotation = Quaternion.Euler(eulerAngles);
            }
            instance.transform.localScale = scale;
            _spawns.Add(Key(modId, instanceId), new SpawnEntry
            {
                ModId = modId,
                Id = instanceId,
                BundleId = bundleId,
                Instance = instance
            });
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            return false;
        }
    }

    public bool DestroySpawn(string modId, string instanceId)
    {
        string key = Key(modId, instanceId);
        SpawnEntry entry;
        if (!_spawns.TryGetValue(key, out entry)) return false;
        if (entry.Instance != null) UnityEngine.Object.Destroy(entry.Instance);
        _spawns.Remove(key);
        return true;
    }

    public bool PlayAudio(string modId, string instanceId, string bundleId, string assetName,
        ClientAssetAnchor anchor, Vector3 position, float volume, bool loop, out string error)
    {
        error = "";
        if (!CheckMainThread(out error) || !ValidId(instanceId))
        {
            if (error.Length == 0) error = "invalid instance id";
            return false;
        }
        AudioClip clip;
        if (!LoadAsset(modId, bundleId, assetName, typeof(AudioClip), out clip, out error)) return false;
        PlayerBehavior player = PlayerBehavior.LocalPlayer;
        if (anchor == ClientAssetAnchor.LocalPlayer && player == null)
        {
            error = "local player is not ready";
            return false;
        }
        DestroySpawn(modId, instanceId);
        try
        {
            GameObject instance = new GameObject("__durango_mod_audio_" + SafeName(modId) + "_" + SafeName(instanceId));
            if (anchor == ClientAssetAnchor.LocalPlayer)
            {
                instance.transform.SetParent(player.transform, false);
                instance.transform.localPosition = position;
            }
            else instance.transform.position = position;
            AudioSource source = instance.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.loop = loop;
            source.Play();
            _spawns.Add(Key(modId, instanceId), new SpawnEntry
            {
                ModId = modId,
                Id = instanceId,
                BundleId = bundleId,
                Instance = instance
            });
            if (!loop) UnityEngine.Object.Destroy(instance, clip.length + 0.1f);
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            return false;
        }
    }

    public bool ReplaceModel(string modId, string overrideId, string bundleId, string assetName,
        bool remapSkeleton, out string error)
    {
        error = "";
        if (!CheckMainThread(out error) || !ValidId(overrideId))
        {
            if (error.Length == 0) error = "invalid override id";
            return false;
        }
        GameObject ignored;
        if (!LoadAsset(modId, bundleId, assetName, typeof(GameObject), out ignored, out error)) return false;
        for (int i = 0; i < _appearanceOrder.Count; i++)
        {
            AppearanceEntry active = _appearanceOrder[i];
            if (active.Kind == AppearanceKind.Model && !String.Equals(Key(active.ModId, active.Id), Key(modId, overrideId), StringComparison.OrdinalIgnoreCase))
            {
                error = "another full local-player model override is already active";
                return false;
            }
        }

        RemoveAppearance(modId, overrideId);
        AppearanceEntry entry = new AppearanceEntry
        {
            ModId = modId,
            Id = overrideId,
            BundleId = bundleId,
            AssetName = assetName,
            Kind = AppearanceKind.Model,
            RemapSkeleton = remapSkeleton
        };
        AddAppearance(entry);
        if (PlayerBehavior.LocalPlayer == null) return true;
        if (!ApplyAppearance(entry, out error))
        {
            RemoveAppearance(modId, overrideId);
            return false;
        }
        return true;
    }

    public bool ReplaceMaterial(string modId, string overrideId, string rendererSelector,
        int materialIndex, string bundleId, string assetName, out string error)
    {
        error = "";
        Material ignored;
        if (!CheckAppearanceArguments(modId, overrideId, rendererSelector, materialIndex,
            bundleId, assetName, typeof(Material), out ignored, out error)) return false;
        AppearanceEntry entry = new AppearanceEntry
        {
            ModId = modId,
            Id = overrideId,
            BundleId = bundleId,
            AssetName = assetName,
            Kind = AppearanceKind.Material,
            RendererSelector = rendererSelector,
            MaterialIndex = materialIndex
        };
        return RegisterAppearance(entry, out error);
    }

    public bool ReplaceTexture(string modId, string overrideId, string rendererSelector,
        int materialIndex, string textureProperty, string bundleId, string assetName, out string error)
    {
        error = "";
        Texture ignored;
        if (!CheckAppearanceArguments(modId, overrideId, rendererSelector, materialIndex,
            bundleId, assetName, typeof(Texture), out ignored, out error)) return false;
        AppearanceEntry entry = new AppearanceEntry
        {
            ModId = modId,
            Id = overrideId,
            BundleId = bundleId,
            AssetName = assetName,
            Kind = AppearanceKind.Texture,
            RendererSelector = rendererSelector,
            MaterialIndex = materialIndex,
            TextureProperty = String.IsNullOrEmpty(textureProperty) ? "_MainTex" : textureProperty
        };
        return RegisterAppearance(entry, out error);
    }

    public bool RemoveAppearance(string modId, string overrideId)
    {
        string key = Key(modId, overrideId);
        AppearanceEntry entry;
        if (!_appearanceByKey.TryGetValue(key, out entry)) return false;
        ClearApplied(entry, true);
        _appearanceByKey.Remove(key);
        _appearanceOrder.Remove(entry);
        return true;
    }

    public int RemoveAllAppearance(string modId)
    {
        int removed = 0;
        for (int i = _appearanceOrder.Count - 1; i >= 0; i--)
        {
            AppearanceEntry entry = _appearanceOrder[i];
            if (!String.Equals(entry.ModId, modId, StringComparison.OrdinalIgnoreCase)) continue;
            ClearApplied(entry, true);
            _appearanceByKey.Remove(Key(entry.ModId, entry.Id));
            _appearanceOrder.RemoveAt(i);
            removed++;
        }
        return removed;
    }

    public void MarkLocalPlayerAppearanceDirty()
    {
        _appearanceDirty = true;
    }

    public void PumpFrame()
    {
        if (_spawns.Count != 0)
        {
            List<string> destroyed = null;
            foreach (KeyValuePair<string, SpawnEntry> pair in _spawns)
            {
                if (pair.Value.Instance != null) continue;
                if (destroyed == null) destroyed = new List<string>();
                destroyed.Add(pair.Key);
            }
            if (destroyed != null)
                for (int i = 0; i < destroyed.Count; i++) _spawns.Remove(destroyed[i]);
        }
        PlayerBehavior player = PlayerBehavior.LocalPlayer;
        if (!_appearanceDirty && player == _lastPlayer) return;
        _appearanceDirty = false;
        _lastPlayer = player;
        for (int i = _appearanceOrder.Count - 1; i >= 0; i--)
        {
            AppearanceEntry entry = _appearanceOrder[i];
            // A model override owns only Renderer.enabled and can safely restore it.
            // Costume refreshes replace materials themselves, so stale material
            // snapshots must be discarded instead of written over the new costume.
            ClearApplied(entry, entry.Kind == AppearanceKind.Model);
        }
        if (player == null) return;
        for (int i = 0; i < _appearanceOrder.Count; i++)
        {
            string error;
            if (!ApplyAppearance(_appearanceOrder[i], out error))
                ClientModLoader.Trace("[clientmods] could not reapply appearance '" + _appearanceOrder[i].Id + "': " + error);
        }
    }

    public void RemoveForMod(string modId)
    {
        RemoveAllAppearance(modId);
        List<string> spawnIds = new List<string>();
        foreach (SpawnEntry entry in _spawns.Values)
            if (String.Equals(entry.ModId, modId, StringComparison.OrdinalIgnoreCase)) spawnIds.Add(entry.Id);
        for (int i = 0; i < spawnIds.Count; i++) DestroySpawn(modId, spawnIds[i]);

        List<string> bundleIds = new List<string>();
        foreach (BundleEntry entry in _bundles.Values)
            if (String.Equals(entry.ModId, modId, StringComparison.OrdinalIgnoreCase)) bundleIds.Add(entry.Id);
        for (int i = 0; i < bundleIds.Count; i++)
        {
            string error;
            UnloadBundle(modId, bundleIds[i], false, out error);
        }
    }

    public void RemoveAll()
    {
        List<string> mods = new List<string>();
        foreach (BundleEntry entry in _bundles.Values)
            if (!mods.Contains(entry.ModId)) mods.Add(entry.ModId);
        foreach (AppearanceEntry entry in _appearanceOrder)
            if (!mods.Contains(entry.ModId)) mods.Add(entry.ModId);
        foreach (SpawnEntry entry in _spawns.Values)
            if (!mods.Contains(entry.ModId)) mods.Add(entry.ModId);
        for (int i = 0; i < mods.Count; i++) RemoveForMod(mods[i]);
    }

    private bool RegisterAppearance(AppearanceEntry entry, out string error)
    {
        RemoveAppearance(entry.ModId, entry.Id);
        AddAppearance(entry);
        if (PlayerBehavior.LocalPlayer == null)
        {
            error = "";
            return true;
        }
        if (!ApplyAppearance(entry, out error))
        {
            RemoveAppearance(entry.ModId, entry.Id);
            return false;
        }
        return true;
    }

    private void AddAppearance(AppearanceEntry entry)
    {
        _appearanceByKey.Add(Key(entry.ModId, entry.Id), entry);
        _appearanceOrder.Add(entry);
    }

    private bool ApplyAppearance(AppearanceEntry entry, out string error)
    {
        error = "";
        PlayerBehavior player = PlayerBehavior.LocalPlayer;
        if (player == null)
        {
            error = "local player is not ready";
            return false;
        }
        entry.Player = player;
        try
        {
            if (entry.Kind == AppearanceKind.Model) return ApplyModel(entry, player, out error);
            Renderer[] targets = FindRenderers(player, entry.RendererSelector);
            if (targets.Length == 0)
            {
                error = "renderer selector matched nothing: " + entry.RendererSelector;
                return false;
            }
            UnityEngine.Object asset;
            Type type = entry.Kind == AppearanceKind.Material ? typeof(Material) : typeof(Texture);
            if (!LoadAssetObject(entry.ModId, entry.BundleId, entry.AssetName, type, out asset, out error)) return false;
            int changed = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                Renderer renderer = targets[i];
                Material[] oldMaterials = renderer.sharedMaterials;
                if (oldMaterials == null || entry.MaterialIndex >= oldMaterials.Length) continue;
                Material[] replacement = (Material[])oldMaterials.Clone();
                RendererSnapshot snapshot = new RendererSnapshot { Renderer = renderer, Enabled = renderer.enabled, Materials = oldMaterials };
                if (entry.Kind == AppearanceKind.Material)
                {
                    replacement[entry.MaterialIndex] = (Material)asset;
                }
                else
                {
                    Material original = replacement[entry.MaterialIndex];
                    if (original == null) continue;
                    Material clone = new Material(original);
                    clone.name = original.name + "__durango_mod_" + SafeName(entry.ModId);
                    clone.SetTexture(entry.TextureProperty, (Texture)asset);
                    replacement[entry.MaterialIndex] = clone;
                    entry.OwnedObjects.Add(clone);
                }
                entry.Renderers.Add(snapshot);
                renderer.sharedMaterials = replacement;
                changed++;
            }
            if (changed == 0)
            {
                ClearApplied(entry, true);
                error = "material index did not exist on any matched renderer";
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            ClearApplied(entry, true);
            error = e.Message;
            return false;
        }
    }

    private bool ApplyModel(AppearanceEntry entry, PlayerBehavior player, out string error)
    {
        error = "";
        GameObject prefab;
        if (!LoadAsset(entry.ModId, entry.BundleId, entry.AssetName, typeof(GameObject), out prefab, out error)) return false;
        Renderer[] originals = FindRenderers(player, "*");
        Transform[] targetBones = player.transform.GetComponentsInChildren<Transform>(true);
        GameObject instance = (GameObject)UnityEngine.Object.Instantiate(prefab);
        instance.name = "__durango_mod_model_" + SafeName(entry.ModId) + "_" + SafeName(entry.Id);
        instance.transform.SetParent(player.transform, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        SetLayerRecursive(instance.transform, player.gameObject.layer);
        if (entry.RemapSkeleton) RemapSkeleton(instance, targetBones);
        for (int i = 0; i < originals.Length; i++)
        {
            entry.Renderers.Add(new RendererSnapshot { Renderer = originals[i], Enabled = originals[i].enabled });
            originals[i].enabled = false;
        }
        entry.ModelInstance = instance;
        return true;
    }

    private void ClearApplied(AppearanceEntry entry, bool restore)
    {
        for (int i = entry.Renderers.Count - 1; i >= 0; i--)
        {
            RendererSnapshot snapshot = entry.Renderers[i];
            if (!restore || snapshot.Renderer == null) continue;
            snapshot.Renderer.enabled = snapshot.Enabled;
            if (snapshot.Materials != null) snapshot.Renderer.sharedMaterials = snapshot.Materials;
        }
        entry.Renderers.Clear();
        if (entry.ModelInstance != null) UnityEngine.Object.Destroy(entry.ModelInstance);
        entry.ModelInstance = null;
        for (int i = 0; i < entry.OwnedObjects.Count; i++)
            if (entry.OwnedObjects[i] != null) UnityEngine.Object.Destroy(entry.OwnedObjects[i]);
        entry.OwnedObjects.Clear();
        entry.Player = null;
    }

    private bool CheckAppearanceArguments<T>(string modId, string overrideId, string rendererSelector,
        int materialIndex, string bundleId, string assetName, Type assetType, out T ignored, out string error)
        where T : UnityEngine.Object
    {
        ignored = null;
        error = "";
        if (!CheckMainThread(out error) || !ValidId(overrideId))
        {
            if (error.Length == 0) error = "invalid override id";
            return false;
        }
        if (String.IsNullOrEmpty(rendererSelector) || materialIndex < 0)
        {
            error = "renderer selector is required and material index must be non-negative";
            return false;
        }
        return LoadAsset(modId, bundleId, assetName, assetType, out ignored, out error);
    }

    private bool LoadAsset<T>(string modId, string bundleId, string assetName, Type type,
        out T asset, out string error) where T : UnityEngine.Object
    {
        asset = null;
        UnityEngine.Object raw;
        if (!LoadAssetObject(modId, bundleId, assetName, type, out raw, out error)) return false;
        asset = raw as T;
        if (asset != null) return true;
        error = "asset has the wrong Unity type";
        return false;
    }

    private bool LoadAssetObject(string modId, string bundleId, string assetName, Type type,
        out UnityEngine.Object asset, out string error)
    {
        asset = null;
        error = "";
        if (String.IsNullOrEmpty(assetName))
        {
            error = "asset name is required";
            return false;
        }
        BundleEntry entry;
        if (!_bundles.TryGetValue(Key(modId, bundleId), out entry) || entry.Bundle == null)
        {
            error = "bundle is not loaded: " + bundleId;
            return false;
        }
        try
        {
            asset = entry.Bundle.LoadAsset(assetName, type);
            if (asset != null) return true;
            error = "asset was not found in bundle: " + assetName;
            return false;
        }
        catch (Exception e)
        {
            error = e.Message;
            return false;
        }
    }

    private static Renderer[] FindRenderers(PlayerBehavior player, string selector)
    {
        Renderer[] all = player.GetComponentsInChildren<Renderer>(true);
        List<Renderer> result = new List<Renderer>();
        for (int i = 0; i < all.Length; i++)
        {
            Renderer renderer = all[i];
            if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
            if (IsModObject(renderer.transform, player.transform)) continue;
            if (selector == "*" || String.Equals(renderer.name, selector, StringComparison.OrdinalIgnoreCase) ||
                String.Equals(RelativePath(player.transform, renderer.transform), selector, StringComparison.OrdinalIgnoreCase))
                result.Add(renderer);
        }
        return result.ToArray();
    }

    private static bool IsModObject(Transform current, Transform player)
    {
        while (current != null && current != player)
        {
            if (current.name.StartsWith("__durango_mod_", StringComparison.Ordinal)) return true;
            current = current.parent;
        }
        return false;
    }

    private static string RelativePath(Transform root, Transform target)
    {
        List<string> names = new List<string>();
        Transform current = target;
        while (current != null && current != root)
        {
            names.Add(current.name);
            current = current.parent;
        }
        names.Reverse();
        return String.Join("/", names.ToArray());
    }

    private static void RemapSkeleton(GameObject instance, Transform[] targetBones)
    {
        Dictionary<string, Transform> byName = new Dictionary<string, Transform>(StringComparer.Ordinal);
        for (int i = 0; i < targetBones.Length; i++)
            if (!byName.ContainsKey(targetBones[i].name)) byName.Add(targetBones[i].name, targetBones[i]);
        SkinnedMeshRenderer[] renderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Transform[] sourceBones = renderers[i].bones;
            if (sourceBones == null || sourceBones.Length == 0) continue;
            Transform[] mapped = new Transform[sourceBones.Length];
            int matches = 0;
            for (int b = 0; b < sourceBones.Length; b++)
            {
                Transform found;
                if (sourceBones[b] != null && byName.TryGetValue(sourceBones[b].name, out found))
                {
                    mapped[b] = found;
                    matches++;
                }
                else mapped[b] = sourceBones[b];
            }
            if (matches == 0) continue;
            renderers[i].bones = mapped;
            Transform root;
            if (renderers[i].rootBone != null && byName.TryGetValue(renderers[i].rootBone.name, out root))
                renderers[i].rootBone = root;
        }
    }

    private static void SetLayerRecursive(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++) SetLayerRecursive(root.GetChild(i), layer);
    }

    private bool BundleInUse(string modId, string bundleId)
    {
        foreach (SpawnEntry entry in _spawns.Values)
            if (SameBundle(entry.ModId, entry.BundleId, modId, bundleId)) return true;
        for (int i = 0; i < _appearanceOrder.Count; i++)
            if (SameBundle(_appearanceOrder[i].ModId, _appearanceOrder[i].BundleId, modId, bundleId)) return true;
        return false;
    }

    private static bool SameBundle(string leftMod, string leftBundle, string rightMod, string rightBundle)
    {
        return String.Equals(leftMod, rightMod, StringComparison.OrdinalIgnoreCase) &&
            String.Equals(leftBundle, rightBundle, StringComparison.OrdinalIgnoreCase);
    }

    private bool CheckMainThread(out string error)
    {
        if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
        {
            error = "";
            return true;
        }
        error = "Unity rendering APIs must be called from the main thread";
        return false;
    }

    private static bool ResolveModFile(string modRoot, string relativePath, out string path, out string error)
    {
        path = "";
        error = "";
        if (String.IsNullOrEmpty(modRoot) || String.IsNullOrEmpty(relativePath) || Path.IsPathRooted(relativePath))
        {
            error = "bundle path must be relative to the mod directory";
            return false;
        }
        try
        {
            string root = Path.GetFullPath(modRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            path = Path.GetFullPath(Path.Combine(root, relativePath));
            if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
            {
                error = "bundle path is outside the mod directory or does not exist";
                path = "";
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            path = "";
            return false;
        }
    }

    private static bool VerifySha256(string path, string expected)
    {
        try
        {
            byte[] bytes = SHA256.Create().ComputeHash(File.ReadAllBytes(path));
            StringBuilder actual = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++) actual.Append(bytes[i].ToString("x2"));
            return String.Equals(actual.ToString(), expected.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool ValidId(string id)
    {
        if (String.IsNullOrEmpty(id) || id.Length > 64) return false;
        for (int i = 0; i < id.Length; i++)
        {
            char c = id[i];
            if (!(Char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.')) return false;
        }
        return true;
    }

    private static string Key(string modId, string id)
    {
        return (modId ?? "") + "\n" + (id ?? "");
    }

    private static string SafeName(string value)
    {
        if (String.IsNullOrEmpty(value)) return "mod";
        StringBuilder result = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
            result.Append(Char.IsLetterOrDigit(value[i]) || value[i] == '_' || value[i] == '-' ? value[i] : '_');
        return result.ToString();
    }
}
