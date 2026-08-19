using UnityEngine;
using UnityEngine.EventSystems;

namespace Durango.AssetTest_PC;

public class TestCamera : MonoBehaviour
{
	[SerializeField]
	public float ZoomMin;

	[SerializeField]
	public float ZoomMax = 1f;

	private float _zoomCurrent = 0.5f;

	private Vector3 _beginMovePosition;

	private Vector3 _movingOffset = Vector3.zero;

	private Transform _cameraTarget;

	private Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

	public void MoveTo(Vector3 newTarget, float newZoomLevel = float.MinValue)
	{
		_movingOffset = newTarget - base.transform.Find("CameraTarget").position;
		_movingOffset.y = 0f;
		if (newZoomLevel >= ZoomMin && newZoomLevel <= ZoomMax)
		{
			_movingOffset.y = newZoomLevel - _zoomCurrent;
		}
	}

	private void Awake()
	{
		_cameraTarget = base.transform.Find("CameraTarget");
	}

	private void Update()
	{
		if (EventSystem.current.IsPointerOverGameObject(-1))
		{
			return;
		}
		if (_movingOffset.sqrMagnitude > Mathf.Epsilon)
		{
			Vector3 vector = Vector3.Lerp(_movingOffset, Vector3.zero, 0.5f);
			Vector3 vector2 = _movingOffset - vector;
			base.transform.position += new Vector3(vector2.x, 0f, vector2.z);
			_movingOffset = vector;
			UpdateZoomLevel(vector2.y);
			if (_movingOffset.sqrMagnitude < 1f)
			{
				base.transform.position += new Vector3(_movingOffset.x, 0f, _movingOffset.z);
				UpdateZoomLevel(_movingOffset.y);
				_movingOffset = Vector3.zero;
			}
			return;
		}
		Vector2 zero = Vector2.zero;
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			_groundPlane.Raycast(ray, out var enter);
			_beginMovePosition = ray.GetPoint(enter);
			_beginMovePosition.y = 0f;
		}
		else if (Input.GetKey(KeyCode.Mouse0))
		{
			Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
			_groundPlane.Raycast(ray2, out var enter2);
			Vector3 point = ray2.GetPoint(enter2);
			point.y = 0f;
			base.transform.position -= point - _beginMovePosition;
		}
		else
		{
			zero.x = Input.GetAxis("Horizontal");
			zero.y = Input.GetAxis("Vertical");
		}
		if (zero.magnitude > float.Epsilon)
		{
			Vector3 vector3 = base.transform.right * zero.x + base.transform.forward * zero.y;
			vector3.y = 0f;
			base.transform.position += vector3.normalized * 20f;
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (Mathf.Abs(axis) > float.Epsilon)
		{
			UpdateZoomLevel(axis);
		}
	}

	private void UpdateCameraTargetPosition()
	{
		Ray ray = new Ray(base.transform.position, base.transform.forward);
		if (_groundPlane.Raycast(ray, out var enter))
		{
			Vector3 point = ray.GetPoint(enter);
			_cameraTarget.position = point;
		}
	}

	private void UpdateZoomLevel(float offset)
	{
		_zoomCurrent += offset;
		float num = Mathf.Clamp(_zoomCurrent, ZoomMin, ZoomMax);
		offset -= _zoomCurrent - num;
		_zoomCurrent = num;
		base.transform.position += base.transform.forward * offset * 1000f;
		UpdateCameraTargetPosition();
	}
}
