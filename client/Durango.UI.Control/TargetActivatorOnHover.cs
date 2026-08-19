using Durango.Render.Camera;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class TargetActivatorOnHover : MonoBehaviour
{
	[SerializeField]
	[Tooltip("타겟이 활성화 되기까지 필요한 호버링 시간")]
	private float _threshold;

	[SerializeField]
	[Tooltip("타겟 활성화 기간, 0일경우 무제한")]
	private float _duration;

	[SerializeField]
	private GameObject _target;

	[SerializeField]
	[Tooltip("활성화시 타겟이 마우스 포인터를 따라감")]
	private bool _isFollowMouse;

	[SerializeField]
	[Tooltip("마우스 따라갈 경우 마우스 커서 기준으로 위치 Offset")]
	private Vector2 _cursorOffset;

	private float _hoveredTime;

	private float _activatedTime;

	private bool _isHovered;

	private bool _isActive;

	private bool _hasActivated;

	private void Awake()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		if (component == null)
		{
			base.gameObject.AddMissingComponent<BoxCollider>();
			UIWidget uIWidget = base.gameObject.AddMissingComponent<UIWidget>();
			uIWidget.autoResizeBoxCollider = true;
			uIWidget.ResizeCollider();
		}
		_target.SetActive(value: false);
	}

	private void OnEnable()
	{
		Init();
		Hide();
	}

	private void OnDisable()
	{
		Hide();
	}

	[UsedImplicitly]
	private void OnHover(bool isHover)
	{
		Init();
		_isHovered = isHover;
		if (isHover)
		{
			_hoveredTime = Time.time;
		}
		else
		{
			Hide();
		}
	}

	private void Init()
	{
		_hoveredTime = 0f;
		_activatedTime = 0f;
		_isActive = false;
		_isHovered = false;
		_hasActivated = false;
	}

	private void Show()
	{
		_isActive = true;
		_hasActivated = true;
		_activatedTime = Time.time;
		if (_target != null)
		{
			_target.SetActive(value: true);
			if (_isFollowMouse)
			{
				MoveTargetToCursor();
			}
		}
	}

	private void Hide()
	{
		_isActive = false;
		if (_target != null)
		{
			_target.SetActive(value: false);
		}
	}

	private void MoveTargetToCursor()
	{
		if (!(_target == null))
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition = MainCamera.ScreenPosToNGUIPos(mousePosition, _target.transform.parent);
			mousePosition.x += _cursorOffset.x;
			mousePosition.y += _cursorOffset.y;
			_target.transform.localPosition = mousePosition;
		}
	}

	private void LateUpdate()
	{
		if (!_isHovered)
		{
			return;
		}
		if (_isActive)
		{
			if (!Mathf.Approximately(_duration, 0f) && _activatedTime + _duration < Time.time)
			{
				Hide();
			}
			else if (_isFollowMouse)
			{
				MoveTargetToCursor();
			}
		}
		else if (!_hasActivated && _hoveredTime + _threshold < Time.time)
		{
			Show();
		}
	}
}
