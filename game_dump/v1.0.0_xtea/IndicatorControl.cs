using System.Collections;
using UnityEngine;

public class IndicatorControl : MonoBehaviour
{
	public delegate void IndicatorDelegate(IndicatorControl indicator);

	public IndicatorDelegate OnBegin;

	public IndicatorDelegate OnEnd;

	[SerializeField]
	private UISpriteLabel _text;

	[SerializeField]
	private Vector3 _positionOffset;

	[SerializeField]
	private float _heightOffset;

	[SerializeField]
	private float _holdingTime;

	[SerializeField]
	private float _fadeIn;

	[SerializeField]
	private float _fadeOut;

	private GameObject _target;

	public GameObject Target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
		}
	}

	public string Text
	{
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				((Component)_text).gameObject.SetActive(false);
				_text.text = string.Empty;
			}
			else
			{
				((Component)_text).gameObject.SetActive(true);
				_text.text = value;
			}
		}
	}

	public void Begin()
	{
		if ((Object)(object)Target == (Object)null)
		{
			Target = ((Component)PlayerBehavior.LocalPlayer).gameObject;
		}
		((MonoBehaviour)this).StartCoroutine(CoIndicatorRoutine());
	}

	private IEnumerator CoIndicatorRoutine()
	{
		Transform t = ((Component)this).transform;
		Transform target = Target.transform;
		UIWidget widget = ((Component)this).GetComponent<UIWidget>();
		widget.alpha = 0f;
		float timer2 = 0f;
		if (OnBegin != null)
		{
			OnBegin(this);
		}
		while ((Object)(object)Target != (Object)null && timer2 < _holdingTime)
		{
			widget.alpha = Mathf.Clamp01(timer2 / _fadeIn);
			float ratio = timer2 / _holdingTime;
			t.localPosition = MainCamera.WorldToNGUIPos(target.position + _positionOffset) + Vector3.up * _heightOffset * ratio;
			timer2 += Time.deltaTime;
			yield return null;
		}
		timer2 = 0f;
		while (timer2 < _fadeOut)
		{
			widget.alpha = 1f - Mathf.Clamp01(timer2 / _fadeOut);
			timer2 += Time.deltaTime;
			yield return null;
		}
		widget.alpha = 0f;
		Target = null;
		if (OnEnd != null)
		{
			OnEnd(this);
		}
	}
}
