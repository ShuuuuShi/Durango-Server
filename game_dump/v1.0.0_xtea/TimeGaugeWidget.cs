using UnityEngine;

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
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		if (TimeGauge.DateTimeYaml == null)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		float num = Mathf.Lerp((float)TimeGauge.DateTimeYaml.sunrise[0], (float)TimeGauge.DateTimeYaml.sunrise[1], 0.5f) / 24f;
		float num2 = Mathf.Lerp((float)TimeGauge.DateTimeYaml.sunset[0], (float)TimeGauge.DateTimeYaml.sunset[1], 0.5f) / 24f;
		_left = Mathf.Lerp(num, num2, 0.5f);
		_right = Mathf.Lerp(num2, num + 1f, 0.5f);
		if (_right > 1f)
		{
			_right -= 1f;
		}
		float num3 = num2 - num;
		Vector3 localPosition = ((Component)_split).transform.localPosition;
		localPosition.x = Mathf.Lerp(_bgLine.localCorners[0].x, _bgLine.localCorners[3].x, num3);
		((Component)_split).transform.localPosition = localPosition;
	}

	private void Update()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		float normalizedTime = TimeGauge.GetNormalizedTime();
		float num = _right;
		float left = _left;
		float num2;
		float num3;
		if (normalizedTime < left)
		{
			if (num > left)
			{
				num -= 1f;
			}
			num2 = 1f - (normalizedTime - num) / 0.5f;
			num3 = 1f;
		}
		else
		{
			num2 = (normalizedTime - left) / 0.5f;
			num3 = -1f;
		}
		Vector3 localPosition = _cursor.localPosition;
		localPosition.x = Mathf.Lerp(_bgLine.localCorners[0].x, _bgLine.localCorners[3].x, num2);
		_cursor.localPosition = localPosition;
		_cursor.localEulerAngles = Vector3.forward * num3 * 90f;
	}
}
