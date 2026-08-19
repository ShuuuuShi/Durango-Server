using Durango.Logic.Explore;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class WorldMapEnvWidget : MonoBehaviour
{
	[SerializeField]
	private RectLayout _layout;

	[SerializeField]
	private UIWidget _topWidget;

	[SerializeField]
	private UIWidget _extendWidget;

	[SerializeField]
	private UIWidget _openerWidget;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UIWidget _regionWidget;

	[SerializeField]
	private Transform _emblemParent;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _biomeLabel;

	[SerializeField]
	private GameObject _unstableFactor;

	[SerializeField]
	private UILabel _unstableFactorLabel;

	[SerializeField]
	private UIWidget _islandRemainedWidget;

	[SerializeField]
	private UILabel _islandLifeTimeLabel;

	[SerializeField]
	private UISprite _openerSprite;

	private UIWidget _widget;

	private Durango.Logic.Explore.Region _region;

	private float _timeLabelUpdateAt;

	private bool _isExtendView = true;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public static void ShowUnstableFactorTooltip(GameObject go)
	{
		string text = T._("<em>불안정 지수</em>에 따라 <em>난이도</em>와 <em>보상</em>이 증가합니다.");
		string text2 = T._("불안정 환경 효과 증가");
		string text3 = T._("야생 동물 전투력 증가");
		string text4 = T._("채집 확률 감소");
		string text5 = T._("경험치 획득량 증가");
		string text6 = T._("재료 속성 레벨 증가");
		string text7 = text + "\n\n[9E0B0FFF]" + text2 + "\n" + text3 + "\n" + text4 + "[-]\n<em>" + text5 + "\n" + text6 + "</em>";
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(T._("불안정 지수"), text7, 400);
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Horizontal;
		widgetTooltipControl.Show(go, Vector2.zero, 10f);
	}

	private void Awake()
	{
		UIEventListener.Get(_openerWidget.gameObject).onClick = OnClick_RiskyOpener;
		UIEventListener.Get(_unstableFactor.gameObject).onClick = ShowUnstableFactorTooltip;
		if (!Debug.isDebugBuild)
		{
			return;
		}
		UIEventListener.Get(_regionWidget.gameObject).onClick = delegate
		{
			if (_region != null)
			{
				LineTooltip lineTooltip = UIManager.Popup.Tooltip<LineTooltip>();
				lineTooltip.SetObject(_region, visiblePrimitive: false, visibleStatic: false, visibleProperty: true);
				lineTooltip.Show(_regionWidget, Vector2.zero, 3600f);
			}
		};
	}

	private void Start()
	{
		UpdateRegion();
	}

	private void OnEnable()
	{
		_islandRemainedWidget.gameObject.SetActive(!GameManager.Region.LifespanInvisible);
		UpdateIslandRemainingTime();
		if (_isExtendView)
		{
			ShowExtendWidget();
		}
		else
		{
			HideExtendWidget();
		}
	}

	private void UpdateRegion()
	{
		_region = GameManager.Region;
		Messages.Archipelago? archipelago = GameManager.Archipelago;
		_nameLabel.text = _region.Name;
		_levelLabel.text = T._("{0:lv:} {1}", _region.Level, LocalizeUtil.Get(_region.Role()));
		bool flag = archipelago.HasValue && archipelago.Value.UnstableFactor > 1;
		_unstableFactor.SetActive(flag);
		if (flag)
		{
			int unstableFactor = archipelago.Value.UnstableFactor;
			_unstableFactorLabel.text = $"<em>[icon=icon_unstable_factor] {unstableFactor}</em>";
		}
		_biomeLabel.text = LocalizeUtil.Get(_region.MajorBiome());
		Durango.Logic.Explore.Region.InstantiateIcon(_emblemParent, _region.GetEmblem());
		_layout.UpdateLayout();
	}

	private void Update()
	{
		if (_timeLabelUpdateAt < Time.time)
		{
			_timeLabelUpdateAt = Time.time + 1f;
			UpdateIslandRemainingTime();
		}
	}

	private void UpdateIslandRemainingTime()
	{
		if (_islandRemainedWidget.gameObject.activeSelf)
		{
			_islandLifeTimeLabel.text = WorldMapGroup.GetIslandLifeTimeText();
		}
	}

	private void ShowExtendWidget()
	{
		_isExtendView = true;
		_openerSprite.flip = UIBasicSprite.Flip.Vertically;
		Widget.bottomAnchor.absolute = 0;
		Widget.bottomAnchor.relative = 0f;
		_extendWidget.gameObject.SetActive(value: true);
		Widget.UpdateAnchors();
		_layout.UpdateLayout();
		_background.bottomAnchor.SetScreen(0f, 0f);
		_background.ResetAndUpdateAnchors();
	}

	private void HideExtendWidget()
	{
		_isExtendView = false;
		_openerSprite.flip = UIBasicSprite.Flip.Nothing;
		int num = _topWidget.height + _openerWidget.height + (_islandRemainedWidget.gameObject.activeSelf ? _islandRemainedWidget.height : 0);
		Widget.bottomAnchor.absolute = -num;
		Widget.bottomAnchor.relative = 1f;
		_extendWidget.gameObject.SetActive(value: false);
		Widget.UpdateAnchors();
		_layout.UpdateLayout();
		_background.bottomAnchor.Set(base.transform, 0f, 0f);
		_background.ResetAndUpdateAnchors();
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
