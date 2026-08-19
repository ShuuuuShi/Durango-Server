using System;
using Durango.Logic.Faction;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils.Extensions;
using L10N;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class FactionSupportRequestList : UIWidget
{
	[SerializeField]
	private UILabel _factionTitleLabel;

	[SerializeField]
	private UILabel _gradeTextLabel;

	[SerializeField]
	private UILabel _pointsLabel;

	[SerializeField]
	private GameObject _labelSeparator1;

	[SerializeField]
	private UILabel _periodLabel;

	[SerializeField]
	private GameObject _labelSeparator2;

	[SerializeField]
	private UISprite _gaugeUpperSprite;

	[SerializeField]
	private UIWidget _requiredItemWidget;

	[SerializeField]
	private Durango.UI.Control.ItemIconWidget _requiredItemIcon;

	[SerializeField]
	private FactionSupportRequestIndexList _indexList;

	[SerializeField]
	private FactionSupportRequestNodeList _nodeList;

	[SerializeField]
	private UIWidget _nextArrow;

	[SerializeField]
	private UIWidget _prevArrow;

	[SerializeField]
	private RectLayoutComponent _leftContainerLayout;

	private float _prevNodeOffset = -1f;

	private int _resetIndex = -1;

	private FactionType _type;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			FactionSupportRequestIndexList indexList = _indexList;
			indexList.LevelSelected = (Action<int>)Delegate.Combine(indexList.LevelSelected, new Action<int>(OnLevelSelected));
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += UpdateRequiredItemCount;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= UpdateRequiredItemCount;
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (!Application.isPlaying)
		{
			return;
		}
		if (_resetIndex != -1)
		{
			int resetIndex = _resetIndex;
			_resetIndex = -1;
			SyncIndexList(resetIndex);
			SyncNodeList(resetIndex);
		}
		else
		{
			float currentOffset = _indexList.CurrentOffset;
			float currentOffset2 = _nodeList.CurrentOffset;
			bool isDragging = _indexList.ScrollView.isDragging;
			bool isDragging2 = _nodeList.ScrollView.isDragging;
			bool flag = currentOffset != _indexList.GoalOffset;
			bool flag2 = currentOffset2 != _nodeList.GoalOffset;
			float num = (isDragging ? (currentOffset / _indexList.NodeSize) : (isDragging2 ? (currentOffset2 / _nodeList.NodeSize) : (flag ? (currentOffset / _indexList.NodeSize) : ((!flag2) ? (currentOffset / _indexList.NodeSize) : (currentOffset2 / _nodeList.NodeSize)))));
			if (num != _prevNodeOffset)
			{
				_prevNodeOffset = num;
				if (isDragging)
				{
					SyncNodeList(num);
				}
				else if (isDragging2)
				{
					SyncIndexList(num);
				}
				else if (flag)
				{
					SyncNodeList(num);
				}
				else if (flag2)
				{
					SyncIndexList(num);
				}
				else
				{
					SyncNodeList(num);
				}
				_nextArrow.alpha = ((!(num <= (float)_nodeList.GetNodeCount() - 2.1f)) ? 0f : 1f);
				_prevArrow.alpha = ((!(num >= 0.9f)) ? 0f : 1f);
			}
		}
		_indexList.OnLateUpdate();
	}

	private void SyncIndexList(float nodeOffset)
	{
		_indexList.MoveTo(nodeOffset * _indexList.NodeSize, instant: true, restrictWithinPanel: false);
	}

	private void SyncNodeList(float nodeOffset)
	{
		if (nodeOffset < 0f)
		{
			nodeOffset *= 0.1f;
		}
		else if (nodeOffset >= (float)(_nodeList.GetNodeCount() - 1))
		{
			float num = nodeOffset - (float)(_nodeList.GetNodeCount() - 1);
			nodeOffset = (float)(_nodeList.GetNodeCount() - 1) + num * 0.1f;
		}
		_nodeList.MoveTo(nodeOffset * _nodeList.NodeSize, instant: true, restrictWithinPanel: false);
	}

	private void OnLevelSelected(int level)
	{
		_indexList.MoveTo((float)(level - 1) * _indexList.NodeSize, instant: false);
	}

	public void Refresh()
	{
		Durango.Logic.Faction.Faction faction = GameSystem<FactionSystem>.Instance().GetFaction(_type);
		Yaml.Faction info = SingletonDict<FactionType, Yaml.Faction>.Get(_type);
		if (faction == null || info == null)
		{
			return;
		}
		_factionTitleLabel.SetText(new SyncString(delegate(out string text, out float period)
		{
			text = $"[icon={IconMap.Get(_type)}] {info.Name}";
			double num2 = GameSystem<FactionSystem>.Instance().SupportRequestsEndAt - Connections.Frontend.GetPredictedServerTime();
			if (num2 > 0.0)
			{
				text += T._(" [size=19]{0} 뒤 목록 갱신 <help>{1}</help>", TimedeltaFormatter.Format(num2), T._("갱신시, 지원 목록이 바뀌고 요청 횟수가 초기화 됩니다."));
				period = (float)(num2 % (double)TimedeltaFormatter.CurrentMinUnit());
			}
			else
			{
				period = 0f;
			}
		}));
		_gradeTextLabel.text = $"[icon=faction_amity] <em>{info.Titles.Get<Gettext>(faction.Level - 1, string.Empty)}</em>";
		faction.GetFactionGaugeValues(out var current, out var max);
		float fillAmount = ((!((float)max > 0f)) ? 0f : ((float)current / (float)max));
		_gaugeUpperSprite.fillAmount = fillAmount;
		_pointsLabel.text = $"<em>{current}</em>/{max}";
		Vector3[] array = localCorners;
		Vector3 vector = Vector3.Lerp(array[2], array[3], 0.5f);
		_pointsLabel.SetPosition(vector + Vector3.left * 20f, 1f, 0.5f);
		_labelSeparator1.transform.localPosition = vector + Vector3.left * (20f + (float)_pointsLabel.width + 20f);
		_gradeTextLabel.SetPosition(vector + Vector3.left * (20f + (float)_pointsLabel.width + 40f), 1f, 0.5f);
		bool flag = faction.StartsAt > 0.0 && faction.EndsAt > 0.0;
		_periodLabel.gameObject.SetActive(flag);
		_labelSeparator2.gameObject.SetActive(flag);
		if (flag)
		{
			_labelSeparator2.transform.localPosition = _labelSeparator1.transform.localPosition + Vector3.left * (20f + (float)_gradeTextLabel.width + 20f);
			_periodLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				double num = faction.EndsAt - Connections.Frontend.GetPredictedServerTime();
				if (num > 0.0)
				{
					text = T._("{0} 남음", TimedeltaFormatter.Format(num, 1, "min"));
					period = (float)(num % (double)TimedeltaFormatter.CurrentMinUnit());
				}
				else
				{
					text = string.Empty;
					period = 0f;
				}
			}));
			_periodLabel.SetPosition(_labelSeparator2.transform.localPosition + Vector3.left * 20f, 1f, 0.5f);
		}
		UpdateRequiredItemCount();
		_indexList.Set(faction);
		_nodeList.Set(faction);
		_leftContainerLayout.UpdateLayout();
		UIUtility.UpdateAnchors(_indexList.transform);
		_indexList.Reposition();
		_nodeList.Reposition();
	}

	private void UpdateRequiredItemCount()
	{
		Durango.Logic.Faction.Faction faction = GameSystem<FactionSystem>.Instance().GetFaction(_type);
		if (faction != null)
		{
			ItemData requiredItemForSupportRequests = faction.GetRequiredItemForSupportRequests();
			bool flag = requiredItemForSupportRequests != null;
			_requiredItemWidget.gameObject.SetActive(flag);
			if (flag)
			{
				PrototypeEvaluator evaluator = new PrototypeEvaluator(requiredItemForSupportRequests.PrototypeName);
				int taggedItemCount = GameSystem<InventorySystem>.Instance().GetTaggedItemCount(evaluator, allowLocked: true);
				_requiredItemIcon.Set(requiredItemForSupportRequests, taggedItemCount, alwaysShowCount: true, ShowWarningTooltip);
			}
		}
	}

	private void ShowWarningTooltip()
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, T._("교환에 사용되는 아이템입니다. 교환시 잠겨진 아이템이 사용될 수 있습니다."), 350);
		widgetTooltipControl.Show(_requiredItemWidget, Vector2.zero, 100f);
	}

	public void Set(FactionType type)
	{
		Init();
		_type = type;
		Refresh();
		if (_requiredItemWidget.gameObject.activeSelf)
		{
			ShowWarningTooltip();
		}
		Durango.Logic.Faction.Faction faction = GameSystem<FactionSystem>.Instance().GetFaction(_type);
		if (faction != null)
		{
			_resetIndex = faction.Level - 1;
		}
	}

	public Transform GetRequestAvailableButtonTransform()
	{
		return _nodeList.GetRequestAvailableButtonTransform();
	}
}
