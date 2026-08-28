using System;
using Durango.Modding;
using UnityEngine;

namespace ExampleRenderMod
{
    /// <summary>
    /// Buildable example for client method hooks and AssetBundle rendering.
    /// Rendering is opt-in so the sample can be installed without shipping a bundle.
    /// </summary>
    public sealed class ExampleRenderPlugin : IClientPlugin, IClientModIdentity, IClientModLifecycle
    {
        private IClientModApi _api;
        private IClientAssetOverrideApi _assets;
        private bool _modelEnabled;
        private int _positionCalls;

        public string Name { get { return "ExampleRenderMod"; } }
        public string Id { get { return "example-render-mod"; } }
        public string ApiVersion { get { return "1"; } }
        public string Version { get { return "0.2.0"; } }
        public string Signature { get { return ""; } }
        public string PublicKey { get { return ""; } }

        public void OnPreLoad(IClientModApi api)
        {
            _api = api;
            _assets = api as IClientAssetOverrideApi;
        }

        public void OnLoad(IClientModApi api)
        {
            IClientMethodOverridesApi methods = api as IClientMethodOverridesApi;
            if (methods != null && Environment.GetEnvironmentVariable("DURANGO_CLIENT_OVERRIDE_PROBE") == "1")
            {
                methods.RegisterMethodOverride("PlayerBehavior::GetCurrentPosition()",
                    ClientMethodOverrideKind.Postfix, delegate(ClientMethodOverrideContext context)
                    {
                        _positionCalls++;
                        if (_positionCalls == 1) _api.Log("client method override probe hit: " + context.MethodId);
                    }, 0);
            }

            if (_assets == null)
            {
                api.Log("This client does not expose IClientAssetOverrideApi.");
                return;
            }

            api.RegisterHotkey(KeyCode.F6, ToggleModel);
            api.RegisterHotkey(KeyCode.F7, SpawnEffect);
            api.RegisterHotkey(KeyCode.F8, RestoreEverything);
            api.OnGameReady(delegate
            {
                if (Environment.GetEnvironmentVariable("DURANGO_RENDER_SAMPLE") != "1")
                {
                    api.Log("Render sample is installed but inactive. Set DURANGO_RENDER_SAMPLE=1 after adding its AssetBundle.");
                    return;
                }
                if (LoadSampleBundle()) ToggleModel();
            });
        }

        public void OnPostLoad(IClientModApi api)
        {
            api.Log("F6=model toggle, F7=spawn attached effect, F8=restore render assets");
        }

        public void OnDisable(IClientModApi api)
        {
            if (_assets == null) return;
            _assets.RestoreAllLocalPlayerAppearance();
            _assets.DestroySpawnedAsset("sample-effect");
        }

        private bool LoadSampleBundle()
        {
            if (_assets.IsAssetBundleLoaded("sample")) return true;
            string relativePath = ValueOr("DURANGO_RENDER_BUNDLE", "assets/render-sample.bundle");
            string sha256 = Environment.GetEnvironmentVariable("DURANGO_RENDER_BUNDLE_SHA256") ?? "";
            bool loaded = _assets.LoadAssetBundle("sample", relativePath, sha256);
            _api.Log(loaded ? "render AssetBundle loaded" : "render AssetBundle failed to load: " + relativePath);
            return loaded;
        }

        private void ToggleModel()
        {
            if (_assets == null || (!LoadSampleBundle())) return;
            if (_modelEnabled)
            {
                _assets.RestoreLocalPlayerAppearance("sample-player-model");
                _modelEnabled = false;
                _api.ShowMessage("Restored original player model");
                return;
            }
            string prefab = ValueOr("DURANGO_RENDER_MODEL_ASSET", "assets/models/custom-player.prefab");
            _modelEnabled = _assets.ReplaceLocalPlayerModel("sample-player-model", "sample", prefab, true);
            _api.ShowMessage(_modelEnabled ? "Custom player model enabled" : "Could not enable custom player model");
        }

        private void SpawnEffect()
        {
            if (_assets == null || (!LoadSampleBundle())) return;
            string prefab = ValueOr("DURANGO_RENDER_EFFECT_ASSET", "assets/effects/sample-effect.prefab");
            bool spawned = _assets.SpawnPrefab("sample-effect", "sample", prefab,
                ClientAssetAnchor.LocalPlayer, new Vector3(0f, 1.2f, 0f), Vector3.zero, Vector3.one);
            _api.ShowMessage(spawned ? "Attached mod effect" : "Could not attach mod effect");
        }

        private void RestoreEverything()
        {
            if (_assets == null) return;
            _assets.RestoreAllLocalPlayerAppearance();
            _assets.DestroySpawnedAsset("sample-effect");
            _modelEnabled = false;
            _api.ShowMessage("All ExampleRenderMod visuals restored");
        }

        private static string ValueOr(string name, string fallback)
        {
            string value = Environment.GetEnvironmentVariable(name);
            return String.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
