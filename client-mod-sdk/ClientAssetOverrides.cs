using UnityEngine;

namespace Durango.Modding
{
    /// <summary>Controls whether a spawned prefab uses world or local-player coordinates.</summary>
    public enum ClientAssetAnchor
    {
        World = 0,
        LocalPlayer = 1
    }

    /// <summary>
    /// Optional client rendering API. Cast IClientModApi to this interface.
    /// All calls must be made from the Unity main thread. Bundle paths are relative
    /// to the mod DLL directory and cannot escape that directory.
    /// </summary>
    public interface IClientAssetOverrideApi
    {
        bool LoadAssetBundle(string bundleId, string relativePath, string sha256);
        bool IsAssetBundleLoaded(string bundleId);
        string[] GetLoadedAssetBundles();
        bool UnloadAssetBundle(string bundleId, bool unloadLoadedAssets);

        bool SpawnPrefab(string instanceId, string bundleId, string prefabAssetName,
            ClientAssetAnchor anchor, Vector3 position, Vector3 eulerAngles, Vector3 scale);
        bool PlayAudioClip(string instanceId, string bundleId, string audioAssetName,
            ClientAssetAnchor anchor, Vector3 position, float volume, bool loop);
        bool DestroySpawnedAsset(string instanceId);

        bool ReplaceLocalPlayerModel(string overrideId, string bundleId,
            string prefabAssetName, bool remapSkeleton);
        bool ReplaceLocalPlayerMaterial(string overrideId, string rendererSelector,
            int materialIndex, string bundleId, string materialAssetName);
        bool ReplaceLocalPlayerTexture(string overrideId, string rendererSelector,
            int materialIndex, string textureProperty, string bundleId, string textureAssetName);
        bool RestoreLocalPlayerAppearance(string overrideId);
        int RestoreAllLocalPlayerAppearance();
    }
}
