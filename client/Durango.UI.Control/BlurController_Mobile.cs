using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace Durango.UI.Control;

public class BlurController_Mobile : BlurControllerBase
{
	private Blur _gameBlur;

	private readonly List<KeyValuePair<string, BlurController.Mask>> _keys = new List<KeyValuePair<string, BlurController.Mask>>();

	private BlurController.Mask _state;

	private Blur GameBlur
	{
		get
		{
			if (_gameBlur != null)
			{
				return _gameBlur;
			}
			_gameBlur = ((!Singleton<OverlayCamera>.HasInstance()) ? null : Singleton<OverlayCamera>.Instance().GetComponent<Blur>());
			if (_gameBlur == null && GameManager.IsPrologueMode)
			{
				GameObject gameObject = GameObject.Find("PrologueCamera");
				if (gameObject != null)
				{
					_gameBlur = gameObject.GetComponent<Blur>();
				}
			}
			return _gameBlur;
		}
	}

	public override BlurController.Mask GetState()
	{
		return _state;
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

	public override bool BlurOn(string key, BlurController.Mask mask, UIBase.AnchorType blurAnchor)
	{
		if (mask == BlurController.Mask.None)
		{
			return BlurOff(key);
		}
		int num = IndexOf(key);
		if (num == -1)
		{
			_keys.Add(new KeyValuePair<string, BlurController.Mask>(key, mask));
		}
		else
		{
			_keys[num] = new KeyValuePair<string, BlurController.Mask>(key, mask);
		}
		return RefreshBlur();
	}

	public override bool BlurOff(string key)
	{
		int num = IndexOf(key);
		if (num == -1)
		{
			return false;
		}
		_keys.RemoveAt(num);
		return RefreshBlur();
	}

	private bool RefreshBlur()
	{
		BlurController.Mask mask = BlurController.Mask.None;
		for (int i = 0; i < _keys.Count; i++)
		{
			if (_keys[i].Value > mask)
			{
				mask = _keys[i].Value;
			}
		}
		return SetBlur(mask);
	}

	private bool SetBlur(BlurController.Mask mask)
	{
		if (mask == _state)
		{
			return false;
		}
		_state = mask;
		bool enabled = false;
		bool enabled2 = false;
		switch (_state)
		{
		case BlurController.Mask.Game:
			enabled = true;
			break;
		case BlurController.Mask.UI:
			enabled2 = true;
			break;
		case BlurController.Mask.BasedOnAnchor:
			enabled = true;
			break;
		}
		if (base.UIBlur != null)
		{
			base.UIBlur.enabled = enabled2;
		}
		if (GameBlur != null)
		{
			GameBlur.enabled = enabled;
		}
		return true;
	}
}
