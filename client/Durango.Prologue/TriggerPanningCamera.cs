using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

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
			return _cameraTargetPosRel;
		}
		set
		{
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
		Singleton<CameraController>.Instance().Target(base.transform.position + CameraTargetPosRel, _cameraPanningTime).ZoomRatio(_fovRatio, _fovTime);
		Singleton<PrologueManager>.Instance().DelayedCall(EndEvent, _duration);
	}

	private void EndEvent()
	{
		Singleton<CameraController>.Instance().Target(null, _cameraReturningTime).Offset(Vector3.zero, _cameraReturningTime)
			.ZoomRatio(1f, _cameraReturningTime);
		if (!string.IsNullOrEmpty(_onFinishCmd))
		{
			Singleton<PrologueManager>.Instance().SendMessage(_onFinishCmd);
		}
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}
}
