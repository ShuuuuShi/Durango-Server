using System;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ItemInfoView : MonoBehaviour
{
	[SerializeField]
	private UIPanel _clipPanel;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _itemName;

	[SerializeField]
	private UIWidget _prototypeWidget;

	[SerializeField]
	private ItemIconTex _iconSprite;

	[SerializeField]
	private UISprite _portraitSprite;

	[SerializeField]
	private UILabel _prototypeNameLabel;

	[SerializeField]
	private UILabel _prototypeLevelLabel;

	[SerializeField]
	private PetGaugeViewerWidget _expWidget;

	[SerializeField]
	private UIWidget _levelModifierWidget;

	[SerializeField]
	private UISpriteLabel _levelModifierLabel;

	[SerializeField]
	private UILabel _durabilityLabel;

	[SerializeField]
	private UISprite _durabilitySprite;

	[SerializeField]
	private UILabel _modifiableLabel;

	[SerializeField]
	private UISprite _modifiableSprite;

	[SerializeField]
	private UIWidget _durabilityWidget;

	[SerializeField]
	private UIWidget _modifiableWidget;

	[SerializeField]
	private UIWidget _ageWidget;

	[SerializeField]
	private UILabel _ageLabel;

	[SerializeField]
	private UIWidget _warningWidget;

	[SerializeField]
	private UILabel _warningLabel;

	[SerializeField]
	private string _modifiableIconName;

	[SerializeField]
	private string _unmodifiableIconName;

	[SerializeField]
	private RectLayoutComponent _container;

	private UIWidget _widget;

	private int _warningMargin;

	private bool _isLevelDown;

	private bool _isInit;

	private bool _isDirtyLayout;

	private float _expandRatio;

	private ItemData _targetItem;

	private Messages.Pet? _pet;

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

	public float ExpandHeight { get; private set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_warningMargin = _warningWidget.height - _warningLabel.fontSize;
			UIEventListener.Get(_prototypeWidget.gameObject).onClick = OnClickPrototypePanel;
			UIEventListener.Get(_durabilityWidget.gameObject).onClick = delegate(GameObject go)
			{
				OnClickDurabilityPanel(go);
			};
			UIEventListener uIEventListener = UIEventListener.Get(_durabilityWidget.gameObject);
			uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, TooltipBase.ToHover(OnClickDurabilityPanel));
			UIEventListener.Get(_modifiableWidget.gameObject).onClick = delegate(GameObject go)
			{
				OnClickModifiablePanel(go);
			};
			UIEventListener uIEventListener2 = UIEventListener.Get(_modifiableWidget.gameObject);
			uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, TooltipBase.ToHover(OnClickModifiablePanel));
			UIEventListener.Get(_ageWidget.gameObject).onClick = OnClickLifePanel;
		}
	}

	private void OnEnable()
	{
		SetExpandRatio(_expandRatio);
	}

	public void SetExpandRatio(float ratio)
	{
		if (_isDirtyLayout)
		{
			UpdateLayout();
		}
		_expandRatio = ratio;
		Widget.height = (int)((float)_container.ParentWidget.height - ExpandHeight * (1f - ratio));
		_clipPanel.UpdateAnchors();
	}

	public void Set([NotNull] ItemData item, string warningText)
	{
		Init();
		_targetItem = item;
		_pet = null;
		FillItemData(item);
		SetWarningText(warningText);
		_isDirtyLayout = true;
	}

	public void Set(Messages.Pet pet, string warningText)
	{
		Init();
		_targetItem = null;
		_pet = pet;
		FillPetData(pet);
		SetWarningText(warningText);
		_isDirtyLayout = true;
	}

	private void FillItemData([NotNull] ItemData item)
	{
		int contentCount = item.ContentCount;
		int num = (int)item.GetFloatAttribute("capacity");
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		string lvModifier = null;
		if (contentCount > 0 && num > 0)
		{
			text = item.Name;
			item = item.GetContent(0);
			text2 = item.Name;
			text3 = T._("{0} {1:lv:}", item.PrototypeName, item.Level);
			text4 = $"{contentCount}/{num}";
		}
		else
		{
			text2 = item.Name;
			text3 = item.PrototypeName;
			text4 = T._("{0:lv:}", item.Level);
		}
		_isLevelDown = item.OriginalLevel > item.Level;
		if (_isLevelDown)
		{
			lvModifier = T._("[icon=img_pet_arrow_down] {0:lv:}", item.OriginalLevel - item.Level);
		}
		double currentTime = Gauge.CurrentTime;
		Gauge durability = item.Durability;
		float num2 = durability.Get(currentTime);
		if (num2 <= 0f)
		{
			text2 = T._("부서진 {0}", text2);
		}
		_itemName.text = ((!string.IsNullOrEmpty(text)) ? $"{text2} ({text})" : text2);
		SetPrototypeInfo(text3, text4, lvModifier);
		SetIcon(item);
		SetDurability(item);
		SetModifiableInfo(item.ModifiableCount);
		SetExp(null);
		SetAge(null);
	}

	private void FillPetData(Messages.Pet pet)
	{
		_targetItem = null;
		Yaml.Pet pet2 = SingletonDict<int, Yaml.Pet>.Get(pet.EntityType);
		Animal animal = ((pet2 != null) ? SingletonDict<int, Animal>.Get(pet2.VehicleEntityType) : null);
		SetPrototypeInfo((pet2 != null) ? pet2.Name.ToString() : pet.GetPetName(), LocalizeUtil.FormatLevel(pet.Statistics.Level), null);
		SetPortrait(animal?.Portrait);
		SetDurability(null);
		SetModifiableInfo(null);
		SetExp(pet);
		SetAge(pet);
		_itemName.text = pet.GetPetName(includeRank: true);
	}

	private void UpdateLayout()
	{
		_isDirtyLayout = false;
		_container.UpdateLayout();
		UIWidget parentWidget = _container.ParentWidget;
		ExpandHeight = parentWidget.height - _titleWidget.height;
		if (_levelModifierWidget.gameObject.activeSelf)
		{
			int num = (int)(_levelModifierLabel.GetPosition(0f, 0f).x - _levelModifierWidget.localCorners[0].x);
			_levelModifierWidget.width = _levelModifierLabel.width + num * 2;
			_levelModifierWidget.SetPosition(Vector3.Lerp(_prototypeLevelLabel.localCorners[0], _prototypeLevelLabel.localCorners[1], 0.5f) + (_prototypeLevelLabel.printedSize.x + (float)num) * Vector3.right, 0f, 0.5f);
		}
	}

	private void SetWarningText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			_warningWidget.gameObject.SetActive(value: false);
			return;
		}
		_warningWidget.gameObject.SetActive(value: true);
		_warningLabel.text = text;
		_warningWidget.height = _warningLabel.height + _warningMargin;
	}

	private void SetPrototypeInfo(string text, string lv, string lvModifier)
	{
		if (_prototypeNameLabel.gameObject.SetActiveAnd(!string.IsNullOrEmpty(text)))
		{
			_prototypeNameLabel.text = text;
		}
		if (_prototypeLevelLabel.gameObject.SetActiveAnd(!string.IsNullOrEmpty(lv)))
		{
			_prototypeLevelLabel.text = lv;
			if (_levelModifierWidget.gameObject.SetActiveAnd(!string.IsNullOrEmpty(lvModifier)))
			{
				_levelModifierLabel.text = lvModifier;
			}
		}
	}

	private void SetIcon([CanBeNull] ItemData item)
	{
		_portraitSprite.transform.parent.gameObject.SetActive(value: false);
		if (item == null)
		{
			_iconSprite.gameObject.SetActive(value: false);
			return;
		}
		_iconSprite.SetIcon(item);
		_iconSprite.gameObject.SetActive(value: true);
	}

	private void SetPortrait(string portrait)
	{
		_iconSprite.gameObject.SetActive(value: false);
		if (string.IsNullOrEmpty(portrait))
		{
			_portraitSprite.transform.parent.gameObject.SetActive(value: false);
			return;
		}
		_portraitSprite.spriteName = portrait;
		_portraitSprite.transform.parent.gameObject.SetActive(value: true);
	}

	private void SetDurability([CanBeNull] ItemData item)
	{
		if (item == null || item.Durability == null)
		{
			_durabilityWidget.gameObject.SetActive(value: false);
			return;
		}
		Gauge durability = item.Durability;
		bool flag = item.Prototype != null && item.Prototype.ImmuneToTime;
		bool flag2 = item.Prototype != null && item.Prototype.TimeLimited;
		string spriteName;
		Color color;
		string text;
		if (flag)
		{
			spriteName = "bg_itemview_durability";
			color = Color.white;
			text = Util.LocalizedDurability(durability.Get(), durability.Max());
		}
		else if (flag2)
		{
			spriteName = "icon_make_alert";
			color = Color.black;
			double num = durability.When(0f);
			if (num > 0.0)
			{
				double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
				text = ((!(num > predictedServerTime)) ? T._("파괴됨") : T._("{0} 남음", TimedeltaFormatter.Format(num - predictedServerTime, 1, "min")));
			}
			else
			{
				float num2 = durability.Get();
				if (num2 > 0f)
				{
					float dailyVelocity = Singleton<Constants>.Instance.Durability.DailyVelocity;
					if (dailyVelocity < 0f)
					{
						num2 /= dailyVelocity;
						num2 *= -86400f;
						text = TimedeltaFormatter.Format(num2, 1, "min");
					}
					else
					{
						text = Util.LocalizedDurability(durability.Get(), durability.Max());
					}
				}
				else
				{
					text = T._("파괴됨");
				}
			}
		}
		else
		{
			spriteName = "img_clock_small";
			color = PresetColor.UIBlack;
			text = Util.LocalizedDurability(durability.Get(), durability.Max());
		}
		_durabilitySprite.spriteName = spriteName;
		_durabilitySprite.color = color;
		_durabilityLabel.text = text;
		_durabilityWidget.gameObject.SetActive(value: true);
	}

	private void SetModifiableInfo(int? modifiableCount)
	{
		if (!modifiableCount.HasValue)
		{
			_modifiableWidget.gameObject.SetActive(value: false);
			return;
		}
		_modifiableSprite.spriteName = ((!modifiableCount.HasValue || modifiableCount.GetValueOrDefault() <= 0) ? _unmodifiableIconName : _modifiableIconName);
		_modifiableLabel.text = Util.LocalizedModifiableCount(modifiableCount.Value);
		_modifiableWidget.gameObject.SetActive(value: true);
	}

	private void SetExp(Messages.Pet? pet)
	{
		if (_expWidget.gameObject.SetActiveAnd(pet.HasValue))
		{
			_expWidget.Set((float)pet.Value.Statistics.Exp / (float)pet.Value.Statistics.RequiredExp);
		}
	}

	private void SetAge(Messages.Pet? pet)
	{
		if (pet.HasValue)
		{
			_ageWidget.gameObject.SetActive(value: true);
			double seconds = pet.Value.Statistics.DerivedAbilities.Get(Derived.LifeSpan, 0f);
			string lifeSpanText = TimedeltaFormatter.Format(seconds, 1);
			_ageLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				PetStats stat = pet.Value.Stat;
				double valueOrDefault = stat.GrazedAt.GetValueOrDefault(Connections.Frontend.GetPredictedServerTime());
				double num = stat.AgingUntil - valueOrDefault;
				string arg;
				if (num > 0.0)
				{
					arg = TimedeltaFormatter.Format(num, 1);
					period = ((!stat.GrazedAt.HasValue) ? TimedeltaFormatter.NextPeriod(num) : 0f);
				}
				else
				{
					arg = T._("노화된");
					period = 0f;
				}
				text = $"[icon=icon_timer] {arg}/{lifeSpanText} [icon=img_loading_unknown_question1]";
			}));
		}
		else
		{
			_ageWidget.gameObject.SetActive(value: false);
		}
	}

	private void OnClickPrototypePanel(GameObject go)
	{
		if (_isLevelDown)
		{
			PopupTooltip(null, T._("캐릭터 레벨이 장비 레벨보다 낮으면 장비의 능력치가 낮게 발현됩니다."), go);
		}
	}

	private WidgetTooltipControl OnClickDurabilityPanel(GameObject go)
	{
		if (_targetItem == null)
		{
			return null;
		}
		string text = null;
		string title = null;
		if (_targetItem.IsDestroyed())
		{
			title = T._("내구도 안내");
			text = T._("내구도가 0이 되어 사용할 수 없습니다. 대부분의 아이템은 수리키트나 워프젬으로 <em>수리</em>하면 다시 사용할 수 있습니다.");
		}
		else
		{
			bool flag = _targetItem.Prototype != null && _targetItem.Prototype.ImmuneToTime;
			bool flag2 = _targetItem.Prototype != null && _targetItem.Prototype.TimeLimited;
			if (flag)
			{
				title = T._("내구도 안내");
				text = T._("장비나 도구는 <em>사용할 때</em>마다 내구도가 조금씩 줄어듭니다.");
			}
			else if (flag2)
			{
				title = T._("사용 기간 안내");
				text = T._("사용 횟수와 상관 없이 <em>남은 기간만큼</em> 사용할 수 있습니다.");
			}
			else
			{
				float num = _targetItem.Durability.Velocity();
				if (num < 0f)
				{
					title = T._("내구도 안내");
					text = T._("장비, 도구를 제외한 대부분의 아이템은 <em>하루에 {0:0.#}씩</em> 내구도가 줄어듭니다.", (0f - num) * 86400f);
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return PopupTooltip(title, text, go);
	}

	private static WidgetTooltipControl OnClickModifiablePanel(GameObject go)
	{
		return PopupTooltip(T._("가공 가능 횟수"), T._("아이템을 몇 번 더 가공할 수 있는지 표시합니다. 가공할 때마다 1씩 줄어들며, 가공 가능 횟수가 0일 때는 아이템을 더 이상 가공할 수 없습니다."), go);
	}

	private static void OnClickLifePanel(GameObject go)
	{
		PopupTooltip(T._("<em>수명</em>"), PetUtil.GetAgingTooltip(), go);
	}

	private static WidgetTooltipControl PopupTooltip(string title, string body, GameObject go = null)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(title, body, 400);
		widgetTooltipControl.Show(go, Vector2.zero, 5f);
		return widgetTooltipControl;
	}
}
