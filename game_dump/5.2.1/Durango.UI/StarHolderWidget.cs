using UnityEngine;

namespace Durango.UI;

public class StarHolderWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _baseStar;

	[SerializeField]
	private SpriteData _iconStarOn;

	[SerializeField]
	private SpriteData _iconStarOff;

	[SerializeField]
	private SpriteData _iconStarOnDisabled;

	[SerializeField]
	private SpriteData _iconStarOffDisabled;

	[SerializeField]
	private int _maxCount;

	private bool _initialized;

	private int _starCount;

	private ListObjectPool<UISprite> _stars = new ListObjectPool<UISprite>();

	private bool _isEnabled;

	private SpriteData _iconForOn;

	private SpriteData _iconForOff;

	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (_isEnabled != value)
			{
				_isEnabled = value;
				if (_isEnabled)
				{
					_iconForOn = _iconStarOn;
					_iconForOff = _iconStarOff;
				}
				else
				{
					_iconForOn = _iconStarOnDisabled;
					_iconForOff = _iconStarOffDisabled;
				}
				Refresh();
			}
		}
	}

	public void Init()
	{
		if (_initialized)
		{
			return;
		}
		_stars.BaseObject = _baseStar;
		_stars.Init(null);
		_stars.Set(_maxCount);
		for (int i = 0; i < _maxCount; i++)
		{
			UISprite uISprite = _stars[i];
			if (uISprite != null)
			{
				uISprite.transform.localPosition = _baseStar.transform.localPosition + Vector3.right * i * _baseStar.width;
			}
		}
		IsEnabled = true;
		_initialized = true;
	}

	public void SetStars(int count)
	{
		_starCount = count;
		Refresh();
	}

	private void Refresh()
	{
		for (int i = 0; i < _maxCount; i++)
		{
			UISprite uISprite = _stars[i];
			if (uISprite != null)
			{
				if (i + 1 <= _starCount)
				{
					_iconForOn.Set(uISprite);
				}
				else
				{
					_iconForOff.Set(uISprite);
				}
			}
		}
	}
}
