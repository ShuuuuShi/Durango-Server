using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PetInfoWidget : UIWidget
{
	public enum PetAction
	{
		[T.EnumName("소환불가")]
		None,
		[T.EnumName("소환하기")]
		Spawn,
		[T.EnumName("소환해제")]
		Return,
		[T.EnumName("귀속해제")]
		Reinify,
		[T.EnumName("풀어주기")]
		Release,
		[T.EnumName("등급 초기화")]
		RevertRank,
		[T.EnumName("방목하기")]
		PutInToStorage,
		[T.EnumName("데려오기")]
		TakeOutFromStorage
	}

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private PetGaugeViewerWidget _expGaugeViewer;

	[SerializeField]
	private UILabel _containerSizeLabel;

	[SerializeField]
	private UILabel _speedLabel;

	[SerializeField]
	private KWidgetScrollView _infosScrollView;

	[SerializeField]
	private RectLayoutComponent _statArea;

	[SerializeField]
	private UILabel _lifeLabel;

	[SerializeField]
	private UILabel _agingLabel;

	[SerializeField]
	private UILabel _hungryLabel;

	[SerializeField]
	private UILabel _lifeValueLabel;

	[SerializeField]
	private UILabel _agingValueLabel;

	[SerializeField]
	private UILabel _hungryValueLabel;

	[SerializeField]
	private PetGaugeViewerWidget _lifeGaugeViewer;

	[SerializeField]
	private PetGaugeViewerWidget _agingGaugeViewer;

	[SerializeField]
	private PetGaugeViewerWidget _hungryGaugeViewer;

	[SerializeField]
	private RectLayoutComponent _infoArea;

	[SerializeField]
	private UIWidget _attackForbiddenArea;

	[SerializeField]
	private UILabel _attackForbiddenLabel;

	[SerializeField]
	private GameObject _battleStatsArea;

	[SerializeField]
	private UILabel _attackLabel;

	[SerializeField]
	private UILabel _defenceLabel;

	[SerializeField]
	private UILabel _accuracyLabel;

	[SerializeField]
	private UILabel _attackValueLabel;

	[SerializeField]
	private UILabel _defenceValueLabel;

	[SerializeField]
	private UILabel _accuracyValueLabel;

	[SerializeField]
	private NestedPrefabLinker _tagsViewerLinker;

	[SerializeField]
	private PetCageRegionInfoWidget _petCageRegionInfoWidget;

	[SerializeField]
	private DefaultMultipleActionButton _actionButton;

	private PetsInfo _petsInfo;

	private Messages.Pet _pet;

	private readonly List<PetAction> _actions = new List<PetAction>();

	private TagsViewerWidget _tagsViewer;

	private bool _isInit;

	public event Action<PetAction, Messages.Pet> PetActionClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_lifeLabel.text = string.Format("[icon=pet_heart] {0}", T._("생명"));
		_agingLabel.text = string.Format("[icon=pet_time] {0} <weak>[icon=img_loading_unknown_question2]</weak>", T._("수명"));
		UIEventListener uIEventListener = UIEventListener.Get(_agingLabel.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickAgeLabel));
		_hungryLabel.text = string.Format("[icon=pet_energy] {0}", T._("활력"));
		_attackForbiddenLabel.text = T._("공격 불가");
		_attackLabel.text = T._("공격");
		_defenceLabel.text = T._("방어");
		_accuracyLabel.text = T._("명중");
		_tagsViewer = _tagsViewerLinker.Object.GetComponent<TagsViewerWidget>();
		_actionButton.Clicked = delegate
		{
			int index = _actionButton.Index;
			if (index >= 0 && index < _actions.Count && this.PetActionClicked != null)
			{
				this.PetActionClicked(_actions[index], _pet);
			}
		};
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			_pet = default(Messages.Pet);
			_petsInfo = default(PetsInfo);
		}
	}

	public void Set(Messages.Pet pet, PetsInfo petsInfo)
	{
		Init();
		bool flag = _pet.EntityId != pet.EntityId;
		_pet = pet;
		_petsInfo = petsInfo;
		PetStats stat = pet.Stat;
		_levelLabel.text = LocalizeUtil.FormatLevel(pet.Statistics.Level);
		_expGaugeViewer.Set((float)pet.Statistics.Exp / (float)pet.Statistics.RequiredExp);
		_containerSizeLabel.text = $"<weak>[icon=bg_equip_bag]</weak> {stat.InventoryUsage}<weak>/{pet.Statistics.DerivedAbilities.Get(Derived.InventoryCapacity, 0f):0}</weak>";
		_speedLabel.text = $"<weak>[icon=icon_se_charge]</weak> {pet.Statistics.DerivedAbilities.Get(Derived.Speed, 0f)}";
		CageInfo? cageInfo = pet.CageInfo;
		if (cageInfo.HasValue && !string.IsNullOrEmpty(pet.CageInfo.Value.RegionId))
		{
			_statArea.gameObject.SetActive(value: false);
			_petCageRegionInfoWidget.gameObject.SetActive(value: true);
			_petCageRegionInfoWidget.Set(pet.CageInfo.Value);
		}
		else
		{
			_statArea.gameObject.SetActive(value: true);
			_petCageRegionInfoWidget.gameObject.SetActive(value: false);
			_lifeValueLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				if (stat.Life == null)
				{
					text = "0/<weak></weak>";
					period = 0f;
				}
				else
				{
					text = $"{(int)stat.Life.Get()}/<weak>{(int)stat.Life.Max()}</weak>";
					period = 0.1f;
				}
			}));
			_hungryValueLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				if (stat.Hungry == null)
				{
					text = "0/<weak></weak>";
					period = 0f;
				}
				else
				{
					text = $"{(int)stat.Hungry.Get()}/<weak>{(int)stat.Hungry.Max()}</weak>";
					period = 0.1f;
				}
			}));
			_agingValueLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				double valueOrDefault = stat.GrazedAt.GetValueOrDefault(Connections.Frontend.GetPredictedServerTime());
				double num = stat.AgingUntil - valueOrDefault;
				if (num > 0.0)
				{
					text = TimedeltaFormatter.Format(num);
					period = ((!stat.GrazedAt.HasValue) ? TimedeltaFormatter.NextPeriod(num) : 0f);
				}
				else
				{
					text = string.Format("<alert>{0}</alert>", T._("노화됨"));
					period = 0f;
				}
			}));
		}
		_lifeGaugeViewer.Set(stat.Life);
		_hungryGaugeViewer.Set(stat.Hungry);
		_agingGaugeViewer.Set(stat.AgingSince, stat.AgingUntil, stat.GrazedAt);
		Yaml.Pet pet2 = SingletonDict<int, Yaml.Pet>.Get(pet.EntityType);
		if (pet2 != null && pet2.IsFightable)
		{
			_attackForbiddenArea.gameObject.SetActive(value: false);
			_battleStatsArea.gameObject.SetActive(value: true);
			_attackValueLabel.text = pet.Statistics.DerivedAbilities.Get(Derived.Attack, 0f).ToString("0");
			_defenceValueLabel.text = pet.Statistics.DerivedAbilities.Get(Derived.Defense, 0f).ToString("0");
			_accuracyValueLabel.text = pet.Statistics.DerivedAbilities.Get(Derived.Accuracy, 0f).ToString("0");
		}
		else
		{
			_attackForbiddenArea.gameObject.SetActive(value: true);
			_battleStatsArea.gameObject.SetActive(value: false);
		}
		_tagsViewer.SettingBegin();
		if (stat.Tags != null)
		{
			foreach (KeyValuePair<string, int> tag in stat.Tags)
			{
				_tagsViewer.AddTagData(tag.Key, tag.Value);
			}
		}
		_tagsViewerLinker.gameObject.SetActive(_tagsViewer.SettingEnd());
		UpdateActionButton();
		_statArea.UpdateLayout();
		_infoArea.UpdateLayout();
		if (flag)
		{
			_infosScrollView.Reposition();
		}
		else
		{
			_infosScrollView.ResetPosition();
		}
		UIUtility.UpdateAnchors(_infosScrollView.transform);
	}

	private static void OnClickAgeLabel(GameObject obj)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(string.Format("<em>{0}</em>", T._("수명")), PetUtil.GetAgingTooltip(), 400);
		widgetTooltipControl.AutoPosition = false;
		widgetTooltipControl.Show(60f);
		UIWidget component = obj.GetComponent<UIWidget>();
		Vector3 position = component.localCorners[1];
		position = component.transform.TransformPoint(position);
		UIUtility.SetPosition(pos: widgetTooltipControl.transform.parent.InverseTransformPoint(position), widget: widgetTooltipControl.Widget, pivotX: 0f, pivotY: 0f);
		widgetTooltipControl.IntoSafeArea();
		widgetTooltipControl.HideArrow();
	}

	private void UpdateActionButton()
	{
		_actions.Clear();
		Yaml.Pet pet = SingletonDict<int, Yaml.Pet>.Get(_pet.EntityType);
		if (Durango.Utils.Singleton<PetManager>.Instance().GetPet(_pet.EntityId).HasValue)
		{
			_actions.Add(PetAction.Return);
			if (pet != null && pet.IsReinifiable)
			{
				_actions.Add(PetAction.Reinify);
			}
			_actions.Add(PetAction.Release);
			_actions.Add(PetAction.RevertRank);
			_actions.Add(PetAction.PutInToStorage);
		}
		else if (_petsInfo.Pets.Data != null && _petsInfo.Pets.Data.Any((Messages.Pet o) => o.EntityId == _pet.EntityId))
		{
			CageInfo? cageInfo = _pet.CageInfo;
			if (!cageInfo.HasValue || string.IsNullOrEmpty(_pet.CageInfo.Value.RegionId))
			{
				_actions.Add(PetAction.Spawn);
				if (pet != null && pet.IsReinifiable)
				{
					_actions.Add(PetAction.Reinify);
				}
				_actions.Add(PetAction.Release);
				_actions.Add(PetAction.RevertRank);
				_actions.Add(PetAction.PutInToStorage);
			}
		}
		else if (_petsInfo.GrazedPets.Data != null && _petsInfo.GrazedPets.Data.Any((Messages.Pet o) => o.EntityId == _pet.EntityId))
		{
			_actions.Add(PetAction.TakeOutFromStorage);
		}
		if (_actions.Count > 0)
		{
			_actionButton.Disabled = false;
		}
		else
		{
			_actionButton.Disabled = true;
			_actions.Add(PetAction.None);
		}
		_actions.Sort();
		_actionButton.BeginLoadAction();
		foreach (PetAction action in _actions)
		{
			_actionButton.AddAction(action.GetName());
		}
		_actionButton.EndLoadAction();
	}
}
