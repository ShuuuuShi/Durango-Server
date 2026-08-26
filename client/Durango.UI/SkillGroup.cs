using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.LearningGuide;
using Durango.Logic.Notification;
using Durango.Logic.Skill;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Shared.Skill;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Skill")]
public class SkillGroup : UIBase, INotificationable
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private KWidgetScrollView _mainScroll;

	[SerializeField]
	private SkillCategoryWidget _skillCategory;

	[SerializeField]
	private SkillCategoryInfoWidget _skillCategoryInfo;

	[SerializeField]
	private SkillListWidget _skillList;

	[SerializeField]
	private SkillInfoWidget _skillInfo;

	[SerializeField]
	private SelectableWidget _guideButton;

	[SerializeField]
	private GameObject _guideNewTag;

	[SerializeField]
	private UILabel _guideMainInfoLabel;

	[SerializeField]
	private UILabel _guideSubInfoLabel;

	private float _scrollOffset;

	private int _scrollIndex;

	private readonly List<Durango.Logic.Skill.Category> _readyToResearchCategories = new List<Durango.Logic.Skill.Category>();

	private DelayedFunction _delayReadyToResearchCategoriesFunc;

	private Countable _notification;

	public Transform LearningGuideButton => _guideButton.transform;

	public Shared.Skill.Category SelectedCategory => _skillCategory.SelectedCategory;

	public Node SelectedSkillNode { get; private set; }

	public Group SelectedSkillGroup { get; private set; }

	public Notification Notification
	{
		get
		{
			if (_notification == null)
			{
				_notification = new Countable(Durango.Logic.Notification.Type.Normal);
			}
			return _notification;
		}
	}

	public event Action<Shared.Skill.Category> SkillListPageShowed;

	private void Awake()
	{
		_openCloseSound = UISound.GroupType.Skill;
		int i = 0;
		for (int count = _mainScroll.Widgets.Count; i < count; i++)
		{
			_mainScroll.Widgets[i].gameObject.SetActive(value: true);
		}
		base.TryClose();
	}

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("스킬"));
		SkillCategoryWidget skillCategory = _skillCategory;
		skillCategory.CategorySelected = (Action<Shared.Skill.Category>)Delegate.Combine(skillCategory.CategorySelected, new Action<Shared.Skill.Category>(OnSelectCategory));
		_skillCategoryInfo.InfoButtonClicked += OnClickInfoButton;
		_skillList.SkillSelected += OnSelectSkill;
		_skillList.SkillGroupSelected += OnSelectSkillGroup;
		_skillInfo.OnLearnSkill += OnLearnSkill;
		_skillInfo.OnUntrainSkill += OnUntrainSkill;
		_skillInfo.SkillSelected += OnSkillSelected;
		SelectableWidget guideButton = _guideButton;
		guideButton.Clicked = (Action)Delegate.Combine(guideButton.Clicked, (Action)delegate
		{
			LearningGuideGroup learningGuideGroup = UIManager.FindScript<LearningGuideGroup>();
			bool softOpen = base.SoftOpen;
			base.SoftOpen = false;
			learningGuideGroup.Open();
			base.SoftOpen = softOpen;
		});
		base.OnOpenSucceed += OnOpened;
		_delayReadyToResearchCategoriesFunc = new DelayedFunction(DelayedReadyToCategoryResearch, new WaitForSeconds(0.5f));
		GameSystem<SkillSystem>.Instance().CategoryResearchReady += OnReadyToCategoryResearch;
		GameSystem<SkillSystem>.Instance().SkillListUpdated += OnSkillListUpdate;
		GameSystem<LearningGuideSystem>.Instance().AchievedInfoUpdated += LearningGuideSystem_AchievedInfoUpdated;
		GameSystem<LearningGuideSystem>.Instance().TargetAdviceUpdated += LearningGuideSystem_TargetAdviceUpdated;
		GameSystem<MenuSystem>.Instance().EnableMenuUpdated += MenuSystem_EnableMenuUpdated;
	}

	private void Update()
	{
		if (base.IsOpened)
		{
			float currentOffset = _mainScroll.CurrentOffset;
			if (_scrollOffset != currentOffset)
			{
				_scrollOffset = currentOffset;
				RefreshInfoOffset();
			}
		}
	}

	private void OnOpened()
	{
		_skillInfo.gameObject.SetActive(value: true);
		// ตอน Init() (OnAwake) Region/Template ยังไม่พร้อม ⇒ รายการหมวดสกิลอาจไม่ถูกสร้าง
		// เปิด UI แล้วค่อยสร้างใหม่ตรงนี้ (Region มาแล้วแน่นอน) — กันแท็บสกิลว่าง/เหลือช่องเดียว
		_skillCategory.Rebuild();
		_skillCategory.Unselect();
		_skillCategoryInfo.Set(Shared.Skill.Category.Invalid);
		_skillCategoryInfo.ShowButtonContainer(show: true);
		_scrollOffset = -1f;
		_scrollIndex = -1;
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		Point2 point = new Point2(_mainScroll.ViewSize);
		point.x -= 20;
		point.y -= 20;
		for (int i = 0; i < _mainScroll.GetNodeCount(); i++)
		{
			UIWidget node = _mainScroll.GetNode(i);
			node.SetDimensions(point.x, point.y);
			RectLayoutComponent component = node.GetComponent<RectLayoutComponent>();
			if (component != null)
			{
				component.UpdateLayout();
			}
		}
		_mainScroll.UpdateLayout();
		UIUtility.UpdateAnchors(_mainScroll.transform);
	}

	protected override bool TryClose()
	{
		if (_mainScroll.GetGoalNodeIndex() > 0 && !UIManager.FindScript<LearningGuideGroup>().IsOpened)
		{
			_skillCategory.UpdateData();
			HasBack.Value = false;
			_mainScroll.MoveToNode(0, instant: false, restrictWithinPanel: false);
			SelectedSkillNode = null;
			SelectedSkillGroup = null;
			return false;
		}
		return base.TryClose();
	}

	public override bool Open()
	{
		bool result = base.Open();
		if (!base.IsOpened)
		{
			HasBack.Value = false;
			_mainScroll.MoveToNode(0, instant: true, restrictWithinPanel: false);
		}
		return result;
	}

	public void Open([CanBeNull] Node skillNode)
	{
		base.Open();
		if (skillNode != null)
		{
			_skillCategory.SelectCategory(skillNode.Category);
			ShowSkillListPage(skillNode.Category, skillNode.Group, instant: true);
			_skillInfo.SelectSkill(skillNode.Id, skillNode.Sub, skillNode.Level);
		}
	}

	public void Open(Shared.Skill.Category category)
	{
		bool instant = !base.IsOpened || !base.Visible;
		base.Open();
		_skillCategory.SelectCategory(category);
		HasBack.Value = false;
		_mainScroll.MoveToNode(0, instant, restrictWithinPanel: false);
	}

	public void ScrollToSkill(string id, string subId, int level)
	{
		_skillInfo.ScrollTo(id, subId, level);
	}

	public Transform GetCategoryTransform(Shared.Skill.Category category)
	{
		SkillCategoryNode categoryNode = _skillCategory.GetCategoryNode(category);
		return (!(categoryNode != null)) ? null : categoryNode.transform;
	}

	public Transform GetCategoryInfoButtonTransform()
	{
		return _skillCategoryInfo.DetailButton.transform;
	}

	public Transform GetSkillListNodeTransform(string subCatergory)
	{
		SkillListNode skillListNode = _skillList.GetSkillListNode(subCatergory);
		return (!(skillListNode != null)) ? null : skillListNode.transform;
	}

	public Transform GetSkillNodeTransform(string id, string subId, int level)
	{
		SkillTreeItem skillTreeItem = _skillInfo.FindSkill(id, subId, level);
		return (!(skillTreeItem != null)) ? null : skillTreeItem.transform;
	}

	public Transform GetLearnButtonTransform()
	{
		return _skillInfo.LearnButton.transform;
	}

	private void OnSelectCategory(Shared.Skill.Category cat)
	{
		_skillCategoryInfo.Set(cat);
	}

	private void OnClickInfoButton()
	{
		ShowSkillListPage(_skillCategory.SelectedCategory, instant: false);
	}

	private void ShowSkillListPage(Shared.Skill.Category category, bool instant)
	{
		ShowSkillListPage(category, null, instant);
	}

	private void ShowSkillListPage(Shared.Skill.Category category, string group, bool instant)
	{
		if (category != Shared.Skill.Category.Invalid)
		{
			_skillList.Set(category, group);
			_skillCategoryInfo.ShowButtonContainer(show: false);
			HasBack.Value = true;
			_mainScroll.MoveToNode(1, instant, restrictWithinPanel: false);
			_titleWidget.Object.SetTitle(Util.CategoryLocalizeName(category));
			if (this.SkillListPageShowed != null)
			{
				this.SkillListPageShowed(category);
			}
		}
	}

	private void OnLearnSkill(Node skill)
	{
		_skillInfo.LearnAndSelectNextSkill(skill);
	}

	private static string GetRemainedTime(string includingCommodityId)
	{
		DateTime dateTime = Times.UnixTimeToDateTimeUtc(Connections.Frontend.GetPredictedServerTime());
		DateTime dateTime2;
		if (!string.IsNullOrEmpty(includingCommodityId))
		{
			StatusEffect statusEffectFromCommodity = GameSystem<StatusEffectSystem>.Instance().GetStatusEffectFromCommodity(includingCommodityId);
			if (statusEffectFromCommodity == null || !statusEffectFromCommodity.DailyContents.HasValue)
			{
				return string.Empty;
			}
			dateTime2 = Times.UnixTimeToDateTimeUtc(statusEffectFromCommodity.DailyContents.Value.ValidUntil);
		}
		else
		{
			dateTime2 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 15, 0, 0, DateTimeKind.Utc).AddDays(1.0);
		}
		TimeSpan timeSpan = dateTime2 - dateTime;
		timeSpan -= TimeSpan.FromDays(timeSpan.Days);
		return T._("{0} 후 초기화", TimedeltaFormatter.Format(timeSpan.TotalSeconds, 2, "min"));
	}

	private void OnUntrainSkill(Node skill)
	{
		string voucherId = Yaml.Util.Singleton<CostsYaml>.Instance.SkillUntrainVoucher.Vouchers[0].VoucherId;
		int voucherCount = InventorySystem.Wallet.GetVoucherCount(voucherId);
		string hiddenVoucherId = Yaml.Util.Singleton<CostsYaml>.Instance.SkillUntrainVoucher.Vouchers[1].VoucherId;
		string includingCommodityId = Yaml.Util.Singleton<CostsYaml>.Instance.SkillUntrainVoucher.Vouchers[1].IncludingCommodityId;
		int voucherCount2 = InventorySystem.Wallet.GetVoucherCount(hiddenVoucherId);
		bool flag = GameSystem<StatusEffectSystem>.Instance().GetStatusEffectFromCommodity(includingCommodityId) != null;
		int untrainedCount = GameSystem<SkillSystem>.Instance().UntrainedCount;
		int freeCount = Yaml.Util.Singleton<Constants>.Instance.SkillUntrain.FreeCount;
		int num = Math.Min(untrainedCount, freeCount);
		int num2 = Yaml.Util.Singleton<Constants>.Instance.SkillUntrain.MaxCount - Yaml.Util.Singleton<Constants>.Instance.SkillUntrain.FreeCount;
		int num3 = Math.Max(untrainedCount - freeCount, 0);
		bool flag2 = num < freeCount;
		MessageBox messageBox = UIManager.MessageBox;
		string text = T._("<em>{0}</em>{0:-을} 습득 취소 하고 [icon=icon_sp]{1:을} 돌려받습니다.", skill.Name, skill.SkillPoints);
		string subText = ((!flag2) ? T._("<alert_icon/> 오늘 무료로 습득 취소 가능한 횟수를 모두 사용했습니다.") : T._("<alert_icon/> 무료 습득 취소 횟수를 먼저 사용합니다."));
		string text2 = $"[preset=dialog_round_box?{GetRemainedTime(string.Empty)}]";
		string text3 = T._("<em>{0}</em>/{1} 회  {2}", num, freeCount, text2);
		messageBox.AddKeyValueInfo(T._("무료 습득 취소"), text3);
		if (voucherCount2 > 0 || flag)
		{
			text2 = $"[preset=dialog_round_box?{GetRemainedTime(includingCommodityId)}]";
			text3 = T._("<em>{0}</em>회 사용 가능  {1}", voucherCount2, text2);
			messageBox.AddKeyValueInfo(T._("무료 습득 취소 - 패키지"), text3);
		}
		if (voucherCount > 0)
		{
			text3 = T._("<em>{0}</em>회 사용 가능", voucherCount);
			messageBox.AddKeyValueInfo(T._("스킬 습득 취소 이용권"), text3);
		}
		text2 = $"[preset=dialog_round_box?{GetRemainedTime(includingCommodityId)}]";
		text3 = T._("<em>{0}</em>/{1} 회  {2}", num3, num2, text2);
		messageBox.AddKeyValueInfo(T._("워프젬 습득 취소"), text3);
		if (flag2)
		{
			messageBox.Show(text, subText, delegate(bool ok)
			{
				if (ok)
				{
					_skillInfo.UntrainAndSelectPreviousSkill(skill);
				}
			});
			return;
		}
		if (voucherCount2 > 0)
		{
			string textFormat = T._("<em>{0}</em>의 효과로 무료 스킬 습득 취소 기회가 {1}회 증가합니다.  <bar/>  <em>{2} 남음</em>");
			string packageEffectText = Vouchers.GetPackageEffectText(hiddenVoucherId, includingCommodityId, textFormat);
			messageBox.SetLowerText(packageEffectText);
			messageBox.Show(text, subText, delegate(bool ok)
			{
				if (ok)
				{
					_skillInfo.UntrainAndSelectPreviousSkill(skill, hiddenVoucherId);
				}
			});
			return;
		}
		if (voucherCount > 0)
		{
			SkillUntrainCost skillUntrainCost = Yaml.Util.Singleton<CostsYaml>.Instance.SkillUntrainVoucher.Vouchers[0];
			messageBox.ShowPayConfirmWithVoucher(skillUntrainCost.VoucherAmount, voucherId, text, subText, delegate(bool ok)
			{
				if (ok)
				{
					_skillInfo.UntrainAndSelectPreviousSkill(skill, voucherId);
				}
			});
			return;
		}
		messageBox.ShowPayConfirm(Yaml.Util.Singleton<CostsYaml>.Instance.SkillUntrainVoucher.Amount, Yaml.Util.Singleton<CostsYaml>.Instance.SkillUntrainVoucher.Currency, text, subText, delegate(bool ok)
		{
			if (ok)
			{
				_skillInfo.UntrainAndSelectPreviousSkill(skill);
			}
		});
	}

	private void OnSkillSelected(Node skill)
	{
		SelectedSkillNode = skill;
	}

	private void OnSelectSkill(Bundle skill)
	{
		_skillInfo.Show((skill == null) ? null : new Bundle[1] { skill });
	}

	private void OnSelectSkillGroup([NotNull] Group group)
	{
		SelectedSkillGroup = group;
		_skillInfo.Show(group.Skills);
	}

	private void RefreshInfoOffset()
	{
		int goalNodeIndex = _mainScroll.GetGoalNodeIndex();
		if (goalNodeIndex != _scrollIndex)
		{
			_scrollIndex = goalNodeIndex;
			_skillCategoryInfo.ShowButtonContainer(goalNodeIndex == 0);
			if (goalNodeIndex == 0)
			{
				_titleWidget.Object.SetTitle(T._("스킬"));
			}
		}
	}

	private void OnReadyToCategoryResearch(Durango.Logic.Skill.Category cat)
	{
		_readyToResearchCategories.Add(cat);
		_delayReadyToResearchCategoriesFunc.Call(this);
	}

	private void OpenGroup()
	{
		base.Open();
	}

	private void DelayedReadyToCategoryResearch()
	{
		if (_readyToResearchCategories.Count == 0)
		{
			return;
		}
		if (_readyToResearchCategories.Count > 1)
		{
			UIManager.Alarm.ShowNotify(T._("<em>연구가능</em>한 스킬계열이 {0}개 있습니다.", _readyToResearchCategories.Count), "icon_mainhud_skill", major: true, 1.8f, OpenGroup);
		}
		else
		{
			Durango.Logic.Skill.Category cat = _readyToResearchCategories[0];
			UIManager.Alarm.ShowNotify(T._("<em>{0}</em> 숙련도를 더 올리려면 <em>연구</em>가 필요합니다.", Util.CategoryLocalizeName(cat.Type)), Util.CategoryIcon(cat.Type), major: true, 1.8f, delegate
			{
				Open(cat.Type);
			});
		}
		_readyToResearchCategories.Clear();
	}

	private void OnSkillListUpdate()
	{
		UpdateNewCheckerCount();
	}

	private void UpdateNewCheckerCount()
	{
		int num = 0;
		int level = GameSystem<StatisticsSystem>.Instance().Level;
		SkillSystem skillSystem = GameSystem<SkillSystem>.Instance();
		int skillCategoryCount = skillSystem.SkillCategoryCount;
		for (int i = 0; i < skillCategoryCount; i++)
		{
			Durango.Logic.Skill.Category skillCategory = skillSystem.GetSkillCategory(i);
			GameSystem<SkillSystem>.Instance().GetCategoryExp(skillCategory.Type, out var current, out var max);
			if (current == max && !skillCategory.IsResearching() && skillCategory.Level < level)
			{
				num++;
			}
		}
		Notification.Count = num;
	}

	private void LearningGuideSystem_AchievedInfoUpdated()
	{
		UpdateGuideInfoLabels();
	}

	private void LearningGuideSystem_TargetAdviceUpdated()
	{
		UpdateGuideInfoLabels();
	}

	private void UpdateGuideInfoLabels()
	{
		LearningGuideSystem learningGuideSystem = GameSystem<LearningGuideSystem>.Instance();
		AdviceAchievement adviceAchievement = null;
		Durango.Logic.LearningGuide.Advice targetAdvice = learningGuideSystem.TargetAdvice;
		if (targetAdvice != null)
		{
			adviceAchievement = learningGuideSystem.GetAchievementState(targetAdvice.RewardTitleId);
		}
		bool flag = adviceAchievement != null && !adviceAchievement.Complete;
		if (flag)
		{
			if (adviceAchievement.Achieved)
			{
				_guideMainInfoLabel.text = string.Format("{0} {1}", targetAdvice.Name, T._("완료"));
				_guideSubInfoLabel.text = T._("보상 수령 가능!");
				_guideSubInfoLabel.color = PresetColor.UIYellow;
			}
			else
			{
				_guideMainInfoLabel.text = string.Format("{0} {1}", targetAdvice.Name, T._("진행 중"));
				_guideSubInfoLabel.text = string.Format("{0:P0} {1}", adviceAchievement.Ratio, T._("달성"));
				_guideSubInfoLabel.color = _guideMainInfoLabel.color;
			}
			_guideSubInfoLabel.gameObject.SetActive(value: true);
			TweenAlpha component = _guideSubInfoLabel.GetComponent<TweenAlpha>();
			if (component != null)
			{
				component.enabled = adviceAchievement.Achieved;
				component.Sample(0f, isFinished: true);
			}
		}
		else
		{
			_guideMainInfoLabel.text = T._("새로운 진로 선택 가능!");
			_guideSubInfoLabel.gameObject.SetActive(value: false);
		}
		TweenAlpha component2 = _guideMainInfoLabel.GetComponent<TweenAlpha>();
		if (component2 != null)
		{
			component2.enabled = !flag;
			component2.Sample(0f, isFinished: true);
		}
		_guideNewTag.SetActive(learningGuideSystem.HasReward);
	}

	private void MenuSystem_EnableMenuUpdated()
	{
		bool active = GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.LearningGuide);
		_guideButton.gameObject.SetActive(active);
	}

	[Uri("Category")]
	private void CategoryUri(string category)
	{
		if (category.TryEnum<Shared.Skill.Category>(out var value))
		{
			Open(value);
		}
	}

	protected override void DefaultUri()
	{
		string argument = UriParser.GetArgument("id");
		string argument2 = UriParser.GetArgument("sub");
		string argument3 = UriParser.GetArgument("level");
		Bundle bundle = ((!string.IsNullOrEmpty(argument)) ? GameSystem<SkillSystem>.Instance().FindSkill(argument) : null);
		if (bundle == null)
		{
			Open();
			return;
		}
		Durango.Logic.Skill.Skill skill = ((!string.IsNullOrEmpty(argument2)) ? bundle.Get(argument2) : bundle.Base);
		if (skill == null)
		{
			Open();
			return;
		}
		int result;
		Node node = ((string.IsNullOrEmpty(argument3) || !int.TryParse(argument3, out result)) ? skill.Get() : skill.Get(result));
		if (node == null)
		{
			Open();
		}
		else
		{
			Open(node);
		}
	}
}
