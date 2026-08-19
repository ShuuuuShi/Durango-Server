using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class BlurController : KSingleton<BlurController>
{
	public enum Mask
	{
		None,
		Game,
		UI
	}

	private BlurOptimized _uiBlur;

	private BlurOptimized _gameBlur;

	private Mask _current;

	private readonly List<KeyValuePair<string, Mask>> _keys = new List<KeyValuePair<string, Mask>>();

	private bool _isInit;

	public event Action<Mask> BlurStateChanged;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UICamera uICamera = UICamera.FindCameraForLayer(LayerMask.NameToLayer("NGUI"));
			_uiBlur = ((!((Object)(object)uICamera == (Object)null)) ? ((Component)uICamera).GetComponent<BlurOptimized>() : null);
			_gameBlur = ((!KSingleton<OverlayCamera>.Exist()) ? null : ((Component)KSingleton<OverlayCamera>.Instance()).GetComponent<BlurOptimized>());
		}
	}

	private int IndexOf(string key)
	{
		for (int i = 0; i < _keys.Count; i++)
		{
			if (_keys[i].Key == key)
			{
				return i;
			}
		}
		return -1;
	}

	public static void BlurOn(string key, Mask mask)
	{
		BlurController blurController = KSingleton<BlurController>.Instance();
		if ((Object)(object)blurController == (Object)null)
		{
			return;
		}
		if (mask == Mask.None)
		{
			BlurOff(key);
			return;
		}
		int num = blurController.IndexOf(key);
		if (num == -1)
		{
			blurController._keys.Add(new KeyValuePair<string, Mask>(key, mask));
		}
		else
		{
			blurController._keys[num] = new KeyValuePair<string, Mask>(key, mask);
		}
		blurController.RefreshBlur();
	}

	public static void BlurOff(string key)
	{
		BlurController blurController = KSingleton<BlurController>.Instance();
		if (!((Object)(object)blurController == (Object)null))
		{
			int num = blurController.IndexOf(key);
			if (num != -1)
			{
				blurController._keys.RemoveAt(num);
				blurController.RefreshBlur();
			}
		}
	}

	private void RefreshBlur()
	{
		Mask mask = Mask.None;
		for (int i = 0; i < _keys.Count; i++)
		{
			if (_keys[i].Value > mask)
			{
				mask = _keys[i].Value;
			}
		}
		SetBlur(mask);
	}

	private void SetBlur(Mask mask)
	{
		Init();
		if (mask != _current)
		{
			_current = mask;
			bool enabled = false;
			bool enabled2 = false;
			switch (_current)
			{
			case Mask.Game:
				enabled = true;
				break;
			case Mask.UI:
				enabled2 = true;
				break;
			}
			if ((Object)(object)_uiBlur != (Object)null)
			{
				((Behaviour)_uiBlur).enabled = enabled2;
			}
			if ((Object)(object)_gameBlur != (Object)null)
			{
				((Behaviour)_gameBlur).enabled = enabled;
			}
			if (this.BlurStateChanged != null)
			{
				this.BlurStateChanged(_current);
			}
		}
	}
}
