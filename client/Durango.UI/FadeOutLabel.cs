using System.Collections;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

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

	public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());

	public MapIndicator Indicator { get; private set; }

	public void Show(MapIndicator indicator, string text)
	{
		Indicator = indicator;
		_label.text = text;
		base.gameObject.SetActive(value: true);
		_label.alpha = 1f;
		StopAllCoroutines();
		StartCoroutine(CoUpdateAlpha());
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
		Singleton<MapIndicators>.Instance().HideToolTipLabel();
	}

	private void SetTransform()
	{
		_label.transform.parent = Indicator.Widget.transform;
		Vector2 vector = Vector3.Lerp(Indicator.Widget.localCorners[0], Indicator.Widget.localCorners[3], 0.5f);
		_label.SetPosition(vector + _posOffset, 0.5f, 1f);
	}
}
