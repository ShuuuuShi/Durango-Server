using UnityEngine;

namespace Durango.UI;

public class FatigueGaugeScrollSprite : MonoBehaviour
{
	public enum ScrollDirection
	{
		Vertical,
		Horizontal
	}

	[SerializeField]
	private UISprite _scrollSprite;

	[SerializeField]
	private float _speed;

	[SerializeField]
	private ScrollDirection _scrollDirection;

	private UIPanel _parent;

	private Transform _spriteTransform;

	private float _scrollLength;

	private Vector3 _defaultAngle;

	public float Speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = value;
		}
	}

	private void Start()
	{
		if (_scrollSprite == null)
		{
			base.enabled = false;
			return;
		}
		_spriteTransform = _scrollSprite.transform;
		UISpriteData atlasSprite = _scrollSprite.GetAtlasSprite();
		if (atlasSprite == null)
		{
			base.enabled = false;
			return;
		}
		_scrollLength = ((_scrollDirection != 0) ? ((float)atlasSprite.width * _spriteTransform.localScale.x) : ((float)atlasSprite.height * _spriteTransform.localScale.y));
		_defaultAngle = _spriteTransform.localEulerAngles;
		_parent = GetComponent<UIPanel>();
	}

	private void Update()
	{
		if (_spriteTransform == null)
		{
			base.enabled = false;
			return;
		}
		_spriteTransform.localEulerAngles = ((!(Speed > 0f)) ? (_defaultAngle + Vector3.forward * 180f) : _defaultAngle);
		Vector3 localPosition = _spriteTransform.localPosition;
		float num = ((_scrollDirection != 0) ? localPosition.x : localPosition.y);
		num += Speed * Time.deltaTime;
		num = Mathf.Repeat(num, _scrollLength);
		if (_scrollDirection == ScrollDirection.Vertical)
		{
			localPosition.y = num;
		}
		else
		{
			localPosition.x = num;
		}
		_spriteTransform.localPosition = localPosition;
		if (_parent != null)
		{
			_parent.alpha = Mathf.Abs(Speed);
		}
	}
}
