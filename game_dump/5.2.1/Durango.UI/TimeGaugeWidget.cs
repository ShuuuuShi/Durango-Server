using UnityEngine;

namespace Durango.UI;

public class TimeGaugeWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _bgLine;

	[SerializeField]
	private Transform _cursor;

	[SerializeField]
	private UIWidget _split;

	private float _left;

	private float _right;

	private void OnEnable()
	{
		if (TimeGauge.DateTimeYaml == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		float num = (float)TimeGauge.DateTimeYaml.Sunrise[0] / 24f;
		float num2 = (float)TimeGauge.DateTimeYaml.Sunset[1] / 24f;
		_left = Mathf.Lerp(num, num2, 0.5f);
		_right = Mathf.Lerp(num2, num + 1f, 0.5f);
		if (_right > 1f)
		{
			_right -= 1f;
		}
		float t = num2 - num;
		Vector3 localPosition = _split.transform.localPosition;
		localPosition.x = Mathf.Lerp(_bgLine.localCorners[0].x, _bgLine.localCorners[3].x, t);
		_split.transform.localPosition = localPosition;
	}

	private void Update()
	{
		float normalizedTime = TimeGauge.GetNormalizedTime();
		float num = _right;
		float left = _left;
		float t;
		float num2;
		if (normalizedTime < left)
		{
			if (num > left)
			{
				num -= 1f;
			}
			t = 1f - (normalizedTime - num) / 0.5f;
			num2 = 1f;
		}
		else
		{
			t = (normalizedTime - left) / 0.5f;
			num2 = -1f;
		}
		Vector3 localPosition = _cursor.localPosition;
		localPosition.x = Mathf.Lerp(_bgLine.localCorners[0].x, _bgLine.localCorners[3].x, t);
		_cursor.localPosition = localPosition;
		_cursor.localEulerAngles = Vector3.forward * num2 * 90f;
	}
}
