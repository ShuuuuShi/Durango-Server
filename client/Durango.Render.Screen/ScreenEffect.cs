using System.Linq;
using Durango.Render.Camera;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Render.Screen;

public class ScreenEffect : MonoBehaviour
{
	public static bool UseVignettingEffect = true;

	[SerializeField]
	private Material _devScreenEffectMaterial;

	[SerializeField]
	private Material _vignettingEffectMaterial;

	[SerializeField]
	private float _effectAlpha = 1f;

	[SerializeField]
	private float _vignettingStartTime = 21f;

	[SerializeField]
	private float _vignettingEndTime = 3f;

	[SerializeField]
	private Material[] _statusScreenEffects;

	private Material _currentScreenEffectMaterial;

	private Material _nextScreenEffectMaterial;

	private float _screenEffectFadeoutBegin = -1f;

	private float _screenEffectFadeinBegin = -1f;

	private string _latestAdded;

	private void Start()
	{
		GameSystem<StatusEffectSystem>.Instance().StatusEffectAdded += delegate(string entityId, string id)
		{
			if (!(entityId != GameManager.PlayerId))
			{
				StatusEffectTemplate[] array = SingletonDict<string, StatusEffectTemplate[]>.Instance.Get(id);
				if (array != null)
				{
					string screenEffectName = array.Select((StatusEffectTemplate template) => template.ScreenEffectName).FirstOrDefault((string s) => !string.IsNullOrEmpty(s));
					if (!string.IsNullOrEmpty(screenEffectName))
					{
						_devScreenEffectMaterial = _statusScreenEffects.FirstOrDefault((Material m) => m.name == screenEffectName);
						if (_devScreenEffectMaterial == null)
						{
						}
						_latestAdded = id;
					}
				}
			}
		};
		GameSystem<StatusEffectSystem>.Instance().StatusEffectRemoved += delegate(string entityId, string id)
		{
			if (!(entityId != GameManager.PlayerId) && id == _latestAdded)
			{
				_devScreenEffectMaterial = null;
			}
		};
	}

	private void OnPostRender()
	{
		RenderVignettingMaterial();
		RenderScreenMaterial();
	}

	private void RenderVignettingMaterial()
	{
		if (!(_vignettingEffectMaterial == null) && UseVignettingEffect && !UIBase.HasOpenedFullscreenUI)
		{
			float num = 1f;
			float num2 = TimeGauge.GetNormalizedTime() * 24f;
			if (num2 > _vignettingStartTime)
			{
				num2 -= _vignettingStartTime;
				num = Mathf.Clamp(num2, 0f, 1f);
			}
			else if (num2 > _vignettingEndTime)
			{
				num2 -= _vignettingEndTime;
				num = 1f - Mathf.Clamp(num2, 0f, 1f);
			}
			if (num > 0f)
			{
				_vignettingEffectMaterial.SetFloat("_VignettingAlpha", num);
				Graphics.Blit(null, Durango.Utils.Singleton<MainCamera>.Instance().TargetTexture, _vignettingEffectMaterial, 0);
			}
		}
	}

	private void RenderScreenMaterial()
	{
		Material material = null;
		if (_devScreenEffectMaterial != null)
		{
			material = _devScreenEffectMaterial;
		}
		float fadeAlpha = 1f;
		if (_screenEffectFadeoutBegin > 0f)
		{
			fadeAlpha = 1f - (Time.time - _screenEffectFadeoutBegin);
			if (fadeAlpha < 0f || _currentScreenEffectMaterial == null)
			{
				fadeAlpha = 0f;
				_currentScreenEffectMaterial = _nextScreenEffectMaterial;
				_screenEffectFadeoutBegin = -1f;
			}
			Blit(_currentScreenEffectMaterial, fadeAlpha);
			Blit(_nextScreenEffectMaterial, 1f);
		}
		else if (_screenEffectFadeinBegin > 0f)
		{
			fadeAlpha = Time.time - _screenEffectFadeinBegin;
			if (fadeAlpha >= 1f || _nextScreenEffectMaterial == null)
			{
				fadeAlpha = 1f;
				_screenEffectFadeinBegin = -1f;
				_screenEffectFadeoutBegin = Time.time;
			}
			Blit(_currentScreenEffectMaterial, 1f);
			Blit(_nextScreenEffectMaterial, fadeAlpha);
		}
		else
		{
			if (_nextScreenEffectMaterial != material)
			{
				_nextScreenEffectMaterial = material;
				_screenEffectFadeinBegin = Time.time;
			}
			Blit(_currentScreenEffectMaterial, fadeAlpha);
			Durango.Utils.Singleton<CustomColorCorrectionEffect>.Instance().EnableOverlayAndDirtTexture = _currentScreenEffectMaterial == null;
		}
	}

	private void Blit([CanBeNull] Material source, float fadeAlpha)
	{
		if (!(source == null))
		{
			source.SetFloat("_EffectAlpha", _effectAlpha * fadeAlpha);
			Graphics.Blit(null, Durango.Utils.Singleton<MainCamera>.Instance().TargetTexture, source, 0);
		}
	}
}
