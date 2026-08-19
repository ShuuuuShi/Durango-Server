using UnityEngine;

namespace Durango.UI.Control;

public class CountableNotificationLabel : UILabel
{
	[SerializeField]
	private UISprite _backgroundSprite;

	[SerializeField]
	private int _maxCount = 99;

	[SerializeField]
	private int _horizontalMargin = 5;

	[SerializeField]
	private int _verticalMargin = 5;

	[SerializeField]
	private int _minSize;

	[SerializeField]
	private bool _isAdjustBgSize = true;

	public void Set(int count)
	{
		if (count > 0)
		{
			base.gameObject.SetActive(value: true);
			base.text = ((_maxCount <= 0 || count <= _maxCount) ? count.ToString() : $"{_maxCount}+");
			if (_isAdjustBgSize)
			{
				Point2 point = new Point2(base.width, base.height);
				point.x = Mathf.Max(_minSize, point.x + _horizontalMargin * 2);
				point.y = Mathf.Max(_minSize, point.y + _verticalMargin * 2);
				_backgroundSprite.SetDimensions(point.x, point.y);
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void SetColor(Color col)
	{
		_backgroundSprite.color = col;
	}
}
