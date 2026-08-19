using UnityEngine;

public class WaveBehaviour : MonoBehaviour
{
	private Vector3 _InitPosition;

	private Vector3 _InitScale;

	public float _FirstStartTime;

	private float _StartTime;

	public float _HighTideDuration = 4f;

	public float _HighTideDistance = 800f;

	public float _HighTideScale = 0.5f;

	public float _HighTideVariation = 2f;

	public float _LowTideDuration = 10f;

	public float _LowTideDistance = 400f;

	public float _LowTideScale = 4f;

	public float _LowTideVariation = 4f;

	public float _IntervalToNext = 1f;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_InitPosition = ((Component)this).transform.localPosition;
		_InitScale = ((Component)this).transform.localScale;
	}

	private void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 1f, 1f, 0f));
		_StartTime = Time.realtimeSinceStartup;
	}

	private void Update()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.realtimeSinceStartup - _FirstStartTime;
		float num2 = num - _StartTime;
		if (!(num2 < 0f))
		{
			if (num2 < _HighTideDuration)
			{
				float num3 = num2 / _HighTideDuration;
				float num4 = 1f - Mathf.Pow(1f - num3, _HighTideVariation);
				float num5 = _InitScale.x * (1f - num3) + _InitScale.x * _HighTideScale * num3;
				((Component)this).transform.localPosition = _InitPosition + new Vector3(num4 * _HighTideDistance + 5f * (1f - num5), 0f, 0f);
				((Component)this).transform.localScale = new Vector3(num5, _InitScale.y, _InitScale.z);
				((Component)this).GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 1f, 1f, num3));
			}
			else if (num2 < _HighTideDuration + _LowTideDuration)
			{
				float num6 = (num2 - _HighTideDuration) / _LowTideDuration;
				float num7 = Mathf.Pow(num6, _LowTideVariation);
				float num8 = _InitScale.x * _HighTideScale * (1f - num6) + _InitScale.x * _LowTideScale * num6;
				((Component)this).transform.localPosition = _InitPosition + new Vector3(_HighTideDistance - num7 * _LowTideDistance + 5f * (1f - num8), 0f, 0f);
				((Component)this).transform.localScale = new Vector3(num8, _InitScale.y, _InitScale.z);
				((Component)this).GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 1f, 1f, 1f - num6));
			}
			else if (num2 > _HighTideDuration + _LowTideDuration + _IntervalToNext)
			{
				_StartTime = num;
				((Component)this).transform.localScale = _InitScale;
				((Component)this).GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 1f, 1f, 0f));
			}
		}
	}
}
