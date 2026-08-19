using UnityEngine;

public class FatigueGaugeScrollSprite : MonoBehaviour
{
	[SerializeField]
	private UISprite _scrollSprite;

	[SerializeField]
	private float _speed;

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
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		_spriteTransform = ((Component)_scrollSprite).transform;
		UISpriteData atlasSprite = _scrollSprite.GetAtlasSprite();
		_scrollLength = (float)atlasSprite.height * _spriteTransform.localScale.y;
		_defaultAngle = _spriteTransform.localEulerAngles;
		_parent = ((Component)this).GetComponent<UIPanel>();
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = _spriteTransform.localPosition;
		float y = localPosition.y;
		_spriteTransform.localEulerAngles = ((!(_speed > 0f)) ? (_defaultAngle + Vector3.forward * 180f) : _defaultAngle);
		for (y += _speed * Time.deltaTime; y < 0f; y += _scrollLength)
		{
		}
		while (y > _scrollLength)
		{
			y -= _scrollLength;
		}
		localPosition.y = y;
		_spriteTransform.localPosition = localPosition;
		_parent.alpha = Mathf.Abs(Speed);
	}
}
