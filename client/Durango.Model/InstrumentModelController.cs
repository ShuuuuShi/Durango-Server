using System.Collections.Generic;
using UnityEngine;

namespace Durango.Model;

public class InstrumentModelController : MonoBehaviour
{
	[SerializeField]
	private List<Transform> _vibrationBones = new List<Transform>();

	private Dictionary<Transform, Quaternion> _vibrationBonesInitRot = new Dictionary<Transform, Quaternion>();

	private Renderer _renderer;

	private float _boneFlinchBeginTime;

	[SerializeField]
	private Vector3 _vibrationDisplacement = new Vector3(0f, -0.1f, 0f);

	[SerializeField]
	private float _boneFlinchingDuration = 1f;

	private void Start()
	{
		_renderer = GetComponentInChildren<Renderer>();
		int count = _vibrationBones.Count;
		for (int i = 0; i < count; i++)
		{
			Transform transform = _vibrationBones[i];
			if (transform != null)
			{
				_vibrationBonesInitRot.Add(transform, transform.localRotation);
			}
		}
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		if (!(_renderer == null) && _renderer.isVisible)
		{
			AccumulateBoneFlinching();
		}
	}

	public void AccumulateBoneFlinching()
	{
		if (!(_boneFlinchBeginTime > 0f))
		{
			return;
		}
		if (Time.time - _boneFlinchBeginTime < _boneFlinchingDuration)
		{
			float flPercent = (Time.time - _boneFlinchBeginTime) / _boneFlinchingDuration;
			int count = _vibrationBones.Count;
			for (int i = 0; i < count; i++)
			{
				Transform transform = _vibrationBones[i];
				if (transform != null)
				{
					float num = BoneFlinchingController.SampleFlinching(flPercent) * 57.29578f;
					transform.localRotation = _vibrationBonesInitRot[transform];
					transform.Rotate(new Vector3(_vibrationDisplacement.x * num, _vibrationDisplacement.y * num, _vibrationDisplacement.z * num));
				}
			}
		}
		else
		{
			_boneFlinchBeginTime = -1f;
		}
	}

	public void Test()
	{
		_boneFlinchBeginTime = Time.time;
	}
}
