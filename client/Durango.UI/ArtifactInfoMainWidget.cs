using System;
using System.Collections.Generic;
using System.Text;
using Durango.Logic.Estate;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using InteractionData;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Accelerator;
using Shared.Building;
using Shared.Estate;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ArtifactInfoMainWidget : MonoBehaviour
{
	public enum StatsType
	{
		Comfort,
		Antibacterial
	}

	private const int MaxHeight = 500;

	public readonly Dictionary<Interaction, InteractionMenuData> Interactions = new Dictionary<Interaction, InteractionMenuData>();

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private UIPanel _headPanel;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _subtitleWidget;

	[SerializeField]
	private UILabel _subTitleLabel;

	[SerializeField]
	private RectLayoutComponent _headLayout;

	[SerializeField]
	private KeyValueLabel _textLabelBase;

	[SerializeField]
	private UILabel _labelBase;

	[SerializeField]
	private NestedPrefabLinker _tagsViewerLinker;

	[SerializeField]
	private ItemGradeViewer _gradeViewer;

	[SerializeField]
	private GameObject _manageButton;

	[SerializeField]
	private ArtifactInfoRights _rightsWidget;

	[SerializeField]
	private ArtifactInfoContextInteriorMood _interiorMood;

	[SerializeField]
	private ArtifactInfoContextInteriorSet _interiorSet;

	[SerializeField]
	private UIWidget _separator;

	[SerializeField]
	private UIWidget _space;

	private ListObjectPool<KeyValueLabel> _textLabels;

	private ListObjectPool<UILabel> _labels;

	private ListObjectPool<UIWidget> _separators;

	private ListObjectPool<UIWidget> _spaces;

	private readonly List<global::System.Action> _onClickTextLabels = new List<global::System.Action>();

	private TagsViewerWidget _tagsViewer;

	private bool _isFillData;

	private float _detailViewOffset;

	private int _titleHeight;

	private Artifact _artifact;

	public UIWidget Widget { get; private set; }

	public ArtifactAccess? Access { get; private set; }

	public event global::System.Action ManageButtonClicked;

	public event Action<bool> LayoutUpdated;

	public event Action<StatsType> ArtifactStatsInfoClicked;

	private void Awake()
	{
		Widget = GetComponent<UIWidget>();
		_textLabels = new ListObjectPool<KeyValueLabel>();
		_textLabels.BaseObject = _textLabelBase;
		_textLabels.UseBase = true;
		_textLabels.Init(delegate(KeyValueLabel obj)
		{
			UIEventListener.Get(obj.gameObject).onClick = OnClickInfoLabel;
		});
		_textLabels.Clear();
		_labels = new ListObjectPool<UILabel>();
		_labels.BaseObject = _labelBase;
		_labels.UseBase = true;
		_labels.Clear();
		_separators = new ListObjectPool<UIWidget>();
		_separators.BaseObject = _separator;
		_separators.UseBase = true;
		_separators.Clear();
		_spaces = new ListObjectPool<UIWidget>();
		_spaces.BaseObject = _space;
		_spaces.UseBase = true;
		_spaces.Clear();
		_rightsWidget.ManageButtonClicked = OnClickManageButton;
		_tagsViewer = _tagsViewerLinker.Object.GetComponent<TagsViewerWidget>();
		UIEventListener uIEventListener = UIEventListener.Get(_manageButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			OnClickManageButton();
		});
		_interiorMood.Init();
		_interiorMood.OnExpandChanged += Interior_OnExpandChanged;
		_interiorSet.Init();
		_interiorSet.OnExpandChanged += Interior_OnExpandChanged;
	}

	private void OnEnable()
	{
		Artifact.ArtifactStateChanged += Artifact_ArtifactStateChanged;
	}

	private void OnDisable()
	{
		Artifact.ArtifactStateChanged -= Artifact_ArtifactStateChanged;
	}

	private void Artifact_ArtifactStateChanged(Artifact artifact)
	{
		if (!(artifact != _artifact))
		{
			Refresh();
		}
	}

	public void SetArtifact(Artifact artifact)
	{
		_artifact = artifact;
		Interactions.Clear();
	}

	public void SetArtifactAccess(ArtifactAccess? access)
	{
		Access = access;
	}

	public void Refresh()
	{
		_isFillData = true;
		_scrollView.Widgets.Clear();
		_textLabels.BeginLoad();
		_labels.BeginLoad();
		_separators.BeginLoad();
		_spaces.BeginLoad();
		_onClickTextLabels.Clear();
		if (_artifact != null)
		{
			FillWarehouse();
			FillEffector();
			FillOccupiedWarning();
			FillPostprocess();
			FillFarming();
			FillHome();
			FillCrack();
			FillStoneCrack();
			FillSprinklable();
			FillInventory();
			FillCage();
			FillCatapult();
			FillLandowner();
			FillDefensive();
			FillWarpAccelerator();
			FillTags();
			FillStats();
			FillArtifactRights();
			FillInteriorMood();
			FillInteriorSet();
			FillTitle();
		}
		_textLabels.EndLoad();
		_labels.EndLoad();
		_separators.EndLoad();
		_spaces.EndLoad();
		for (int i = 0; i < _textLabels.Count; i++)
		{
			_textLabels[i].UpdateLayout(Widget.width);
		}
		_isFillData = false;
	}

	public int UpdateHeight(bool keepScrollOffset)
	{
		if (!keepScrollOffset)
		{
			_titleHeight = _titleWidget.height + (_subtitleWidget.gameObject.activeSelf ? _subtitleWidget.height : 0);
			_headPanel.bottomAnchor.absolute = -_titleHeight;
		}
		_scrollView.UpdateLayout();
		float f = _scrollView.ContentsLength + (float)_titleHeight;
		Widget.height = Mathf.Min(Mathf.CeilToInt(f), 500);
		return Widget.height;
	}

	public void UpdateLayout(bool keepScrollOffset)
	{
		if (keepScrollOffset)
		{
			_detailViewOffset = _scrollView.CurrentOffset;
			_scrollView.Reposition(resetPosition: false, tween: false);
		}
		else
		{
			_headLayout.UpdateLayout();
			_scrollView.Reposition(resetPosition: true, tween: false);
		}
	}

	public void UpdateScrollOffset()
	{
		if (!_subtitleWidget.gameObject.activeSelf)
		{
			return;
		}
		float currentOffset = _scrollView.CurrentOffset;
		if (_detailViewOffset == currentOffset)
		{
			return;
		}
		float num = -_headPanel.bottomAnchor.absolute;
		float num2 = _titleWidget.height;
		float num3;
		if (currentOffset > 0f && num > num2)
		{
			float b = num - num2;
			num3 = Mathf.Min(currentOffset, b);
		}
		else
		{
			if (!(currentOffset < 0f) || !(num < (float)_titleHeight))
			{
				_detailViewOffset = currentOffset;
				return;
			}
			float b = 0f - ((float)_titleHeight - num);
			num3 = Mathf.Max(currentOffset, b);
		}
		Vector3 currentMomentum = _scrollView.ScrollView.currentMomentum;
		_headPanel.bottomAnchor.absolute = (int)(0f - num + num3);
		UIUtility.UpdateAnchors(base.transform);
		_headLayout.UpdateLayout();
		_scrollView.UpdateLayout();
		_scrollView.MoveTo(0f, instant: true);
		_scrollView.ScrollView.currentMomentum = currentMomentum;
		_detailViewOffset = 0f;
	}

	private KeyValueLabel GetRawInfoLabel(global::System.Action onClick = null)
	{
		KeyValueLabel next = _textLabels.GetNext();
		_onClickTextLabels.Add(onClick);
		_scrollView.Widgets.Add(next.GetComponent<UIWidget>());
		return next;
	}

	private KeyValueLabel GetInfoLabel(global::System.Action onClick = null)
	{
		AddSeparator();
		AddSpace(18);
		KeyValueLabel rawInfoLabel = GetRawInfoLabel(onClick);
		AddSpace(18);
		return rawInfoLabel;
	}

	private UILabel AddTextLabel(SyncString text, Point2 padding)
	{
		UILabel next = _labels.GetNext();
		_scrollView.Widgets.Add(next);
		next.width = Widget.width - padding.x * 2;
		next.overflowMethod = UILabel.Overflow.ResizeHeight;
		next.SetText(text);
		Vector2 printedSize = next.printedSize;
		next.overflowMethod = UILabel.Overflow.ClampContent;
		next.height = (int)printedSize.y + padding.y * 2;
		return next;
	}

	private void AddSpace(int size)
	{
		if (size > 0)
		{
			UIWidget next = _spaces.GetNext();
			next.height = size;
			_scrollView.Widgets.Add(next);
		}
	}

	private void AddSeparator()
	{
		if (_scrollView.Widgets.Count > 0)
		{
			UIWidget next = _separators.GetNext();
			_scrollView.Widgets.Add(next);
		}
	}

	private void FillTitle()
	{
		_titleLabel.SetText(T._("{0} [size=22]{1:lv:}[/size]", _artifact.GetName(), _artifact.ArtifactState.Level));
		_titleWidget.height = _titleLabel.height + 28;
		SyncString subTitle = GetSubTitle();
		_subTitleLabel.SetText(subTitle);
		int num = (_gradeViewer.gameObject.activeSelf ? 28 : 0);
		num += (_manageButton.gameObject.activeSelf ? 42 : 0);
		num = Mathf.Max(num, subTitle.HasText() ? (_subTitleLabel.height + 20) : 0);
		_subtitleWidget.gameObject.SetActive(num > 0);
		_subtitleWidget.height = num;
	}

	private void FillStats()
	{
		if (!_artifact.ArtifactState.Stats.HasValue || _artifact.ArtifactState.BuildingState != BuildingState.Completed)
		{
			return;
		}
		ArtifactStats value = _artifact.ArtifactState.Stats.Value;
		if (_artifact.IsEnterable)
		{
			KeyValueLabel infoLabel = GetInfoLabel(OnClickComfortStats);
			infoLabel.Set(T._("총 안락함"), GetStatFactorText(value.Comfort.Factor, value.Comfort.Complexity));
			infoLabel = GetInfoLabel(OnClickAntibacterialStats);
			infoLabel.Set(T._("총 항균력"), GetStatFactorText(value.Antibacterial.Factor, value.Antibacterial.Complexity));
			return;
		}
		if (value.Comfort.Factor != 0)
		{
			KeyValueLabel infoLabel2 = GetInfoLabel(OnClickComfortStats);
			infoLabel2.Set(string.Format("{0}[icon=img_loading_unknown_question2]", T._("안락함")), value.Comfort.Factor.ToString());
		}
		if (value.Antibacterial.Factor != 0)
		{
			KeyValueLabel infoLabel3 = GetInfoLabel(OnClickAntibacterialStats);
			infoLabel3.Set(string.Format("{0}[icon=img_loading_unknown_question2]", T._("항균력")), value.Antibacterial.Factor.ToString());
		}
	}

	private void FillTags()
	{
		List<TagData> tags = _artifact.Tags;
		if (_tagsViewer.Set(tags))
		{
			AddSeparator();
			_tagsViewerLinker.gameObject.SetActive(value: true);
			_gradeViewer.gameObject.SetActive(value: true);
			_gradeViewer.Set(tags, 0f, upward: false, 0);
			_scrollView.Widgets.Add(_tagsViewerLinker.GetComponent<UIWidget>());
		}
		else
		{
			_tagsViewerLinker.gameObject.SetActive(value: false);
			_gradeViewer.gameObject.SetActive(value: false);
		}
	}

	private void FillWarehouse()
	{
		if (!Interactions.TryGetValue(Interaction.UseWarehouse, out var value) || value.Disabled)
		{
			return;
		}
		Durango.Logic.Item.Inventory trackingInventory = GameSystem<InventorySystem>.Instance().TrackingInventory;
		if (trackingInventory.OwnerId != _artifact.EntityId)
		{
			GameSystem<InventorySystem>.Instance().SetWarehouseInventory(_artifact);
			return;
		}
		using Reusable<List<string>> reusable = ReusableList<string>.Pop();
		foreach (KeyValuePair<string, int> category in trackingInventory.Categories)
		{
			reusable.Value.Add($"{category.Key} <weak>({category.Value})</weak>");
		}
		if (reusable.Value.Count > 0)
		{
			KeyValueLabel infoLabel = GetInfoLabel();
			infoLabel.Set(T._("{0:l:{}|, }", reusable.Value), null);
		}
	}

	private void FillWarpAccelerator()
	{
		if (!_artifact.ArtifactState.Warpaccelerator.HasValue)
		{
			return;
		}
		Messages.WarpAccelerator value = _artifact.ArtifactState.Warpaccelerator.Value;
		double? deactiveUntil = null;
		switch (value.Status)
		{
		case AcceleratorStatus.End:
			if (value.StatusUntil.HasValue)
			{
				deactiveUntil = value.StatusUntil.Value + (double)Yaml.Util.Singleton<Constants>.Instance.WarpAccelerator.InactivateTime;
			}
			break;
		case AcceleratorStatus.RiftInactivated:
			deactiveUntil = value.StatusUntil;
			break;
		}
		if (deactiveUntil.HasValue)
		{
			AddTextLabel(new SyncString(delegate(out string text, out float period)
			{
				SyncString.UpdateRemainTimeMsg(deactiveUntil.Value, T._("<em>{0}</em> 후 워프 가속기 설치 가능"), out text, out period, string.Empty);
			}), new Point2(40, 20));
		}
	}

	private void FillEffector()
	{
		if (_artifact.ArtifactState.Effector.HasValue)
		{
			Effector value = _artifact.ArtifactState.Effector.Value;
			KeyValueLabel infoLabel = GetInfoLabel();
			infoLabel.Set(T._("남은 사용 횟수"), T._("{0}회", value.RemainCount));
		}
	}

	private void FillOccupiedWarning()
	{
		if (_artifact.BuildState == BuildingState.Occupied)
		{
			AddTextLabel(T._("[icon=icon_make_alert] 기간 내에 건설하지 않으면 재료가 모두 사라집니다."), new Point2(40, 20));
		}
	}

	private void FillPostprocess()
	{
		ArtifactState artifactState = _artifact.ArtifactState;
		if (!artifactState.Postprocess.HasValue || artifactState.Postprocess.Value.MaxHelperCount <= 0)
		{
			return;
		}
		Postprocess value = artifactState.Postprocess.Value;
		string text = T._("마무리 참여 인원");
		string text2 = ((value.Helpers.Length < value.MaxHelperCount) ? T._("{0} / {1}명", value.Helpers.Length, value.MaxHelperCount) : T._("[9E0B0F]{0} / {1}명[-]", value.Helpers.Length, value.MaxHelperCount));
		AddSeparator();
		AddSpace(18);
		GetRawInfoLabel().Set(text, text2);
		AddSpace(8);
		if (KUtility.GetSize(value.Helpers) > 0)
		{
			KeyValueLabel lb = GetRawInfoLabel();
			lb.Set(null, null);
			Durango.Utils.Singleton<PlayerInfoManager>.Instance().RequestPlayerInfos(value.Helpers, delegate(Durango.Player.PlayerInfo[] playerInfos)
			{
				using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
				StringBuilder value2 = reusable.Value;
				for (int i = 0; i < playerInfos.Length; i++)
				{
					value2.AppendLine((!playerInfos[i].Valid) ? T._("알수없음") : playerInfos[i].Name);
				}
				lb.Set(null, value2.ToString().Trim());
				if (!_isFillData)
				{
					lb.UpdateLayout(lb.Widget.width);
					if (this.LayoutUpdated != null)
					{
						this.LayoutUpdated(obj: false);
					}
				}
			});
		}
		AddSpace(18);
	}

	private void FillFarming()
	{
		ArtifactState artifactState = _artifact.ArtifactState;
		Farming? farming = artifactState.Farming;
		if (!farming.HasValue)
		{
			return;
		}
		Farming value = artifactState.Farming.Value;
		AddSeparator();
		AddSpace(18);
		GetRawInfoLabel().Set(T._("작물"), value.PlantName);
		AddSpace(8);
		double num = value.GrowsUntil - Connections.Frontend.GetPredictedServerTime();
		string text;
		if (num > 0.0)
		{
			text = TimedeltaFormatter.Format(num, 2, "min");
			if (string.IsNullOrEmpty(text))
			{
				text = T._("곧 수확 가능");
			}
		}
		else
		{
			text = T._("수확 가능");
		}
		GetRawInfoLabel().Set(T._("수확까지"), text);
		AddSpace(8);
		GetRawInfoLabel().Set(T._("필요 물의 양"), (!(value.Water.x < value.Water.y)) ? T._("충분") : $"{value.Water.y - value.Water.x:F1}");
		AddSpace(8);
		GetRawInfoLabel().Set(T._("기후 적합성"), LocalizeUtil.Get(value.BiomeFitness));
		AddSpace(8);
		GetRawInfoLabel().Set(T._("비옥도"), $"{value.FertilizedRatio:P0}");
		AddSpace(8);
		GetRawInfoLabel().Set(T._("퇴비"), Mathf.FloorToInt(value.FertilizerAmount).ToString());
		AddSpace(8);
		GetRawInfoLabel().Set(T._("퇴비 효과"), (!string.IsNullOrEmpty(value.AppliedCropBooster)) ? TagData.GetTagNameWithLevel(value.AppliedCropBooster, value.BoosterLevel) : T._("없음"));
		AddSpace(18);
	}

	private void FillHome()
	{
		ArtifactState artifactState = _artifact.ArtifactState;
		Home? home = artifactState.Home;
		if (!home.HasValue)
		{
			return;
		}
		Home value = artifactState.Home.Value;
		string text = T._("수용 인원");
		string text2 = ((value.ResidentEntityIds.Length < value.Capacity) ? $"{value.ResidentEntityIds.Length} / {value.Capacity}" : $"[9E0B0F]{value.ResidentEntityIds.Length} / {value.Capacity}[-]");
		AddSeparator();
		AddSpace(18);
		GetRawInfoLabel().Set(text, text2);
		if (KUtility.GetSize(value.ResidentEntityIds) > 0)
		{
			AddSpace(8);
			KeyValueLabel lb = GetRawInfoLabel();
			lb.Set(null, null);
			Durango.Utils.Singleton<PlayerInfoManager>.Instance().RequestPlayerInfos(value.ResidentEntityIds, delegate(Durango.Player.PlayerInfo[] playerInfos)
			{
				using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
				StringBuilder value2 = reusable.Value;
				for (int i = 0; i < playerInfos.Length; i++)
				{
					value2.AppendLine((!playerInfos[i].Valid) ? T._("알수없음") : playerInfos[i].Name);
				}
				lb.Set(null, value2.ToString().Trim());
				if (!_isFillData)
				{
					lb.UpdateLayout(lb.Widget.width);
					if (this.LayoutUpdated != null)
					{
						this.LayoutUpdated(obj: false);
					}
				}
			});
		}
		AddSpace(18);
	}

	private void FillCrack()
	{
		ArtifactState artifactState = _artifact.ArtifactState;
		Messages.Crack? crack = artifactState.Crack;
		if (!crack.HasValue)
		{
			return;
		}
		Messages.Crack value = artifactState.Crack.Value;
		double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
		double? activatedSince = value.ActivatedSince;
		if (activatedSince.HasValue && !(value.ActivatedSince.Value > bufferedServerTime))
		{
			double? activatedUntil = value.ActivatedUntil;
			if (!activatedUntil.HasValue || !(activatedUntil.GetValueOrDefault() <= bufferedServerTime))
			{
				return;
			}
		}
		if (value.PotentialBiocoms == null || value.PotentialBiocoms.Length <= 0)
		{
			return;
		}
		KeyValueLabel infoLabel = GetInfoLabel();
		infoLabel.Set(T._("[icon=icon_map_poi_crack] 워프 가능한 군락"), null);
		infoLabel.TopBottomPaddingRatio = 0.7f;
		infoLabel = GetInfoLabel();
		using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
		{
			StringBuilder value2 = reusable.Value;
			value2.Append("[EAEAEA]");
			string[] potentialBiocoms = value.PotentialBiocoms;
			foreach (string value3 in potentialBiocoms)
			{
				value2.AppendLine(value3);
			}
			infoLabel.Set(value2.ToString().TrimEnd(), null);
		}
		infoLabel.TopBottomPaddingRatio = 0.7f;
	}

	private void FillStoneCrack()
	{
	}

	private void FillInventory()
	{
		ArtifactState artifactState = _artifact.ArtifactState;
		InventoryState? inventory = artifactState.Inventory;
		if (!inventory.HasValue)
		{
			return;
		}
		InventoryState value = artifactState.Inventory.Value;
		if (KUtility.GetSize(value.StorableTags) > 0)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < value.StorableTags.Length; i++)
			{
				string text = value.StorableTags[i];
				if (SingletonDict<string, Yaml.Tag>.TryGetValue(text, out var value2))
				{
					list.Add(value2.Name);
				}
				else if (Debug.isDebugBuild)
				{
					list.Add(text);
				}
			}
			if (list.Count > 0)
			{
				KeyValueLabel infoLabel = GetInfoLabel();
				infoLabel.Set(T._("보관 물품 종류"), T._("{0:l:{}|, }", list));
			}
		}
		if (KUtility.GetSize(value.UnstorableTags) > 0)
		{
			List<string> list2 = new List<string>();
			for (int j = 0; j < value.UnstorableTags.Length; j++)
			{
				string text2 = value.UnstorableTags[j];
				if (SingletonDict<string, Yaml.Tag>.TryGetValue(text2, out var value3))
				{
					list2.Add(value3.Name);
				}
				else if (Debug.isDebugBuild)
				{
					list2.Add(text2);
				}
			}
			if (list2.Count > 0)
			{
				KeyValueLabel infoLabel2 = GetInfoLabel();
				infoLabel2.Set(T._("보관 불가 품목"), T._("{0:l:{}|, }", list2));
			}
		}
		if (value.ReduceDurabilityVelocity > 0f)
		{
			KeyValueLabel infoLabel3 = GetInfoLabel();
			infoLabel3.Set(T._("내구도 감소 완화 정도"), $"{value.ReduceDurabilityVelocity:p0}");
		}
	}

	private void FillLandowner()
	{
		Landowner artifactComponent = _artifact.GetArtifactComponent<Landowner>();
		if (artifactComponent == null)
		{
			return;
		}
		EstateInfo estateInfo = _artifact.GetEstateInfo();
		if (estateInfo != null && estateInfo.License.IsProtected())
		{
			double at = estateInfo.License.ProtectedUntil.Value;
			KeyValueLabel infoLabel = GetInfoLabel();
			infoLabel.Set(T._("보호기간 종료까지 남은 시간"), new SyncString(delegate(out string text, out float period)
			{
				double num = at - Connections.Frontend.GetPredictedServerTime();
				text = TimedeltaFormatter.Format(num);
				int num2 = TimedeltaFormatter.CurrentMinUnit();
				period = (float)(num % (double)num2);
			}));
			infoLabel = GetInfoLabel();
			infoLabel.Set(T._("보호 기간이 종료되면 화물 워프홀은 동작을 정지합니다. 전쟁에서 일정 기간 방어에 성공하면 다시 보호 기간이 됩니다."), null);
		}
	}

	private void FillDefensive()
	{
		if (_artifact.ArtifactState.Defensive.HasValue)
		{
			DefensiveState value = _artifact.ArtifactState.Defensive.Value;
			AddSeparator();
			AddSpace(18);
			GetRawInfoLabel().Set(T._("공격력"), value.Atk.ToString());
			AddSpace(8);
			GetRawInfoLabel().Set(T._("공격 주기"), T._("{0}초", $"{value.LoadTime:F1}"));
			AddSpace(8);
			GetRawInfoLabel().Set(T._("공격 사거리"), $"{value.AtkRange:F0}");
			AddSpace(18);
		}
	}

	private void FillSprinklable()
	{
		if (_artifact.ArtifactState.Sprinkler.HasValue)
		{
			AddSeparator();
			AddSpace(18);
			GetRawInfoLabel().Set(T._("사용 횟수"), $"{_artifact.ArtifactState.Sprinkler.Value.ChargeCount}/{_artifact.ArtifactState.Sprinkler.Value.ChargeCapacity}");
			AddSpace(8);
			string text = TimedeltaFormatter.Format(_artifact.ArtifactState.Sprinkler.Value.ChargeDelay);
			GetRawInfoLabel().Set(T._("충전 주기"), text);
			AddSpace(8);
			GetRawInfoLabel().Set(T._("급수량"), _artifact.ArtifactState.Sprinkler.Value.SprinkleWaterCount.ToString());
			if (_artifact.ArtifactState.Sprinkler.Value.FertilizerInventory > 0)
			{
				AddSpace(8);
				GetRawInfoLabel().Set(T._("비료"), $"{_artifact.ArtifactState.Sprinkler.Value.FertilizerCount}/{_artifact.ArtifactState.Sprinkler.Value.FertilizerInventory}");
			}
			AddSpace(18);
		}
	}

	private void FillArtifactRights()
	{
		EstateInfo estateInfo = _artifact.GetEstateInfo();
		if (estateInfo == null || !Access.HasValue)
		{
			_rightsWidget.gameObject.SetActive(value: false);
			_manageButton.SetActive(value: false);
			return;
		}
		switch (estateInfo.License.Type)
		{
		default:
			_manageButton.SetActive(value: false);
			_rightsWidget.gameObject.SetActive(value: false);
			break;
		case OwnerType.Player:
		case OwnerType.ClanEstate:
		case OwnerType.ClanWarphole:
		case OwnerType.PersonalPlayer:
			AddSeparator();
			_manageButton.SetActive(Access.Value.Others && EstateSystem.IsAdmin(estateInfo));
			_rightsWidget.Set(Access.Value, estateInfo, _artifact.Blueprint != null && _artifact.Blueprint.HasComponent("Secured"));
			_scrollView.Widgets.Add(_rightsWidget);
			_rightsWidget.gameObject.SetActive(value: true);
			break;
		}
	}

	private void FillCage()
	{
		if (_artifact.ArtifactState.DomesticCage.HasValue)
		{
			DomesticCage value = _artifact.ArtifactState.DomesticCage.Value;
			AddCageInfo(value.Size - value.RemainSize, value.Size);
		}
		if (_artifact.ArtifactState.Cage is Messages.Cage)
		{
			Messages.Cage cage = (Messages.Cage)_artifact.ArtifactState.Cage;
			AddCageInfo(cage.Size - cage.RemainSize, cage.Size);
		}
		if (_artifact.ArtifactState.Cage is GrowCage)
		{
			GrowCage growCage = (GrowCage)_artifact.ArtifactState.Cage;
			AddCageInfo(growCage.Size - growCage.RemainSize, growCage.Size);
		}
	}

	private void AddCageInfo(int size, int capacity)
	{
		AddSeparator();
		AddSpace(18);
		GetRawInfoLabel().Set(T._("축사 공간"), $"<weak>{size}</weak> / {capacity}");
		AddSpace(18);
	}

	private void FillCatapult()
	{
		if (_artifact.ArtifactState.Catapult.HasValue)
		{
			CatapultState value = _artifact.ArtifactState.Catapult.Value;
			KeyValueLabel infoLabel = GetInfoLabel();
			infoLabel.Set(T._("투척물"), $"{value.RemainedProjectilesSize} / {value.MaxProjectilesSize}");
		}
	}

	private void FillInteriorMood()
	{
		if (_artifact.Blueprint != null && _artifact.Blueprint.InteriorSetEffect)
		{
			int statFactor = (_artifact.ArtifactState.Stats.HasValue ? _artifact.ArtifactState.Stats.Value.Comfort.Factor : 0);
			_interiorMood.gameObject.SetActive(value: true);
			bool expand = _interiorMood.Set(_artifact.ArtifactState.InteriorMood, statFactor, _artifact.BlueprintId);
			_interiorMood.SetExpand(expand, instant: true);
			_scrollView.Widgets.Add(_interiorMood);
		}
		else
		{
			_interiorMood.gameObject.SetActive(value: false);
		}
	}

	private void FillInteriorSet()
	{
		if (_artifact.Blueprint != null && _artifact.Blueprint.InteriorSetEffect)
		{
			int statFactor = (_artifact.ArtifactState.Stats.HasValue ? _artifact.ArtifactState.Stats.Value.Antibacterial.Factor : 0);
			_interiorSet.gameObject.SetActive(value: true);
			bool expand = _interiorSet.Set(_artifact.ArtifactState.InteriorSet, statFactor, _artifact.BlueprintId);
			_interiorSet.SetExpand(expand, instant: true);
			_scrollView.Widgets.Add(_interiorSet);
		}
		else
		{
			_interiorSet.gameObject.SetActive(value: false);
		}
	}

	private SyncString GetSubTitle()
	{
		if (_artifact == null)
		{
			return string.Empty;
		}
		ArtifactState artifactState = _artifact.ArtifactState;
		Messages.Crack? crack = artifactState.Crack;
		if (crack.HasValue)
		{
			Messages.Crack value = artifactState.Crack.Value;
			double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
			double? activatedSince = value.ActivatedSince;
			if (activatedSince.HasValue && !(value.ActivatedSince.Value > bufferedServerTime))
			{
				double? activatedUntil = value.ActivatedUntil;
				if (!activatedUntil.HasValue || !(activatedUntil.GetValueOrDefault() <= bufferedServerTime))
				{
					goto IL_00dd;
				}
			}
			return string.Format("{0}\n[icon={1}] <em>{2}</em>", T._("활성화에 필요한 자원유도석"), Yaml.Util.Singleton<Constants>.Instance.Crack.VoucherId, value.RequiredInvestment);
		}
		goto IL_00dd;
		IL_00dd:
		return new SyncString(delegate(out string text, out float period)
		{
			FillDurability(out text, out period);
		});
	}

	private void FillDurability(out string text, out float period)
	{
		text = string.Empty;
		period = 0f;
		if (_artifact == null)
		{
			return;
		}
		ArtifactState artifactState = _artifact.ArtifactState;
		if (artifactState.Durability == null)
		{
			return;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double num = artifactState.Durability.Get(predictedServerTime);
		double num2 = artifactState.Durability.Max(predictedServerTime);
		string text2 = string.Empty;
		double predictedServerTime2 = Connections.Frontend.GetPredictedServerTime();
		Pair<double, double>? repairement = artifactState.Repairement;
		if (repairement.HasValue && artifactState.Repairement.Value.Item2 > predictedServerTime2)
		{
			text = $"[icon=icon_skill_time] - /{num2:0.#}";
			text2 = T._("수리 중");
			text = $"{text}\n{text2}";
			period = (float)(artifactState.Repairement.Value.Item2 - predictedServerTime2);
			return;
		}
		double num3 = artifactState.Durability.When(0f, predictedServerTime);
		double num4 = num3 - predictedServerTime;
		if (num3 > 0.0 && num4 <= 0.0)
		{
			text2 = T._("파괴됨");
		}
		else if (artifactState.BuildingState == BuildingState.Remodeling)
		{
			text2 = T._("개조 중");
		}
		else if (num3 > 0.0)
		{
			switch (artifactState.BuildingState)
			{
			case BuildingState.Occupied:
				text2 = T._("{0} 내 완성 필요", TimedeltaFormatter.Format(num4, 2, "min"));
				break;
			case BuildingState.Built:
			case BuildingState.Completed:
				text2 = ((!(num4 > 3600.0) && !_artifact.Blueprint.TimeLimited) ? T._("곧 파괴됨") : T._("{0} 사용 가능", TimedeltaFormatter.Format(num4, 2, "min")));
				break;
			}
		}
		if (_artifact.Blueprint.TimeLimited)
		{
			text = $"[icon=icon_make_alert] {text2}";
		}
		else
		{
			bool flag = ((artifactState.BuildingState != BuildingState.Completed) ? (num3 > 0.0 && num4 < 86400.0) : (_artifact.Condition >= Shared.Building.Condition.Worn));
			text = string.Format((!flag) ? "[icon=icon_skill_time] {0:0.#}/{1:0.#}" : "<alert>[icon=icon_skill_time] {0:0.#}</alert>/{1:0.#}", num, num2);
			if (!string.IsNullOrEmpty(text2))
			{
				text = string.Format((!flag) ? "{0}\n{1}" : "{0}\n<alert>{1}</alert>", text, text2);
			}
		}
		period = ((!(num4 <= 0.0)) ? ((float)num4 % 60f) : 0f);
	}

	private static string GetStatFactorText(int factor, int complexity)
	{
		return (!((float)Mathf.Abs(complexity) > 0f)) ? $"{factor} [icon=icon_information]" : $"<alert>{factor}</alert> [icon=icon_information]";
	}

	private void OnClickInfoLabel(GameObject obj)
	{
		int num = _textLabels.IndexOf(obj.GetComponent<KeyValueLabel>());
		if (num != -1 && _onClickTextLabels[num] != null)
		{
			_onClickTextLabels[num]();
		}
	}

	private void OnClickManageButton()
	{
		if (this.ManageButtonClicked != null)
		{
			this.ManageButtonClicked();
		}
	}

	private void OnClickComfortStats()
	{
		if (this.ArtifactStatsInfoClicked != null)
		{
			this.ArtifactStatsInfoClicked(StatsType.Comfort);
		}
	}

	private void OnClickAntibacterialStats()
	{
		if (this.ArtifactStatsInfoClicked != null)
		{
			this.ArtifactStatsInfoClicked(StatsType.Antibacterial);
		}
	}

	private void Interior_OnExpandChanged(ItemContextBase comp)
	{
		bool flag = !comp.IsExpanded;
		comp.SetExpand(flag, instant: false);
		if (flag)
		{
			if (this.LayoutUpdated != null)
			{
				this.LayoutUpdated(obj: true);
			}
		}
		else
		{
			_scrollView.UpdateLayout(instant: false);
		}
	}
}
