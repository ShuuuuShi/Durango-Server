using System.Collections.Generic;

namespace Durango.UI.Control;

public class BlurController_PC : BlurControllerBase
{
	private struct BlurData
	{
		public string Key;

		public BlurController.Mask BlurMask;

		public UIBase.AnchorType BlurAnchor;

		public bool Equals(BlurData rhs)
		{
			if (BlurMask == rhs.BlurMask)
			{
				if (BlurMask != BlurController.Mask.BasedOnAnchor)
				{
					return true;
				}
				return BlurAnchor == rhs.BlurAnchor;
			}
			return false;
		}
	}

	private BlurTexture _blurTexture;

	private readonly List<BlurData> _keys = new List<BlurData>();

	private BlurData _lastBlurState;

	private BlurTexture BlurTexture
	{
		get
		{
			if (_blurTexture != null)
			{
				return _blurTexture;
			}
			_blurTexture = UIManager.FindScript<BlurTexture>();
			return _blurTexture;
		}
	}

	public override BlurController.Mask GetState()
	{
		return _lastBlurState.BlurMask;
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
			_keys.Add(new BlurData
			{
				Key = key,
				BlurMask = mask,
				BlurAnchor = blurAnchor
			});
		}
		else
		{
			_keys[num] = new BlurData
			{
				Key = key,
				BlurMask = mask,
				BlurAnchor = blurAnchor
			};
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
		BlurData blurData = default(BlurData);
		blurData.BlurMask = BlurController.Mask.None;
		BlurData blur = blurData;
		for (int i = 0; i < _keys.Count; i++)
		{
			if (_keys[i].BlurMask > blur.BlurMask)
			{
				blur = _keys[i];
			}
		}
		return SetBlur(blur);
	}

	private bool SetBlur(BlurData blurData)
	{
		bool result = !blurData.Equals(_lastBlurState);
		_lastBlurState = blurData;
		bool flag = false;
		bool enabled = false;
		bool show = false;
		switch (_lastBlurState.BlurMask)
		{
		case BlurController.Mask.BasedOnAnchor:
			show = true;
			break;
		case BlurController.Mask.Game:
			flag = true;
			break;
		case BlurController.Mask.UI:
			enabled = true;
			break;
		}
		UIBase.AnchorType anchorType = blurData.BlurAnchor;
		if (flag)
		{
			anchorType = UIBase.AnchorType.Base;
			show = true;
		}
		if (base.UIBlur != null)
		{
			base.UIBlur.enabled = enabled;
		}
		if (BlurTexture != null)
		{
			BlurTexture.Show(show, anchorType);
		}
		return result;
	}
}
