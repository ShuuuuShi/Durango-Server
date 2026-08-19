using System.Collections;
using UnityEngine;

public class FadeOutLabel : MonoBehaviour
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private float _showTime;

	[SerializeField]
	private float _fadeoutTime;

	[SerializeField]
	private Vector2 _posOffset;

	private UIWidget _widget;

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());

	public MapIndicator Indicator { get; private set; }

	public void Show(MapIndicator indicator, string text)
	{
		Indicator = indicator;
		_label.text = text;
		((Component)this).gameObject.SetActive(true);
		_label.alpha = 1f;
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(CoUpdateAlpha());
	}

	private IEnumerator CoUpdateAlpha()
	{
		float showTime = 0f;
		SetTransform();
		while (showTime < _showTime)
		{
			showTime += Time.deltaTime;
			yield return null;
		}
		while (_label.alpha > 0f)
		{
			_label.alpha -= 1f / _fadeoutTime * Time.deltaTime;
			yield return null;
		}
		KSingleton<MapIndicators>.Instance().HideToolTipLabel();
	}

	private void SetTransform()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		((Component)_label).transform.parent = ((Component)Indicator.Widget).transform;
		((Component)_label).transform.localPosition = Vector2.op_Implicit(_posOffset);
	}
}
