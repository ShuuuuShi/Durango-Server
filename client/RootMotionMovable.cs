using Durango.Model;
using Durango.Utils;
using UnityEngine;

public class RootMotionMovable
{
	private readonly CharacterBehavior _owner;

	private readonly Vector3 _initRootMotionWorldOffset;

	private bool _isRootMotionEnable = true;

	private bool _isInPlaceMotion;

	private bool _useLocalRootMotionYaw;

	private Vector3 _rootMotionForward = Vector3.zero;

	public Vector3 CurRootMotionForward
	{
		get
		{
			if (_rootMotionForward == Vector3.zero)
			{
				RootMotionExporter component = _owner.GetComponent<RootMotionExporter>();
				_rootMotionForward = ((!(component != null)) ? Vector3.forward : component._rootMotionForward);
			}
			return _owner.Bip001Transform.localToWorldMatrix.MultiplyVector(_rootMotionForward);
		}
	}

	public RootMotionMovable(CharacterBehavior characterBehavior)
	{
		_owner = characterBehavior;
		_initRootMotionWorldOffset = characterBehavior.MeshObjectTransform.localPosition;
		_initRootMotionWorldOffset.y = 0f;
	}

	public void SetActivateRootMotion(bool active)
	{
		_isRootMotionEnable = active;
	}

	public void SetInPlaceMotionMode(bool isInPlaceMotion)
	{
		_isInPlaceMotion = isInPlaceMotion;
	}

	public void SetLocalRootMotionYawMode(bool isIgnoreYaw)
	{
		_useLocalRootMotionYaw = isIgnoreYaw;
	}

	public void LateUpdateRootMotion()
	{
		if (_isRootMotionEnable && !_isInPlaceMotion)
		{
			if (!_useLocalRootMotionYaw)
			{
				ApplyRootMotionYaw();
			}
			ApplyRootMotionPosition();
		}
	}

	private void ApplyRootMotionYaw()
	{
		Vector3 curRootMotionForward = CurRootMotionForward;
		float num = Maths.CalcYaw(curRootMotionForward) - Maths.CalcYaw(_owner.transform);
		float y = Maths.NormalizeAngDeg(0f - num);
		_owner.Bip001Transform.localRotation = Quaternion.Euler(0f, y, 0f) * _owner.Bip001Transform.localRotation;
	}

	public void ApplyRootMotionPosition()
	{
		Vector3 position = _owner.RootMotionTransform.position;
		Vector3 vector = _owner.MeshObjectTransform.worldToLocalMatrix.MultiplyPoint(position);
		vector.y = 0f;
		_owner.MeshObjectTransform.localPosition = -vector;
	}

	public void ResetRootMotionOffset()
	{
		_owner.MeshObjectTransform.localPosition = _initRootMotionWorldOffset;
	}
}
