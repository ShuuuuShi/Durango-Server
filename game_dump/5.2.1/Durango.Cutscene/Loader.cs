using System;
using Durango.Render.Camera;
using Durango.UI;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Cutscene;

[ResourcePath("cutscene_loader")]
public class Loader : ResourceSingleton<Loader>
{
	private const string HideKey = "Cutscene";

	[EnumList(typeof(Type), false, 0, -1)]
	[SerializeField]
	private GameObjectType[] _cutscenePath;

	private Action _cutsceneEnded;

	public SceneBase Current { get; private set; }

	public void LoadCutscene(Type cutsceneType, [NotNull] Action cutsceneEnded, params object[] args)
	{
		if (Current != null)
		{
			return;
		}
		string assetPath = _cutscenePath[(int)cutsceneType];
		Singleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!(Current != null))
			{
				_cutsceneEnded = cutsceneEnded;
				Current = KUtility.Instantiate<SceneBase>(asset);
				if (Current == null)
				{
					_cutsceneEnded();
					_cutsceneEnded = null;
				}
				else
				{
					UIManager.FindScript<CutsceneGroup>().Open(cutsceneType, delegate
					{
						Singleton<MainCamera>.Instance().Camera.enabled = false;
						VisibleController.HideExceptFor(VisibleType.VisibleOnCutScene, hide: true, "Cutscene");
						Current.Play(UnloadCutscene, args);
					});
				}
			}
		});
	}

	public void UnloadCutscene()
	{
		if (Current == null)
		{
			return;
		}
		UIManager.FindScript<CutsceneGroup>().Close(delegate
		{
			if (_cutsceneEnded != null)
			{
				_cutsceneEnded();
				_cutsceneEnded = null;
			}
			if (Current != null)
			{
				UnityEngine.Object.Destroy(Current.gameObject);
				Current = null;
			}
			Singleton<MainCamera>.Instance().Camera.enabled = true;
			VisibleController.HideExceptFor(VisibleType.VisibleOnCutScene, hide: false, "Cutscene");
		});
	}
}
