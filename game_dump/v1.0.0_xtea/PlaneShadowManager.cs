using UnityEngine;

public class PlaneShadowManager : MonoBehaviour
{
	[SerializeField]
	private Transform _plane;

	[SerializeField]
	private Transform _lightDir;

	[SerializeField]
	private Camera _camera;

	[ExposedInEditor("그림자 각도 매 프레임 업데이트")]
	private bool _debugShadowUpdate;

	private void Start()
	{
		UpdateShadowMatrix();
	}

	public void UpdateShadowMatrix()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = _lightDir.forward;
		Vector3 val = -_plane.up;
		float num = Vector3.Dot(forward, val);
		Matrix4x4 val2 = default(Matrix4x4);
		((Matrix4x4)(ref val2))[0, 0] = num - val.x * forward.x;
		((Matrix4x4)(ref val2))[1, 0] = (0f - val.x) * forward.y;
		((Matrix4x4)(ref val2))[2, 0] = (0f - val.x) * forward.z;
		((Matrix4x4)(ref val2))[3, 0] = 0f;
		((Matrix4x4)(ref val2))[0, 1] = (0f - val.y) * forward.x;
		((Matrix4x4)(ref val2))[1, 1] = num - val.y * forward.y;
		((Matrix4x4)(ref val2))[2, 1] = (0f - val.y) * forward.z;
		((Matrix4x4)(ref val2))[3, 1] = 0f;
		((Matrix4x4)(ref val2))[0, 2] = (0f - val.z) * forward.x;
		((Matrix4x4)(ref val2))[1, 2] = (0f - val.z) * forward.y;
		((Matrix4x4)(ref val2))[2, 2] = num - val.z * forward.z;
		((Matrix4x4)(ref val2))[3, 2] = 0f;
		((Matrix4x4)(ref val2))[0, 3] = 0f;
		((Matrix4x4)(ref val2))[1, 3] = 0f;
		((Matrix4x4)(ref val2))[2, 3] = 0f;
		((Matrix4x4)(ref val2))[3, 3] = num;
		Shader.SetGlobalMatrix("_projectionMatrix", val2);
		Shader.SetGlobalMatrix("_viewInv", _camera.cameraToWorldMatrix);
		Shader.SetGlobalMatrix("_view", _camera.worldToCameraMatrix);
		Shader.SetGlobalVector("_planeNormal", Vector4.op_Implicit(_plane.up));
	}

	private void LateUpdate()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalMatrix("_viewInv", _camera.cameraToWorldMatrix);
		Shader.SetGlobalMatrix("_view", _camera.worldToCameraMatrix);
	}

	public void ResetShadow(Material mat)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		mat.SetMatrix("_viewInv", _camera.cameraToWorldMatrix);
		mat.SetMatrix("_view", _camera.worldToCameraMatrix);
	}
}
