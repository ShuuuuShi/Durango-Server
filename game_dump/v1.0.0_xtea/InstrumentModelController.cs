using System.Collections.Generic;
using UnityEngine;

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
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		_renderer = ((Component)this).GetComponentInChildren<Renderer>();
		int count = _vibrationBones.Count;
		for (int i = 0; i < count; i++)
		{
			Transform val = _vibrationBones[i];
			if ((Object)(object)val != (Object)null)
			{
				_vibrationBonesInitRot.Add(val, val.localRotation);
			}
		}
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		if (!((Object)(object)_renderer == (Object)null) && _renderer.isVisible)
		{
			AccumulateBoneFlinching();
		}
	}

	public void AccumulateBoneFlinching()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
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
				Transform val = _vibrationBones[i];
				if ((Object)(object)val != (Object)null)
				{
					float num = KUtility.FlinchingFunc(flPercent) * 57.29578f;
					val.localRotation = _vibrationBonesInitRot[val];
					val.Rotate(new Vector3(_vibrationDisplacement.x * num, _vibrationDisplacement.y * num, _vibrationDisplacement.z * num));
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
