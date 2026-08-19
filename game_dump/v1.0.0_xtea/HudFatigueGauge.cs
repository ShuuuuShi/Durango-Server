using EnvironmentData;
using UnityEngine;

public class HudFatigueGauge : MonoBehaviour
{
	[SerializeField]
	private UISprite _upper;

	[SerializeField]
	private UISprite _warningArea;

	[SerializeField]
	private Transform _currentArrow;

	[SerializeField]
	private string[] _arrowSprites;

	[SerializeField]
	private FatigueGaugeScrollSprite _scrollSprite;

	[SerializeField]
	private TweenColor _warningAlarm;

	[SerializeField]
	private float _warningAnimationRatio;

	[SerializeField]
	private float _scollSpeedModifier;

	private UIWidget _widget;

	private Fatigue _fatigue;

	private Fatigue.State _prevState = Fatigue.State.None;

	private int _prevLv;

	private readonly float[] _fatigueLevelTerm = new float[3] { 0.3f, 0f, 1f };

	private float _defaultPeriod;

	private Color _defaultColor;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void Awake()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)_warningAlarm).enabled = false;
		_defaultPeriod = _warningAlarm.duration;
		_defaultColor = _upper.color;
	}

	private void OnEnable()
	{
		GameSystem<FatigueSystem>.Instance().FatigueUpdated += OnUpdateFatigue;
		OnUpdateFatigue();
	}

	private void OnDisable()
	{
		GameSystem<FatigueSystem>.Instance().FatigueUpdated -= OnUpdateFatigue;
	}

	private void Update()
	{
		UpdateCurrent();
	}

	private void OnUpdateFatigue()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		_fatigue = GameSystem<FatigueSystem>.Instance().Fatigue;
		if (_fatigue == null)
		{
			return;
		}
		float ratio = _fatigue.GetRatio(_fatigue.Warning);
		((Component)_warningArea).transform.localPosition = Vector3.up * (float)Widget.height * ratio;
		_fatigueLevelTerm[1] = ratio;
		_prevLv = -1;
		UpdateCurrent();
		Fatigue.State state = _fatigue.GetState();
		if (_prevState != state)
		{
			_prevState = state;
			switch (state)
			{
			case Fatigue.State.Normal:
				((Behaviour)_warningAlarm).enabled = false;
				_upper.color = _defaultColor;
				break;
			case Fatigue.State.Warning:
				((Behaviour)_warningAlarm).enabled = true;
				_warningAlarm.duration = _defaultPeriod;
				break;
			case Fatigue.State.Danger:
				((Behaviour)_warningAlarm).enabled = true;
				_warningAlarm.duration = _defaultPeriod * _warningAnimationRatio;
				break;
			}
		}
	}

	private void UpdateCurrent()
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (_fatigue != null)
		{
			float val = _fatigue.Get();
			float num = Mathf.Clamp01(_fatigue.GetRatio(val));
			_upper.height = (int)((float)Widget.height * num);
			int fatigueLavel = GetFatigueLavel(num);
			if (_prevLv != fatigueLavel)
			{
				_prevLv = fatigueLavel;
				UISprite component = ((Component)_currentArrow).GetComponent<UISprite>();
				component.spriteName = _arrowSprites[fatigueLavel];
			}
			_scrollSprite.Speed = _fatigue.Velocity * _scollSpeedModifier;
			Vector3 localPosition = _currentArrow.localPosition;
			localPosition.y = _upper.height;
			_currentArrow.localPosition = localPosition;
		}
	}

	private int GetFatigueLavel(float ratio)
	{
		int num = 0;
		int i = 0;
		for (int num2 = _fatigueLevelTerm.Length; i < num2 && !(_fatigueLevelTerm[i] > ratio); i++)
		{
			num++;
		}
		return num;
	}
}
