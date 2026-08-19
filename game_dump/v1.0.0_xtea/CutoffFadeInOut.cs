using UnityEngine;

public class CutoffFadeInOut : MonoBehaviour
{
	public float _startAlpha = 1f;

	public float _endAlpha;

	public float _duration = 5f;

	private float _startTime;

	private bool _activated;

	private Material _material;

	private void Start()
	{
		StartFadeOut();
	}

	[ExposedInEditor(null)]
	private void StartFadeOut()
	{
		_material = ((Component)this).gameObject.GetComponent<Renderer>().material;
		if (!((Object)(object)_material == (Object)null) && _material.HasProperty("_Cutoff"))
		{
			_activated = true;
			_startTime = Time.realtimeSinceStartup;
			_material.SetFloat("_Cutoff", _startAlpha);
		}
	}

	private void Update()
	{
		if (_activated)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float num = realtimeSinceStartup - _startTime;
			if (_duration <= num)
			{
				_activated = false;
				_material.SetFloat("_Cutoff", _endAlpha);
			}
			else
			{
				float num2 = num / _duration;
				float num3 = _startAlpha * (1f - num2) + _endAlpha * num2;
				_material.SetFloat("_Cutoff", num3);
			}
		}
	}
}
