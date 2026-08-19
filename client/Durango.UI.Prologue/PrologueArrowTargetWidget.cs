using Durango.Render.Camera;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueArrowTargetWidget : MonoBehaviour
{
	[SerializeField]
	private float _arrowScreenMargin = 100f;

	[SerializeField]
	private GameObject _arrowSprite;

	private Vector3 _targetPos;

	public bool WithinScreen { get; private set; }

	private void LateUpdate()
	{
		if (IsEnabled())
		{
			Vector3 world = _targetPos;
			Vector3 vector = _targetPos - PlayerBehavior.LocalPlayer.CurrentPosition;
			if (vector.magnitude > 3200f)
			{
				vector.Normalize();
				world = PlayerBehavior.LocalPlayer.CurrentPosition + vector * 3200f;
			}
			Vector3 vector2 = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			Vector3 vector3 = MainCamera.WorldToScreenPos(world);
			Vector3 vector4 = vector3 - vector2;
			vector4.z = 0f;
			float num = (64f + _arrowScreenMargin) / MainCamera.NGUIScale();
			if (vector3.x < num || vector3.x > (float)Screen.width - num || vector3.y < num || vector3.y >= (float)Screen.height - num)
			{
				float num2 = (float)Screen.width - num * 2f;
				float num3 = (float)Screen.height - num * 2f;
				float num4 = Mathf.Min(num2 / Mathf.Abs(vector4.x), num3 / Mathf.Abs(vector4.y));
				Vector3 nguiPos = vector2;
				nguiPos.x += vector4.x * 0.5f * num4;
				nguiPos.y += vector4.y * 0.5f * num4;
				nguiPos.z = 0f;
				base.transform.localPosition = MainCamera.ScreenPosToNGUIPos(nguiPos);
				vector4.Normalize();
				float num5 = Mathf.Atan2(vector4.x, vector4.y) * 57.29578f;
				num5 = 0f - num5;
				base.transform.localRotation = Quaternion.Euler(0f, 0f, num5);
				_arrowSprite.SetActive(value: true);
				WithinScreen = false;
			}
			else
			{
				base.transform.localPosition = MainCamera.ScreenPosToNGUIPos(vector3);
				_arrowSprite.SetActive(value: false);
				WithinScreen = true;
			}
		}
	}

	public bool ShowTargetIfEnabled(bool visible)
	{
		if (IsEnabled())
		{
			base.gameObject.SetActive(visible);
			if (!visible)
			{
				ClearTarget();
				return false;
			}
		}
		return true;
	}

	public bool FinishTargetIf()
	{
		if (IsEnabled() && WithinScreen)
		{
			ClearTarget();
			return false;
		}
		return true;
	}

	public void SetTarget(Vector3 target)
	{
		_targetPos = target;
		base.gameObject.SetActive(IsEnabled());
	}

	public void ClearTarget()
	{
		_targetPos = Vector3.zero;
		base.gameObject.SetActive(value: false);
	}

	public bool IsEnabled()
	{
		return _targetPos != Vector3.zero;
	}
}
