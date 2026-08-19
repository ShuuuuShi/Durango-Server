using Shared.Battle;
using UnityEngine;

public class RidingStabilizer : MonoBehaviour
{
	private PlayerBehavior _player;

	private Transform _spine;

	private float _curRatio;

	private float _targetRatio;

	[SerializeField]
	private float _blendSpeed = 2f;

	private void Start()
	{
		_player = GetComponent<PlayerBehavior>();
		_spine = _player.GetBodyPartTransform(BodyPart.Back);
		SetActive(active: false);
	}

	public void SetActive(bool active)
	{
		_targetRatio = (active ? 1 : 0);
		base.enabled = base.enabled || active;
	}

	private void LateUpdate()
	{
		_curRatio = Mathf.MoveTowards(_curRatio, _targetRatio, Time.deltaTime * _blendSpeed);
		_spine.rotation = Quaternion.Slerp(_spine.rotation, Quaternion.Euler(_spine.rotation.eulerAngles.x, _spine.rotation.eulerAngles.y, _player.Driver.SpinePitchAtRiding), _curRatio);
		if (_curRatio <= 0f)
		{
			base.enabled = false;
		}
	}
}
