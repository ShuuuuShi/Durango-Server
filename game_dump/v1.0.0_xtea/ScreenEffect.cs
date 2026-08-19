using Player;
using UnityEngine;

public class ScreenEffect : MonoBehaviour
{
	[SerializeField]
	private Material _devScreenEffectMaterial;

	[SerializeField]
	private Material _fireScreenEffectMaterial;

	[SerializeField]
	private Material _icedScreenEffectMaterial;

	[SerializeField]
	private Material _hotScreenEffectMaterial;

	[SerializeField]
	private Material _coolScreenEffectMaterial;

	[SerializeField]
	private Material _bloodyScreenEffectMaterial;

	[SerializeField]
	private Material _vignettingEffectMaterial;

	[SerializeField]
	private float _effectAlpha = 1f;

	[SerializeField]
	private float _vignettingStartTime = 21f;

	[SerializeField]
	private float _vignettingEndTime = 3f;

	private Material _currentScreenEffectMaterial;

	private Material _nextScreenEffectMaterial;

	private float _screenEffectFadeoutBegin = -1f;

	private float _screenEffectFadeinBegin = -1f;

	private void OnPostRender()
	{
		RenderVignettingMaterial();
		RenderScreenMaterial();
	}

	private void RenderVignettingMaterial()
	{
		if (!((Object)(object)_vignettingEffectMaterial == (Object)null) && !((Object)(object)UIBase.FullScreenUI != (Object)null))
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
				Graphics.Blit((Texture)null, _vignettingEffectMaterial, 0);
			}
		}
	}

	private void RenderScreenMaterial()
	{
		Material val = null;
		if ((Object)(object)_devScreenEffectMaterial != (Object)null)
		{
			val = _devScreenEffectMaterial;
		}
		else
		{
			MainStatus mainStatus = KSingleton<PlayerController>.Instance().MainStatus;
			ScreenEffectType screenEffectType = mainStatus.GetScreenEffectType();
			if (screenEffectType != 0)
			{
				val = GetScreenEffectMaterial(screenEffectType);
			}
		}
		float num = 1f;
		if (_screenEffectFadeoutBegin > 0f)
		{
			num = 1f - (Time.time - _screenEffectFadeoutBegin);
			if (num < 0f || (Object)(object)_currentScreenEffectMaterial == (Object)null)
			{
				_currentScreenEffectMaterial = _nextScreenEffectMaterial;
				_screenEffectFadeoutBegin = -1f;
				_screenEffectFadeinBegin = Time.time;
			}
		}
		if (_screenEffectFadeinBegin > 0f)
		{
			num = Time.time - _screenEffectFadeinBegin;
			if (num >= 1f || (Object)(object)_currentScreenEffectMaterial == (Object)null)
			{
				_screenEffectFadeinBegin = -1f;
			}
		}
		if (num >= 1f && (Object)(object)_nextScreenEffectMaterial != (Object)(object)val)
		{
			_nextScreenEffectMaterial = val;
			_screenEffectFadeoutBegin = Time.time;
		}
		if ((Object)(object)_currentScreenEffectMaterial != (Object)null)
		{
			_currentScreenEffectMaterial.SetFloat("_EffectAlpha", _effectAlpha * num);
			Graphics.Blit((Texture)null, _currentScreenEffectMaterial, 0);
		}
	}

	private Material GetScreenEffectMaterial(ScreenEffectType screenEffectType)
	{
		return (Material)(screenEffectType switch
		{
			ScreenEffectType.Fire => _fireScreenEffectMaterial, 
			ScreenEffectType.Iced => _icedScreenEffectMaterial, 
			ScreenEffectType.Hot => _hotScreenEffectMaterial, 
			ScreenEffectType.Cool => _coolScreenEffectMaterial, 
			ScreenEffectType.Bloody => _bloodyScreenEffectMaterial, 
			ScreenEffectType.None => null, 
			_ => null, 
		});
	}
}
