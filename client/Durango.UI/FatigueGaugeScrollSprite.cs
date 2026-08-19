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
		// [แก้เอง] เดิมไม่เช็ค null เลย
		//
		// resources.assets ของเกมชุดนี้อ่านบางส่วนไม่ได้ ("is corrupted!" ใน log ตั้งแต่บูต)
		// ทำให้ _scrollSprite ที่ผูกไว้ใน prefab มาเป็น null ⇒ Start() โยน exception
		// ⇒ _spriteTransform ไม่เคยถูกตั้งค่า ⇒ **Update() โยน NullReferenceException ทุกเฟรม**
		// Unity เขียน stack trace ลง log ทุกครั้ง (log บวม 700KB ใน 2 นาที) แล้วเกมอืดจนโหลดแมพไม่จบ
		if (_scrollSprite == null)
		{
			enabled = false;
			return;
		}
		_spriteTransform = _scrollSprite.transform;
		UISpriteData atlasSprite = _scrollSprite.GetAtlasSprite();
		if (atlasSprite == null)
		{
			enabled = false;
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
			enabled = false;      // กัน exception รัวทุกเฟรมถ้า Start ไม่ได้ทำงาน
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
