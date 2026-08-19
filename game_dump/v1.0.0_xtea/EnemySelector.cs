using UnityEngine;

public class EnemySelector : KSingleton<EnemySelector>
{
	[SerializeField]
	private UISprite _selectLine;

	private Transform _selectLineTransfom;

	[SerializeField]
	private float _selectTime;

	[SerializeField]
	private float _endDelay = 2f;

	[SerializeField]
	private float _arrowTailChaseDelay = 0.7f;

	private float _enableTime;

	private Transform _start;

	private Transform _end;

	private bool _showSelectAnimation;

	private TweenAlpha _alphaTweener;

	private UIRect _panel;

	public float Alpha
	{
		get
		{
			return _panel.alpha;
		}
		set
		{
			if ((!((Behaviour)_alphaTweener).enabled || _alphaTweener.to != value) && _panel.alpha != value)
			{
				_alphaTweener.from = _panel.alpha;
				_alphaTweener.to = value;
				_alphaTweener.tweenFactor = 0f;
				_alphaTweener.PlayForward();
			}
		}
	}

	protected override void OnAwake()
	{
		_selectLineTransfom = ((Component)_selectLine).transform;
		_alphaTweener = ((Component)this).GetComponent<TweenAlpha>();
		_panel = ((Component)this).GetComponent<UIRect>();
		((Component)_selectLine).gameObject.SetActive(true);
	}

	private void Update()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_start == (Object)null || (Object)(object)_end == (Object)null)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		float num = _enableTime + _selectTime;
		float num2;
		if (Time.time < num)
		{
			num2 = (Time.time - _enableTime) / _selectTime;
		}
		else
		{
			num2 = 1f;
			if (Time.time >= num + _endDelay)
			{
				End();
				return;
			}
		}
		Vector3 val = _start.position;
		val.y = 0f;
		Vector3 position = _end.position;
		position.y = 0f;
		if (_arrowTailChaseDelay > 0f)
		{
			float num3 = num + _arrowTailChaseDelay - 0.1f;
			float num4 = Mathf.Clamp01((Mathf.Min(Time.time, num3) - num) / _selectTime);
			val = Vector3.Lerp(val, position, num4);
		}
		Vector3 val2 = position - val;
		float num5 = ((Vector3)(ref val2)).magnitude / _selectLineTransfom.localScale.x;
		_selectLineTransfom.position = val;
		_selectLine.width = (int)(num5 * num2);
		float num6 = Mathf.Atan2(val2.z, val2.x) * 57.29578f;
		_selectLineTransfom.eulerAngles = new Vector3(90f, 0f, num6);
	}

	public void SetTarget(Transform target)
	{
		SetTarget(((Component)PlayerBehavior.LocalPlayer).transform, ((Component)target).transform);
	}

	public void SetTarget(Transform start, Transform end)
	{
		((Behaviour)this).enabled = true;
		_enableTime = Time.time;
		_start = start;
		_end = end;
		_showSelectAnimation = true;
		((Component)this).gameObject.SetActive(true);
		((Component)_selectLine).gameObject.SetActive(true);
		_panel.alpha = 0f;
		Alpha = 1f;
		Update();
	}

	public void End()
	{
		((Component)this).gameObject.SetActive(false);
		((Component)_selectLine).gameObject.SetActive(false);
		((Behaviour)this).enabled = false;
	}
}
