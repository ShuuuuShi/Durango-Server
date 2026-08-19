using System.Collections.Generic;
using EnvironmentData;
using ExploreData;
using FatigueData;
using L10N;
using TimerData;
using UnityEngine;

public class WorldMapEnvWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UIWidget _emblemWidget;

	[SerializeField]
	private UISprite _emblemSprite;

	[SerializeField]
	private UISprite _emblemBackSprite;

	[SerializeField]
	private UILabel _regionRole;

	[SerializeField]
	private UILabel _regionName;

	[SerializeField]
	private UILabel _regionLevel;

	[SerializeField]
	private UIWidget _fatigueWidget;

	[SerializeField]
	private UIWidget _extendWidget;

	[SerializeField]
	private UISpriteLabel _fatigueTimeLabel;

	[SerializeField]
	private UIScrollView _riskeyScrollView;

	[SerializeField]
	private ListObjectPool _riskyList;

	[SerializeField]
	private GameObject _noRisky;

	[SerializeField]
	private GameObject _riskyOpener;

	[SerializeField]
	private UISpriteLabel _riskyOpenerLabel;

	private UIWidget _invisibleBox;

	private Vector3 _baseEmblemPos;

	private Vector3 _baseFatigueTimePos;

	private Vector3 _baseNamePos;

	private UIWidget _widget;

	private Region _region;

	private Fatigue _fatigue;

	private float _timeLabelUpdateTimer;

	private bool _isExtendView = true;

	private int _enableFrame;

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
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		_baseEmblemPos = ((Component)_emblemWidget).transform.localPosition;
		_baseFatigueTimePos = ((Component)_fatigueWidget).transform.localPosition;
		_baseNamePos = ((Component)_regionName).transform.localPosition;
		UIEventListener.Get(_riskyOpener).onClick = OnClick_RiskyOpener;
		if (!Debug.isDebugBuild)
		{
			return;
		}
		UIEventListener.Get(((Component)_emblemWidget).gameObject).onClick = delegate
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (_region != null)
			{
				LineTooltipControl lineTooltipControl = UIManager.Popup.Tooltip<LineTooltipControl>();
				lineTooltipControl.SetObject(_region, visiblePrimitive: false, visibleStatic: false, visibleProperty: true);
				lineTooltipControl.Show(_emblemWidget, Vector2.zero, 3600f);
			}
		};
	}

	private void OnEnable()
	{
		_enableFrame = Time.frameCount;
		_invisibleBox = UIUtility.SetScrollViewInvisibleBox(_riskeyScrollView, _invisibleBox);
		GameSystem<FatigueSystem>.Instance().FatigueUpdated += OnUpdateFatigue;
		OnUpdateFatigue();
		HideExtendWidget();
	}

	private void OnDisable()
	{
		GameSystem<FatigueSystem>.Instance().FatigueUpdated -= OnUpdateFatigue;
	}

	private void OnUpdateFatigue()
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		_region = KSingleton<GameManager>.Instance().Region;
		_fatigue = GameSystem<FatigueSystem>.Instance().Fatigue;
		List<FatigueVelocity> fatigueVelocities = GameSystem<FatigueSystem>.Instance().FatigueVelocities;
		if (_region == null || _fatigue == null || fatigueVelocities == null)
		{
			return;
		}
		_emblemSprite.spriteName = _region.GetEmblem();
		_regionName.text = LocalizeSystem.Format("#worldmap_region_name_label", _region.Name);
		_regionRole.text = LocalizeUtil.Get(_region.Role());
		_regionRole.color = _region.GetColor();
		_regionLevel.text = T.Format("{0:lv:}", _region.Level);
		_emblemBackSprite.color = _region.GetColor();
		UpdateTimer();
		_riskyList.Set(fatigueVelocities.Count);
		int count = _riskyList.Count;
		if (count > 0)
		{
			Vector3 val = Vector3.zero;
			for (int i = 0; i < count; i++)
			{
				WorldMapEnvNode component = _riskyList[i].GetComponent<WorldMapEnvNode>();
				component.Set(fatigueVelocities[i]);
				((Component)component).transform.localPosition = val;
				val += Vector3.down * (float)component.Widget.height;
			}
			_noRisky.SetActive(false);
			((Component)_riskeyScrollView).gameObject.SetActive(true);
			_riskeyScrollView.ResetPosition();
		}
		else
		{
			_noRisky.SetActive(true);
			((Component)_riskeyScrollView).gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (_timeLabelUpdateTimer > 0f)
		{
			_timeLabelUpdateTimer -= Time.deltaTime;
		}
		else
		{
			UpdateTimer();
		}
	}

	private void UpdateTimer()
	{
		_timeLabelUpdateTimer = 1f;
		if (_fatigue != null)
		{
			float num = 0f;
			if (_fatigue.Velocity > 0.01f)
			{
				num = _fatigue.Remain(_fatigue.Max);
			}
			_fatigueTimeLabel.text = ((!(num > 0f)) ? "-" : TimerSystem.TimeToString(num, TimePeriod.Min));
		}
	}

	public void ShowExtendWidget()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (!_isExtendView)
		{
			_isExtendView = true;
			_riskyOpenerLabel.text = "[img_arrow_up:0.8]";
			((Component)_regionRole).gameObject.SetActive(true);
			((Component)_extendWidget).gameObject.SetActive(true);
			TweenAlpha.Begin(((Component)_regionRole).gameObject, 0.3f, 1f).delay = 0.3f;
			TweenAlpha.Begin(((Component)_extendWidget).gameObject, 0.3f, 1f).delay = 0.3f;
			InfoLayoutReposition(_baseEmblemPos, Vector3.one, _baseFatigueTimePos, _baseNamePos, Widget.height);
		}
	}

	public void HideExtendWidget()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		if (_isExtendView)
		{
			_isExtendView = false;
			_riskyOpenerLabel.text = "[img_arrow_down:0.8]";
			((Component)_regionRole).gameObject.SetActive(false);
			((Component)_extendWidget).gameObject.SetActive(false);
			_regionRole.alpha = 0f;
			_extendWidget.alpha = 0f;
			int fontSize = _regionName.fontSize;
			Vector3 fatigueTimePos = Vector3.down * ((float)fontSize * 2f + (float)_fatigueWidget.height * (1f - _fatigueWidget.pivotOffset.y) - 5f);
			Vector3 val = Vector3.down * (float)fontSize + Vector3.right * (float)fontSize;
			Vector3 emblemPos = val + Vector3.left * (float)(_regionName.width / 2 + fontSize);
			Vector3 emblemScale = Vector3.one * (float)fontSize * 1.5f / (float)_emblemWidget.width;
			int bgHeight = fontSize * 3 + _fatigueWidget.height;
			InfoLayoutReposition(emblemPos, emblemScale, fatigueTimePos, val, bgHeight);
		}
	}

	private void InfoLayoutReposition(Vector3 emblemPos, Vector3 emblemScale, Vector3 fatigueTimePos, Vector3 namePos, int bgHeight)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = ((Component)this).gameObject.activeInHierarchy && _enableFrame < Time.frameCount;
		AnimationWidget animationWidget = ((Component)_emblemWidget).gameObject.AddMissingComponent<AnimationWidget>();
		animationWidget.Duration = 0.3f;
		animationWidget.SetPosition(emblemPos, flag);
		animationWidget.SetScale(emblemScale, flag);
		AnimationWidget animationWidget2 = ((Component)_fatigueWidget).gameObject.AddMissingComponent<AnimationWidget>();
		animationWidget2.Duration = 0.3f;
		animationWidget2.SetPosition(fatigueTimePos, flag);
		AnimationWidget animationWidget3 = ((Component)_regionName).gameObject.AddMissingComponent<AnimationWidget>();
		animationWidget3.Duration = 0.3f;
		animationWidget3.SetPosition(namePos, flag);
		UIWidget background = _background;
		if (flag)
		{
			TweenHeight.Begin(background, 0.3f, bgHeight);
			return;
		}
		background.height = bgHeight;
		TweenHeight component = ((Component)background).GetComponent<TweenHeight>();
		if ((Object)(object)component != (Object)null)
		{
			((Behaviour)component).enabled = false;
		}
	}

	private void OnClick_RiskyOpener(GameObject go)
	{
		if (_isExtendView)
		{
			HideExtendWidget();
		}
		else
		{
			ShowExtendWidget();
		}
	}
}
