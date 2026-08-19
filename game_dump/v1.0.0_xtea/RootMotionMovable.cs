using UnityEngine;

public class RootMotionMovable
{
	private readonly CharacterBehavior _owner;

	private readonly Transform _ownerTransform;

	private readonly Vector3 _initRootMotionWorldOffset;

	private bool _isServerSideRootMotionEnable = true;

	private bool _isInPlaceMotion;

	private bool _useLocalRootMotionYaw;

	private Vector3 _rootMotionForward = Vector3.zero;

	public Vector3 CurRootMotionForward
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			if (_rootMotionForward == Vector3.zero)
			{
				RootMotionExporter component = ((Component)_owner).GetComponent<RootMotionExporter>();
				_rootMotionForward = ((!((Object)(object)component != (Object)null)) ? Vector3.forward : component._rootMotionForward);
			}
			Matrix4x4 localToWorldMatrix = _owner.Bip001Transform.localToWorldMatrix;
			return ((Matrix4x4)(ref localToWorldMatrix)).MultiplyVector(_rootMotionForward);
		}
	}

	public RootMotionMovable(CharacterBehavior characterBehavior, Transform ownerTransform, Transform rootMotionTransform)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		_owner = characterBehavior;
		_ownerTransform = ownerTransform;
		Vector3 position = rootMotionTransform.position;
		Matrix4x4 worldToLocalMatrix = characterBehavior.MeshObjectTransform.worldToLocalMatrix;
		_initRootMotionWorldOffset = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint(position);
		_initRootMotionWorldOffset.y = 0f;
	}

	public void SetServerSideRootMotionEnable(bool isServerSideRootMotionEnable)
	{
		_isServerSideRootMotionEnable = isServerSideRootMotionEnable;
	}

	public void SetInPlaceMotionMode(bool isInPlaceMotion)
	{
		_isInPlaceMotion = isInPlaceMotion;
	}

	public void SetLocalRootMotionYawMode(bool isIgnoreYaw)
	{
		_useLocalRootMotionYaw = isIgnoreYaw;
	}

	public void LateUpdateRootMotion(Transform meshObjectTransformOut)
	{
		if (_isServerSideRootMotionEnable && !_isInPlaceMotion)
		{
			if (!_useLocalRootMotionYaw)
			{
				ApplyRootMotionYaw();
			}
			ApplyRootMotionPosition(meshObjectTransformOut);
		}
	}

	private void ApplyRootMotionYaw()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Vector3 curRootMotionForward = CurRootMotionForward;
		float num = KMathUtil.CalcYaw(curRootMotionForward) - KMathUtil.CalcYaw(_ownerTransform);
		float num2 = KMathUtil.NormalizeAngDeg(0f - num);
		_owner.Bip001Transform.localRotation = Quaternion.Euler(0f, num2, 0f) * _owner.Bip001Transform.localRotation;
	}

	private void ApplyRootMotionPosition(Transform meshObjectTransformOut)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = _owner.RootMotionTransform.position;
		Matrix4x4 worldToLocalMatrix = meshObjectTransformOut.worldToLocalMatrix;
		Vector3 val = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint(position);
		val.y = 0f;
		meshObjectTransformOut.localPosition = -val;
	}

	public void ResetRootMotionOffset(Transform meshObjectTransformOut)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		meshObjectTransformOut.localPosition = _initRootMotionWorldOffset;
	}
}
