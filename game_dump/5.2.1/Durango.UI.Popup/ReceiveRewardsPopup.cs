using System;
using System.Collections.Generic;
using System.Text;
using Building;
using Crafting;
using Durango.Logic;
using Durango.Logic.Encyclopedia;
using Durango.Logic.Item;
using Durango.Logic.LearningGuide;
using Durango.Logic.Shop;
using Durango.Logic.Skill;
using Durango.Logic.Statistics;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Economy;
using Shared.Faction;
using Shared.Memo;
using Shared.Voucher;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class ReceiveRewardsPopup : TooltipBase
{
	private struct Argument
	{
		public string Title;

		public List<ItemArgument> Items;

		public WarpAcceleratorInfo? WarpAccelerator;

		public string Button;

		public string Caption;

		public bool EffectOn;

		public string Sound;

		public Action Clicked;
	}

	public struct ItemArgument
	{
		public string Title;

		public string SubTitle;

		public int Amount;

		public string Icon;

		public ItemColor IconColor;

		public string IconRTable;

		public string IconGTable;

		public string IconBTable;

		public string Sup;

		public bool GoodEffect;

		public bool IsBonus;

		public KeyValuePair<string, int>? ItemPrototype;

		public string GetSubText()
		{
			if (string.IsNullOrEmpty(SubTitle) && Amount <= 0)
			{
				return null;
			}
			if (string.IsNullOrEmpty(SubTitle))
			{
				return Amount.ToString("N0", T.Culture);
			}
			if (Amount > 0)
			{
				return string.Format(T.Culture, "{0} x{1:N0}", SubTitle, Amount);
			}
			return SubTitle;
		}
	}

	[SerializeField]
	private TweenerPlayer _showTweener;

	[SerializeField]
	private UILabel _rewardTitle;

	[SerializeField]
	private WarpAcceleratorRewardWidget _warpAcceleratorRewardWidget;

	[SerializeField]
	private UIWidget _scrollWidget;

	[SerializeField]
	private KScrollView _kScrollView;

	[SerializeField]
	private UIWidget _captionWidget;

	[SerializeField]
	private UILabel _captionLabel;

	[SerializeField]
	private UIWidget _bottomWidget;

	[SerializeField]
	private SelectableButton _button;

	[SerializeField]
	private TweenerPlayer _rewardedEffect;

	[SerializeField]
	private RectLayout _layout;

	private readonly Queue<Argument> _queue = new Queue<Argument>();

	private UIBase _parent;

	private Action _clicked;

	private bool _reset = true;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void Start()
	{
		base.Start();
		_button.Clicked = Button_Clicked;
		_button.SetEffect(PresetButton.Effect.Emphasis);
		_parent = UIUtility.FindComponentInParent<UIBase>(base.gameObject);
		_parent.VisibleController.Changed += OnParentVisibleChanged;
	}

	private void OnParentVisibleChanged(bool visible)
	{
		if (visible)
		{
			ProcessQueue();
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		_showTweener.Play();
		string key = GetType().ToString();
		UIManager.FindScript<DialogueGroupBase>().SetVisible(visible: false, key);
		UIManager.FindScript<ChapterGroup>().SetVisible(visible: false, key);
	}

	protected override void OnHide()
	{
		base.OnHide();
		string key = GetType().ToString();
		UIManager.FindScript<DialogueGroupBase>().SetVisible(visible: true, key, 0.2f);
		UIManager.FindScript<ChapterGroup>().SetVisible(visible: true, key, 0.2f);
		_reset = true;
	}

	public override void Hide()
	{
		if (!ProcessQueue())
		{
			base.Hide();
		}
	}

	private void AddQueue(Argument arg)
	{
		_queue.Enqueue(arg);
		if (!base.IsVisible)
		{
			ProcessQueue();
		}
	}

	private bool ProcessQueue()
	{
		if (_parent != null && !_parent.Visible)
		{
			return true;
		}
		if (_queue.Count > 0)
		{
			Argument arg = _queue.Dequeue();
			Show(arg);
			return true;
		}
		return false;
	}

	private void Show(Argument arg)
	{
		SetTitle(arg.Title);
		SetWarpAccelerator(arg.WarpAccelerator);
		SetCaption(arg.Caption);
		SetButton(arg.Button, arg.Clicked);
		_kScrollView.Nodes.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(arg.Items); i < size; i++)
		{
			ItemArgument itemArgument = arg.Items[i];
			if (itemArgument.IsBonus)
			{
				_kScrollView.Nodes.GetNext().GetComponent<RewardItemWidget>().SetTitle(itemArgument.Title, itemArgument.GetSubText())
					.SetIcon(itemArgument.Icon, itemArgument.IconColor, itemArgument.IconRTable, itemArgument.IconGTable, itemArgument.IconBTable)
					.SetSupText(itemArgument.Sup)
					.SetBonus(isBonus: true)
					.SetGoodEffect(itemArgument.GoodEffect);
			}
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(arg.Items); j < size2; j++)
		{
			ItemArgument itemArgument2 = arg.Items[j];
			if (!itemArgument2.IsBonus)
			{
				_kScrollView.Nodes.GetNext().GetComponent<RewardItemWidget>().SetTitle(itemArgument2.Title, itemArgument2.GetSubText())
					.SetIcon(itemArgument2.Icon, itemArgument2.IconColor, itemArgument2.IconRTable, itemArgument2.IconGTable, itemArgument2.IconBTable)
					.SetSupText(itemArgument2.Sup)
					.SetBonus(isBonus: false)
					.SetGoodEffect(itemArgument2.GoodEffect);
			}
		}
		_kScrollView.Nodes.EndLoad();
		_rewardedEffect.gameObject.SetActive(arg.EffectOn);
		if (arg.EffectOn)
		{
			_rewardedEffect.Play();
		}
		if (!string.IsNullOrEmpty(arg.Sound))
		{
			SoundManager.PlayEvent(arg.Sound);
		}
		Show();
	}

	public void ShowRecipeBonusInfo(string recipeId, int? level)
	{
		BonusPrototypes[] array = SingletonDict<string, BonusPrototypes[]>.Get(recipeId);
		if (array == null || array.Length == 0)
		{
			return;
		}
		Argument arg = default(Argument);
		arg.Title = T._("보너스 획득 가능");
		arg.Items = new List<ItemArgument>();
		using (Reusable<List<BonusPrototypes>> reusable = ReusableList<BonusPrototypes>.Pop())
		{
			using Reusable<Dictionary<string, Pair<int, int>>> reusable2 = ReusableDictionary<string, Pair<int, int>>.Pop();
			using Reusable<HashSet<string>> reusable3 = ReusableHashSet<string>.Pop();
			reusable.Value.AddRange(array);
			reusable.Value.Sort((BonusPrototypes b1, BonusPrototypes b2) => b1.Rate.CompareTo(b2.Rate));
			foreach (BonusPrototypes item in reusable.Value)
			{
				if (item.Prototypes == null)
				{
					continue;
				}
				BonusPrototype[] prototypes = item.Prototypes;
				foreach (BonusPrototype bonusPrototype in prototypes)
				{
					Pair<int, int> value = ((!reusable2.Value.TryGetValue(bonusPrototype.PrototypeId, out value)) ? new Pair<int, int>(bonusPrototype.Count, bonusPrototype.Count) : new Pair<int, int>(Mathf.Min(value.Item1, bonusPrototype.Count), Mathf.Max(value.Item2, bonusPrototype.Count)));
					reusable2.Value[bonusPrototype.PrototypeId] = value;
					if (item.Rate <= 0.01f)
					{
						reusable3.Value.Add(bonusPrototype.PrototypeId);
					}
				}
			}
			int num = arg.Items.Count;
			foreach (KeyValuePair<string, Pair<int, int>> item2 in reusable2.Value)
			{
				AddItemWidget(arg.Items, item2.Key, level, 0, isBonus: false);
				if (arg.Items.Count > num)
				{
					ItemArgument value2 = arg.Items[num];
					Pair<int, int> value3 = item2.Value;
					value2.SubTitle = ((value3.Item1 >= value3.Item2) ? T._("{0} {1:N0}개", value2.SubTitle, value3.Item1) : T._("{0} {1:N0}-{2:N0}개", value2.SubTitle, value3.Item1, value3.Item2));
					value2.GoodEffect = reusable3.Value.Contains(item2.Key);
					arg.Items[num] = value2;
					num++;
				}
			}
		}
		AddQueue(arg);
	}

	public void ShowCommodityRewarded(Durango.Logic.Shop.Commodity commodity)
	{
		if (commodity == null)
		{
			return;
		}
		Argument arg = default(Argument);
		arg.Title = commodity.Title;
		arg.Button = T._("확인");
		List<ContentDescription> contentDescriptions = commodity.ContentDescriptions;
		if (contentDescriptions != null)
		{
			arg.Items = new List<ItemArgument>();
			foreach (ContentDescription item in contentDescriptions)
			{
				AddCommodityPreview(arg.Items, item, isBonus: false);
			}
		}
		arg.Sound = "ui_menu_quest_recieve";
		AddQueue(arg);
	}

	public void ShowQuestRewarded(QuestRewardResults quest)
	{
		QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(quest.QuestId);
		Argument arg = default(Argument);
		arg.Title = T._("{0} 완료", (questYml != null) ? questYml.Subject.ToString() : quest.QuestId);
		arg.Button = T._("확인");
		RewardInfo? reward = quest.Reward;
		if (reward.HasValue)
		{
			arg.Items = new List<ItemArgument>();
			AddRewardedItems(arg.Items, reward.Value, isBonus: false);
		}
		arg.EffectOn = true;
		arg.Sound = "ui_menu_quest_recieve";
		AddQueue(arg);
	}

	public void ShowAdviceReward([NotNull] Durango.Logic.LearningGuide.Advice advice, bool isRewarded)
	{
		Argument arg = default(Argument);
		arg.Title = T._("{0} 달성 보상", advice.Name);
		arg.Button = ((!isRewarded) ? null : T._("확인"));
		arg.Items = new List<ItemArgument>();
		AddTitle(arg.Items, advice.RewardTitleId, isBonus: false);
		if (KUtility.GetSize(advice.RewardItems) > 0)
		{
			Yaml.RewardItem[] rewardItems = advice.RewardItems;
			foreach (Yaml.RewardItem rewardItem in rewardItems)
			{
				AddItemWidget(arg.Items, rewardItem.prototype_id, rewardItem.level, rewardItem.count, isBonus: false);
			}
		}
		arg.EffectOn = isRewarded;
		if (isRewarded)
		{
			arg.Sound = "ui_menu_quest_recieve";
		}
		AddQueue(arg);
	}

	public void ShowAcceptedSupportRewards(AcceptedSupportRewards rewards)
	{
		Argument arg = default(Argument);
		arg.Title = T._("지원 요청 성공");
		arg.Button = T._("확인");
		arg.Items = new List<ItemArgument>();
		AddSupportRewards(arg.Items, rewards.RandomRewards, isBonus: true);
		AddSupportRewards(arg.Items, rewards.Rewards, isBonus: false);
		arg.EffectOn = true;
		arg.Sound = "ui_menu_faction_recieve";
		AddQueue(arg);
	}

	public void ShowMissionRewarded(Rewarded rewarded, Rewarded? bonus)
	{
		Argument arg = default(Argument);
		arg.Title = T._("임무 완료");
		arg.Button = T._("확인");
		arg.Items = new List<ItemArgument>();
		AddRewardedItems(arg.Items, rewarded.Reward, isBonus: false);
		if (bonus.HasValue)
		{
			AddRewardedItems(arg.Items, bonus.Value.Reward, isBonus: true);
		}
		arg.EffectOn = true;
		arg.Sound = "ui_menu_quest_recieve";
		AddQueue(arg);
	}

	public void ShowRewardInfo(string title, string buttonText, string sound, bool effectOn, RewardInfo reward, Action clicked = null)
	{
		Argument argument = default(Argument);
		argument.Title = title;
		argument.Button = buttonText;
		argument.Items = new List<ItemArgument>();
		argument.EffectOn = effectOn;
		argument.Sound = sound;
		argument.Clicked = clicked;
		Argument arg = argument;
		AddRewardedItems(arg.Items, reward, isBonus: false);
		AddQueue(arg);
	}

	public void ShowWarpAcceleratorRewardInfo(string title, string buttonText, string sound, bool effectOn, RewardInfo reward, WarpAcceleratorInfo warpAccelerator, Action clicked = null)
	{
		Argument argument = default(Argument);
		argument.Title = title;
		argument.Button = buttonText;
		argument.Items = new List<ItemArgument>();
		argument.WarpAccelerator = warpAccelerator;
		argument.EffectOn = effectOn;
		argument.Sound = sound;
		argument.Clicked = clicked;
		Argument arg = argument;
		AddRewardedItems(arg.Items, reward, isBonus: false);
		AddQueue(arg);
	}

	public void ShowPetTaskFinished(PetTaskFinishedEffect effect, RewardInfo info)
	{
		Argument arg = default(Argument);
		PetTask petTask = SingletonDict<string, PetTask>.Get(effect.TaskId);
		arg.Title = T._("{0} 완료", (petTask != null) ? petTask.Name.ToString() : effect.TaskId);
		arg.Button = T._("확인");
		arg.Items = new List<ItemArgument>();
		if (petTask != null && effect.PetExp == 0 && petTask.Exp > 0)
		{
			arg.Items.Add(new ItemArgument
			{
				Title = T._("경험치"),
				SubTitle = "FULL",
				Icon = "icon_exp_pet"
			});
		}
		else
		{
			AddPetExp(arg.Items, effect.PetExp, isBonus: false);
		}
		AddRewardedItems(arg.Items, info, isBonus: false);
		arg.Sound = "ui_menu_quest_recieve";
		AddQueue(arg);
	}

	public void ShowWarpRushRewardItemReceived(string title, WarpRushReward reward)
	{
		if (reward != null)
		{
			Argument argument = default(Argument);
			argument.Title = title;
			argument.Button = T._("확인");
			argument.EffectOn = true;
			argument.Sound = "ui_menu_quest_recieve";
			argument.Items = new List<ItemArgument>();
			Argument arg = argument;
			string caption = T._("획득한 제작법은 기름독섬에서 확인할 수 있습니다.");
			if (reward.Currency != null)
			{
				AddCurrency(arg.Items, reward.Currency.Type, reward.Currency.Amount, isBonus: false);
			}
			else if (reward.Item != null)
			{
				AddItemWidget(arg.Items, reward.Item.PrototypeId, reward.Item.Level, reward.Item.Count, isBonus: false);
			}
			else if (!string.IsNullOrEmpty(reward.Recipe))
			{
				AddRecipe(arg.Items, reward.Recipe, isBonus: false);
				arg.Caption = caption;
			}
			else if (!string.IsNullOrEmpty(reward.BlueprintId))
			{
				AddBlueprint(arg.Items, reward.BlueprintId, isBonus: false);
				arg.Caption = caption;
			}
			AddQueue(arg);
		}
	}

	public void ShowPioneerGradeUp(PioneerGradeUpEffect effect, RewardInfo info)
	{
		Argument argument = default(Argument);
		argument.Title = T._("개인섬 개척도 {0} 달성", effect.Grade);
		argument.Button = T._("확인");
		argument.Items = new List<ItemArgument>();
		argument.EffectOn = true;
		argument.Sound = "ui_menu_quest_recieve";
		Argument arg = argument;
		AddEstateSize(arg.Items, effect.EstateSize, isBonus: false);
		AddRewardedItems(arg.Items, info, isBonus: false);
		AddQueue(arg);
	}

	public void ShowOpenRewardBox(OpenRewardBoxEffect effect, RewardInfo info)
	{
		Argument argument = default(Argument);
		argument.Title = T._("획득");
		argument.Button = T._("확인");
		argument.Items = new List<ItemArgument>();
		argument.EffectOn = true;
		argument.Sound = "ui_menu_quest_recieve";
		Argument arg = argument;
		AddRewardedItems(arg.Items, info, isBonus: false);
		AddQueue(arg);
	}

	public void ShowReactingPropRewardItems([NotNull] Item[] rewardItems)
	{
		Argument arg = default(Argument);
		arg.Title = T._("아이템 획득");
		arg.Button = T._("확인");
		arg.Items = new List<ItemArgument>();
		for (int i = 0; i < rewardItems.Length; i++)
		{
			Item item = rewardItems[i];
			AddItemWidget(arg.Items, item.Prototype, item.Level, 1, item.Name, item.ColorR, item.ColorG, item.ColorB, isBonus: false);
		}
		AddQueue(arg);
	}

	protected override void UpdateLayout()
	{
		if (_captionWidget.gameObject.activeSelf)
		{
			int num = _captionLabel.fontSize * 2 + _captionLabel.spacingY;
			if (_captionLabel.printedSize.y > (float)num)
			{
				_captionLabel.overflowMethod = UILabel.Overflow.ShrinkContent;
				_captionLabel.height = num;
			}
			_captionWidget.height = _captionLabel.height + 20;
		}
		int safeHeight = UIManager.SafeHeight;
		safeHeight -= 120;
		Vector2 vector = _layout.UpdateLayout(null, safeHeight);
		_kScrollView.UpdateLayout();
		float num2 = (float)_scrollWidget.height - _kScrollView.ContentsLength;
		if (num2 > 0f)
		{
			vector = _layout.UpdateLayout(null, (float)safeHeight - num2);
		}
		if (_reset)
		{
			_kScrollView.MoveTo(0f, instant: true);
		}
		else
		{
			_kScrollView.MoveTo(_kScrollView.CurrentOffset, instant: false);
		}
		base.Widget.SetDimensions((int)vector.x, (int)vector.y);
		UIUtility.UpdateAnchors(base.transform);
		_reset = false;
	}

	private void Button_Clicked()
	{
		if (_clicked != null)
		{
			_clicked();
		}
		Hide();
	}

	protected override void OnTryConfirmOnModal()
	{
		if (_bottomWidget.gameObject.activeInHierarchy)
		{
			Button_Clicked();
		}
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _button;
	}

	private void SetTitle(string title)
	{
		_rewardTitle.text = title;
	}

	private void SetWarpAccelerator(WarpAcceleratorInfo? info)
	{
		if (!info.HasValue)
		{
			_warpAcceleratorRewardWidget.gameObject.SetActive(value: false);
			return;
		}
		_warpAcceleratorRewardWidget.gameObject.SetActive(value: true);
		_warpAcceleratorRewardWidget.Set(info.Value);
	}

	private void SetCaption(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			_captionWidget.gameObject.SetActive(value: false);
			return;
		}
		_captionLabel.text = text;
		_captionWidget.gameObject.SetActive(value: true);
	}

	private void SetButton(string text, [CanBeNull] Action clicked)
	{
		if (string.IsNullOrEmpty(text))
		{
			_bottomWidget.gameObject.SetActive(value: false);
			return;
		}
		_button.Text = text;
		_clicked = clicked;
		_bottomWidget.gameObject.SetActive(value: true);
	}

	public static void AddRewardedItems(List<ItemArgument> items, RewardInfo reward, bool isBonus)
	{
		if (reward.Titles != null)
		{
			string[] titles = reward.Titles;
			foreach (string id in titles)
			{
				AddTitle(items, id, isBonus);
			}
		}
		if (reward.UnlockedSkills != null)
		{
			Messages.Skill[] unlockedSkills = reward.UnlockedSkills;
			foreach (Messages.Skill skill in unlockedSkills)
			{
				AddSkill(items, skill, isBonus);
			}
		}
		if (reward.RandomItems != null)
		{
			Messages.RewardItem[] randomItems = reward.RandomItems;
			foreach (Messages.RewardItem msg in randomItems)
			{
				AddRewardItem(items, msg, isBonus: true);
			}
		}
		if (reward.Items != null)
		{
			Messages.RewardItem[] randomItems = reward.Items;
			foreach (Messages.RewardItem msg2 in randomItems)
			{
				AddRewardItem(items, msg2, isBonus);
			}
		}
		if (reward.Currency != null)
		{
			foreach (KeyValuePair<Currency, long> item in reward.Currency)
			{
				AddCurrency(items, item.Key, item.Value, isBonus);
			}
		}
		AddSkillPoint(items, reward.SkillPoints, isBonus);
		AddExp(items, reward.Exp, isBonus);
		if (reward.FriendshipPoint != null)
		{
			foreach (KeyValuePair<FactionType, int> item2 in reward.FriendshipPoint)
			{
				AddFactionGradePoint(items, item2.Key, item2.Value, isBonus);
			}
		}
		if (reward.Vouchers != null)
		{
			VoucherInfo[] vouchers = reward.Vouchers;
			foreach (VoucherInfo voucher in vouchers)
			{
				AddVoucher(items, voucher, isBonus);
			}
		}
		if (reward.Abilities != null)
		{
			AddAbilities(items, reward.Abilities, isBonus);
		}
		if (reward.BlueprintIds != null)
		{
			string[] titles = reward.BlueprintIds;
			foreach (string blueprintId in titles)
			{
				AddBlueprint(items, blueprintId, isBonus);
			}
		}
		if (reward.RecipeIds != null)
		{
			string[] titles = reward.RecipeIds;
			foreach (string recipeId in titles)
			{
				AddRecipe(items, recipeId, isBonus);
			}
		}
		if (KUtility.GetSize(reward.Memos) > 0)
		{
			AddMemos(items, reward.Memos);
		}
	}

	private static void AddMemos(List<ItemArgument> items, Pair<Shared.Memo.MemoType, int>[] memos)
	{
		Durango.Logic.Encyclopedia.MemoType type = MemosYaml.ToClientMemoType(memos[0].Item1);
		int zeroIndex = memos[0].Item2 - 1;
		string memoTitle = MemoSystem.GetMemoTitle(type, zeroIndex);
		items.Add(new ItemArgument
		{
			Title = T._("메모"),
			SubTitle = memoTitle,
			Icon = "icon_encyclopedia_submemo",
			Amount = memos.Length
		});
	}

	private static void AddCommodityPreview(List<ItemArgument> items, ContentDescription preview, bool isBonus)
	{
		if (preview != null)
		{
			items.Add(new ItemArgument
			{
				Title = preview.Name,
				SubTitle = preview.Text,
				Icon = preview.Icon,
				IconColor = preview.IconColor,
				IsBonus = isBonus
			});
		}
	}

	private static void AddTitle(List<ItemArgument> items, string id, bool isBonus)
	{
		if (!string.IsNullOrEmpty(id))
		{
			Durango.Logic.Statistics.Title title = GameSystem<StatisticsSystem>.Instance().GetTitle(id);
			if (title != null)
			{
				items.Add(new ItemArgument
				{
					Title = T._("타이틀"),
					SubTitle = title.Name,
					Icon = "icon_autoguidegroup_title",
					IsBonus = isBonus
				});
			}
		}
	}

	private static void AddItemWidget(List<ItemArgument> items, string prototypeId, int? level, int count, bool isBonus)
	{
		AddItemWidget(items, prototypeId, level, count, null, null, null, null, isBonus);
	}

	private static void AddItemWidget(List<ItemArgument> items, string prototypeId, int? level, int count, string name, string colorR, string colorG, string colorB, bool isBonus)
	{
		Prototype prototype = null;
		if (level.HasValue)
		{
			prototype = PrototypeYaml.GetItemPrototype(prototypeId, level.Value);
		}
		else
		{
			List<Prototype> list = SingletonDict<string, List<Prototype>>.Get(prototypeId);
			if (list != null && list.Count > 0)
			{
				prototype = list[list.Count - 1];
			}
		}
		if (prototype != null)
		{
			string iconRTable = null;
			string iconGTable = null;
			string iconBTable = null;
			ItemColor iconColor = default(ItemColor);
			if (string.IsNullOrEmpty(colorR) && string.IsNullOrEmpty(colorG) && string.IsNullOrEmpty(colorB))
			{
				iconRTable = prototype.ColorR;
				iconGTable = prototype.ColorG;
				iconBTable = prototype.ColorB;
			}
			else
			{
				iconColor = new ItemColor(colorR, colorG, colorB);
			}
			ItemArgument itemArgument = default(ItemArgument);
			itemArgument.Title = T._("아이템");
			itemArgument.SubTitle = ((name == null) ? prototype.Name.ToString() : name);
			itemArgument.Amount = count;
			itemArgument.Icon = prototype.Icon;
			itemArgument.IconColor = iconColor;
			itemArgument.IconRTable = iconRTable;
			itemArgument.IconGTable = iconGTable;
			itemArgument.IconBTable = iconBTable;
			itemArgument.IsBonus = isBonus;
			ItemArgument item = itemArgument;
			if (level.HasValue)
			{
				item.Sup = LocalizeUtil.FormatLevel(level.Value);
				item.ItemPrototype = new KeyValuePair<string, int>(prototypeId, level.Value);
			}
			items.Add(item);
		}
	}

	private static void AddRewardItem(List<ItemArgument> items, Messages.RewardItem msg, bool isBonus)
	{
		AddItemWidget(items, msg.PrototypeId, msg.Level, msg.Count, msg.NameGettext, msg.ColorR, msg.ColorG, msg.ColorB, isBonus);
	}

	private static void AddRecipe(List<ItemArgument> items, string recipeId, bool isBonus)
	{
		Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(recipeId);
		if (recipe != null)
		{
			items.Add(new ItemArgument
			{
				Title = T._("제작법"),
				SubTitle = recipe.Name,
				Icon = recipe.Icon,
				IsBonus = isBonus
			});
		}
	}

	private static void AddBlueprint(List<ItemArgument> items, string blueprintId, bool isBonus)
	{
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(blueprintId);
		if (blueprint != null)
		{
			items.Add(new ItemArgument
			{
				Title = T._("제작법"),
				SubTitle = blueprint.Name,
				Icon = blueprint.Icon,
				IsBonus = isBonus
			});
		}
	}

	private static void AddPetExp(List<ItemArgument> items, int? exp, bool isBonus)
	{
		if (exp.HasValue && exp.Value != 0)
		{
			items.Add(new ItemArgument
			{
				Title = T._("경험치"),
				Amount = exp.Value,
				Icon = "icon_exp_pet",
				IsBonus = isBonus
			});
		}
	}

	private static void AddExp(List<ItemArgument> items, int? exp, bool isBonus)
	{
		if (exp.HasValue && exp.Value != 0)
		{
			items.Add(new ItemArgument
			{
				Title = T._("경험치"),
				Amount = exp.Value,
				Icon = "icon_exp",
				IsBonus = isBonus
			});
		}
	}

	private static void AddSupportRewards(List<ItemArgument> items, Messages.SupportRewards rewards, bool isBonus)
	{
		int i = 0;
		for (int size = KUtility.GetSize(rewards.Items); i < size; i++)
		{
			ItemSupportReward support = rewards.Items[i];
			AddSupportItemReward(items, support, isBonus);
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(rewards.Moneys); j < size2; j++)
		{
			Money money = rewards.Moneys[j];
			AddCurrency(items, money.Currency, money.Amount, isBonus);
		}
	}

	private static void AddSupportItemReward(List<ItemArgument> items, ItemSupportReward support, bool isBonus)
	{
		items.Add(new ItemArgument
		{
			Title = T._("아이템"),
			SubTitle = support.Item.Name,
			Amount = support.Count,
			Sup = LocalizeUtil.FormatLevel(support.Item.Level),
			Icon = support.Item.Icon,
			IconColor = new ItemColor(support.Item.ColorR, support.Item.ColorG, support.Item.ColorB),
			IsBonus = isBonus
		});
	}

	private static void AddFactionGradePoint(List<ItemArgument> items, FactionType factionKey, int factionValue, bool isBonus)
	{
		if (factionValue != 0)
		{
			Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(factionKey);
			if (faction != null)
			{
				items.Add(new ItemArgument
				{
					Title = string.Format("{0} {1}", faction.Name, T._("우호도")),
					Amount = factionValue,
					Icon = IconMap.Get(factionKey),
					IsBonus = isBonus
				});
			}
		}
	}

	private static void AddVoucher(List<ItemArgument> items, VoucherInfo voucher, bool isBonus)
	{
		Voucher voucher2 = SingletonDict<string, Voucher>.Get(voucher.VoucherId);
		string text = null;
		switch (voucher2.GuideType)
		{
		case GuideType.WarpToPort:
			text = T._("개인섬/도시섬 항구에서 이용 가능");
			break;
		case GuideType.Cashshop:
			text = T._("상점에서 이용가능");
			break;
		}
		items.Add(new ItemArgument
		{
			Title = ((!string.IsNullOrEmpty(text)) ? string.Format("{0} [size=16]<em>{1}</em>[/size]", T._("이용권"), text) : T._("이용권")),
			SubTitle = voucher2.Name,
			Amount = voucher.Count,
			Icon = voucher2.Icon,
			IsBonus = isBonus
		});
	}

	private static void AddSkillPoint(List<ItemArgument> items, int? point, bool isBonus)
	{
		if (point.HasValue && point.Value != 0)
		{
			items.Add(new ItemArgument
			{
				Title = T._("스킬포인트"),
				Amount = point.Value,
				Icon = "icon_sp",
				IsBonus = isBonus
			});
		}
	}

	private static void AddSkill(List<ItemArgument> items, Messages.Skill skill, bool isBonus)
	{
		Node node = GameSystem<SkillSystem>.Instance().FindSkill(skill);
		if (node != null)
		{
			items.Add(new ItemArgument
			{
				Title = T._("스킬"),
				SubTitle = node.Name,
				Icon = node.Icon,
				IsBonus = isBonus
			});
		}
	}

	private static void AddAbilities(List<ItemArgument> items, IEnumerable<KeyValuePair<Basic, int>> abilities, bool isBonus)
	{
		if (abilities == null)
		{
			return;
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		if (value.Length == 0)
		{
			return;
		}
		foreach (KeyValuePair<Basic, int> ability in abilities)
		{
			if (ability.Value != 0)
			{
				value.Append(ability.Key.GetName()).Append(" ").AppendFormat("{0:+0;-0}", ability.Value)
					.Append(" ");
			}
		}
		items.Add(new ItemArgument
		{
			Title = T._("능력치"),
			SubTitle = value.ToString().Trim(),
			Icon = "craft_icon_star_enable",
			IsBonus = isBonus
		});
	}

	private static void AddCurrency(List<ItemArgument> items, Currency currency, long currencyValue, bool isBonus)
	{
		if (currencyValue != 0L && currency != Currency.Invalid)
		{
			items.Add(new ItemArgument
			{
				Title = currency.GetName(),
				Amount = (int)currencyValue,
				Icon = Durango.Logic.Item.Inventory.GetIcon(currency),
				IsBonus = isBonus
			});
		}
	}

	private static void AddEstateSize(List<ItemArgument> items, int? size, bool isBonus)
	{
		if (size.HasValue && (!size.HasValue || size.GetValueOrDefault() > 0))
		{
			items.Add(new ItemArgument
			{
				Title = T._("개인섬 사유지 면적 최대 <em>{0}칸</em>", size.Value),
				Icon = "privateland_icon",
				IsBonus = isBonus
			});
		}
	}
}
