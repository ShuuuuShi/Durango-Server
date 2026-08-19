using UnityEngine;

public class TriggerPanningCamera : TriggerOnce
{
	[SerializeField]
	private float _duration = 3f;

	[SerializeField]
	private float _cameraPanningTime = 0.3f;

	[SerializeField]
	private float _cameraReturningTime = 0.3f;

	[SerializeField]
	private float _fovRatio = 1f;

	[SerializeField]
	private float _fovTime = 0.3f;

	[SerializeField]
	private float _fovReturningTime = 0.3f;

	[SerializeField]
	private Vector3 _cameraTargetPosRel = new Vector3(100f, 100f, 100f);

	[SerializeField]
	private string _onFinishCmd;

	public Vector3 CameraTargetPosRel
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _cameraTargetPosRel;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_cameraTargetPosRel = value;
		}
	}

	protected override bool TriggerEntered(Collider other)
	{
		BeginEvent();
		return true;
	}

	private void BeginEvent()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<CameraController>.Instance().SetCameraTargetPos(((Component)this).transform.position + CameraTargetPosRel, _cameraPanningTime, _fovRatio, _fovTime);
		KSingleton<PrologueManager>.Instance().DelayedCall(EndEvent, _duration);
	}

	private void EndEvent()
	{
		KSingleton<CameraController>.Instance().ResetCameraTarget(_cameraReturningTime, _fovReturningTime);
		if (!string.IsNullOrEmpty(_onFinishCmd))
		{
			((Component)KSingleton<PrologueManager>.Instance()).SendMessage(_onFinishCmd);
		}
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}
}
