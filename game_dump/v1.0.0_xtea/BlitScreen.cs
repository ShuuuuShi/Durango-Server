using UnityEngine;

public class BlitScreen : MonoBehaviour
{
	[ExposedInEditor(null)]
	private RenderTexture _renderTexture;

	private RenderTexture _rtRemoved;

	private void Update()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		if ((Object)(object)_renderTexture == (Object)null || _renderTexture.width != Screen.width || _renderTexture.height != Screen.height)
		{
			_rtRemoved = _renderTexture;
			_renderTexture = new RenderTexture(Screen.width, Screen.height, 24, (RenderTextureFormat)0);
			KSingleton<MainCamera>.Instance().TargetTexture = _renderTexture;
		}
	}

	private void OnDestroy()
	{
		Object.Destroy((Object)(object)_rtRemoved);
		Object.Destroy((Object)(object)_renderTexture);
		if (KSingleton<MainCamera>.HasInstance())
		{
			KSingleton<MainCamera>.Instance().TargetTexture = null;
		}
	}

	private void OnPostRender()
	{
		if ((Object)(object)_rtRemoved != (Object)null)
		{
			Object.Destroy((Object)(object)_rtRemoved);
			_rtRemoved = null;
		}
	}

	private void OnPreRender()
	{
		Graphics.Blit((Texture)(object)_renderTexture, (RenderTexture)null);
	}
}
