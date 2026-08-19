using System.Collections.Generic;
using System.Linq;
using Building;
using Crafting;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ExpectResultDetailWidget : MonoBehaviour
{
	private enum DescriptionType
	{
		Hide,
		NoTags,
		UnknownTags
	}

	private struct ExpectTag
	{
		public string Id;

		public int Level;

		private bool _isValidTag;

		private Yaml.Tag _tag;

		public bool IsUnreveal => string.IsNullOrEmpty(Id);

		public Yaml.Tag GetTag()
		{
			if (!_isValidTag)
			{
				_isValidTag = true;
				_tag = ((!string.IsNullOrEmpty(Id)) ? SingletonDict<string, Yaml.Tag>.Get(Id) : null);
			}
			return _tag;
		}
	}

	private const float _tagWidgetPadding = 16f;

	private const float _tagWidgetMargin = 10f;

	private const int _unrevelTagRowCount = 2;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private RectLayoutComponent _scrollViewContainer;

	[SerializeField]
	private UIWidget _durabilityAndModifiableWidget;

	[SerializeField]
	private UISprite _iconDurability;

	[SerializeField]
	private UILabel _textDurability;

	[SerializeField]
	private UIWidget _modifiableWidget;

	[SerializeField]
	private UISprite _iconModifiableCount;

	[SerializeField]
	private UILabel _textModifiableCount;

	[SerializeField]
	private UIWidget _reformWidget;

	[SerializeField]
	private UISprite _iconReformRecipe;

	[SerializeField]
	private UILabel _textReformSlotName;

	[SerializeField]
	private UIWidget _timerWidget;

	[SerializeField]
	private UILabel _textTimer;

	[SerializeField]
	private UIWidget _expectTagsWidget;

	[SerializeField]
	private CraftExpectTagItemWidget _tagWidgetBase;

	[SerializeField]
	private UIWidget _emptyTagsWidget;

	[SerializeField]
	private UILabel _emptyTagsLabel;

	[SerializeField]
	private string[] _durabilityIconNames;

	[SerializeField]
	private int[] _durabilityPercentages;

	[SerializeField]
	private string _modifiableIconName;

	[SerializeField]
	private string _unmodifiableIconName;

	private bool _scrollResetFlag = true;

	private ListObjectPool<CraftExpectTagItemWidget> _tagWidgetList;

	private readonly List<ExpectTag> _expectTags = new List<ExpectTag>();

	private void OnDisable()
	{
		_scrollResetFlag = true;
	}

	public void Set(CraftSlotContainer craft)
	{
		SetEntrustTime(craft?.Recipe);
	}

	public void Set(BuildSlotContainer build)
	{
		SetBuildTime(build?.Blueprint);
	}

	public void ClearEstimation()
	{
		_durabilityAndModifiableWidget.gameObject.SetActive(value: false);
		_reformWidget.gameObject.SetActive(value: false);
		_modifiableWidget.gameObject.SetActive(value: false);
		SetDurability(null);
		SetTagItemWidgets(null);
		SetEmptyTagsWidget(DescriptionType.Hide);
		_scrollViewContainer.UpdateLayout();
		_scrollView.Reposition(_scrollResetFlag, !_scrollResetFlag);
		_scrollResetFlag = false;
		UIUtility.UpdateAnchors(base.transform);
	}

	public void SetEstimation(CraftEstimation? estimation, RecipeReform reformRecipe, bool isImmuneToTime, bool isTimeLimited)
	{
		if (reformRecipe != null)
		{
			_durabilityAndModifiableWidget.gameObject.SetActive(value: false);
			_reformWidget.gameObject.SetActive(value: true);
			_iconReformRecipe.spriteName = reformRecipe.Icon;
			_textReformSlotName.text = reformRecipe.RecipeNameForSlot;
		}
		else
		{
			_durabilityAndModifiableWidget.gameObject.SetActive(value: true);
			_reformWidget.gameObject.SetActive(value: false);
			if (!estimation.HasValue)
			{
				SetDurability(null);
			}
			else
			{
				SetDurability(estimation.Value.Durability, isImmuneToTime, isTimeLimited);
			}
			SetModifiableInfo(estimation);
		}
		SetTagsInfo(estimation);
		_scrollViewContainer.UpdateLayout();
		_scrollView.Reposition(_scrollResetFlag, !_scrollResetFlag);
		_scrollResetFlag = false;
		UIUtility.UpdateAnchors(base.transform);
	}

	public void SetEstimation(BuildEstimation? estimation)
	{
		_durabilityAndModifiableWidget.gameObject.SetActive(value: true);
		_reformWidget.gameObject.SetActive(value: false);
		if (!estimation.HasValue)
		{
			SetDurability(null);
		}
		else
		{
			SetDurability(Vector2.one * estimation.Value.Durability);
		}
		SetTagsInfo(estimation);
		_modifiableWidget.gameObject.SetActive(value: false);
		_scrollViewContainer.UpdateLayout();
		_scrollView.Reposition(_scrollResetFlag, !_scrollResetFlag);
		_scrollResetFlag = false;
		UIUtility.UpdateAnchors(base.transform);
	}

	public void SetEstimation([NotNull] Artifact artifact)
	{
		_durabilityAndModifiableWidget.gameObject.SetActive(value: true);
		_reformWidget.gameObject.SetActive(value: false);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		SetDurability(new Vector2(artifact.ArtifactState.Durability.Get(predictedServerTime), artifact.ArtifactState.Durability.Max(predictedServerTime)));
		SetTagsInfo(artifact);
		_modifiableWidget.gameObject.SetActive(value: false);
		_scrollViewContainer.UpdateLayout();
		_scrollView.Reposition(_scrollResetFlag, !_scrollResetFlag);
		_scrollResetFlag = false;
		UIUtility.UpdateAnchors(base.transform);
	}

	public void SetEstimation(TechSupportEstimate? estimate, RecipeReform reformRecipe)
	{
		_durabilityAndModifiableWidget.gameObject.SetActive(value: false);
		if (reformRecipe != null)
		{
			_reformWidget.gameObject.SetActive(value: true);
			_iconReformRecipe.spriteName = reformRecipe.Icon;
			_textReformSlotName.text = reformRecipe.RecipeNameForSlot;
		}
		else
		{
			_reformWidget.gameObject.SetActive(value: false);
		}
		SetTagItemWidgets((!estimate.HasValue) ? null : GetRawTags(estimate.Value.Tags));
		SetEmptyTagsWidget(DescriptionType.Hide);
		_scrollViewContainer.UpdateLayout();
		_scrollView.Reposition(_scrollResetFlag, !_scrollResetFlag);
		_scrollResetFlag = false;
		UIUtility.UpdateAnchors(base.transform);
	}

	private void SetEntrustTime(Crafting.Recipe recipe)
	{
		if (recipe.Entrusts)
		{
			_timerWidget.gameObject.SetActive(value: true);
			_textTimer.text = TimedeltaFormatter.ColonFormat(recipe.DurationWait);
		}
		else
		{
			_timerWidget.gameObject.SetActive(value: false);
		}
	}

	private void SetBuildTime(Building.Blueprint blueprint)
	{
		int num = blueprint?.PostprocessTime ?? 0;
		if (num > 0)
		{
			_timerWidget.gameObject.SetActive(value: true);
			_textTimer.text = TimedeltaFormatter.ColonFormat(num);
		}
		else
		{
			_timerWidget.gameObject.SetActive(value: false);
		}
	}

	private void SetDurability(Vector2? durability, bool isImmuneToTime = false, bool isTimeLimited = false)
	{
		if (durability.HasValue)
		{
			float x = durability.Value.x;
			float y = durability.Value.y;
			if (isImmuneToTime)
			{
				int percentage = ((y > 0f) ? Mathf.CeilToInt(x / y * 100f) : 0);
				_iconDurability.spriteName = UIUtility.GetValueByPercentage(percentage, _durabilityPercentages, _durabilityIconNames);
				_iconDurability.color = Color.white;
			}
			else if (isTimeLimited)
			{
				_iconDurability.spriteName = "icon_make_alert";
				_iconDurability.color = Color.black;
			}
			else
			{
				_iconDurability.spriteName = "img_clock_small";
				_iconDurability.color = new Color32(208, 201, 179, byte.MaxValue);
			}
			_textDurability.text = Util.LocalizedDurability(x, y);
		}
		else
		{
			_iconDurability.spriteName = _durabilityIconNames[0];
			_iconDurability.color = Color.white;
			_textDurability.text = "?";
		}
	}

	private void SetModifiableInfo(CraftEstimation? estimation)
	{
		_modifiableWidget.gameObject.SetActive(value: true);
		if (estimation.HasValue)
		{
			_iconModifiableCount.spriteName = ((estimation.Value.ModifiableCount <= 0) ? _unmodifiableIconName : _modifiableIconName);
			_textModifiableCount.text = Util.LocalizedModifiableCount(estimation.Value.ModifiableCount);
		}
		else
		{
			_iconModifiableCount.spriteName = _modifiableIconName;
			_textModifiableCount.text = "?";
		}
	}

	private void SetTagsInfo(CraftEstimation? estimation)
	{
		if (!estimation.HasValue)
		{
			SetTagItemWidgets(null);
			SetEmptyTagsWidget(DescriptionType.UnknownTags);
			return;
		}
		CraftEstimation value = estimation.Value;
		if (KUtility.GetSize(value.Tags) + value.UnrevealedRareTagCount == 0)
		{
			SetTagItemWidgets(null);
			SetEmptyTagsWidget(DescriptionType.NoTags);
		}
		else
		{
			SetTagItemWidgets(value.Tags, value.UnrevealedRareTagCount);
			SetEmptyTagsWidget(DescriptionType.Hide);
		}
	}

	private void SetTagsInfo(BuildEstimation? estimation)
	{
		if (!estimation.HasValue)
		{
			SetTagItemWidgets(null);
			SetEmptyTagsWidget(DescriptionType.UnknownTags);
			return;
		}
		BuildEstimation value = estimation.Value;
		if (KUtility.GetSize(value.Tags) + value.UnrevealedRareTagCount == 0)
		{
			SetTagItemWidgets(null);
			SetEmptyTagsWidget(DescriptionType.NoTags);
		}
		else
		{
			SetTagItemWidgets(value.Tags, value.UnrevealedRareTagCount);
			SetEmptyTagsWidget(DescriptionType.Hide);
		}
	}

	private void SetTagsInfo([NotNull] Artifact artifact)
	{
		IList<TagData> tags = artifact.Tags;
		if (KUtility.GetSize(tags) == 0)
		{
			SetTagItemWidgets(null);
			SetEmptyTagsWidget(DescriptionType.NoTags);
		}
		else
		{
			SetTagItemWidgets(GetRawTags(tags));
			SetEmptyTagsWidget(DescriptionType.Hide);
		}
	}

	private void SetTagItemWidgets([CanBeNull] IEnumerable<KeyValuePair<string, int>> tags)
	{
		if (tags == null)
		{
			_expectTagsWidget.gameObject.SetActive(value: false);
			return;
		}
		_expectTagsWidget.gameObject.SetActive(value: true);
		ListObjectPool<CraftExpectTagItemWidget> tagWidgetList = GetTagWidgetList();
		tagWidgetList.BeginLoad();
		foreach (KeyValuePair<string, int> tag in tags)
		{
			tagWidgetList.GetNext().Set(tag.Key, tag.Value);
		}
		tagWidgetList.EndLoad();
		float num = UpdateTagsLayout(tagWidgetList);
		_expectTagsWidget.height = (int)(num + 32f);
		UIUtility.UpdateAnchors(_expectTagsWidget.transform);
	}

	private void SetTagItemWidgets([NotNull] Dictionary<string, int> tags, int unrevealedRareTagCount)
	{
		_expectTagsWidget.gameObject.SetActive(value: true);
		_expectTags.Clear();
		for (int i = 0; i < unrevealedRareTagCount; i++)
		{
			_expectTags.Add(default(ExpectTag));
		}
		foreach (KeyValuePair<string, int> tag in tags)
		{
			_expectTags.Add(new ExpectTag
			{
				Id = tag.Key,
				Level = tag.Value
			});
		}
		_expectTags.Sort(ExpectTagComparision);
		int width = (int)(((float)_expectTagsWidget.width - 32f - 10f * (float)(unrevealedRareTagCount - 1)) / 2f);
		ListObjectPool<CraftExpectTagItemWidget> tagWidgetList = GetTagWidgetList();
		tagWidgetList.BeginLoad();
		for (int j = 0; j < _expectTags.Count; j++)
		{
			ExpectTag expectTag = _expectTags[j];
			CraftExpectTagItemWidget next = tagWidgetList.GetNext();
			if (expectTag.IsUnreveal)
			{
				next.SetUnrevealTag(TagGrade.Rare);
				next.width = width;
			}
			else
			{
				next.Set(expectTag.Id, expectTag.Level);
			}
		}
		tagWidgetList.EndLoad();
		float num = UpdateTagsLayout(tagWidgetList);
		_expectTagsWidget.height = (int)(num + 32f);
		UIUtility.UpdateAnchors(_expectTagsWidget.transform);
	}

	private void SetEmptyTagsWidget(DescriptionType type)
	{
		switch (type)
		{
		case DescriptionType.Hide:
			_emptyTagsWidget.gameObject.SetActive(value: false);
			break;
		case DescriptionType.NoTags:
			_emptyTagsWidget.gameObject.SetActive(value: true);
			_emptyTagsLabel.text = T._("속성 없음");
			break;
		case DescriptionType.UnknownTags:
			_emptyTagsWidget.gameObject.SetActive(value: true);
			_emptyTagsLabel.text = "?";
			break;
		}
	}

	private static IEnumerable<KeyValuePair<string, int>> GetRawTags(IEnumerable<TagData> tags)
	{
		return tags?.Select((TagData t) => new KeyValuePair<string, int>(t.Id, t.Level));
	}

	private static IEnumerable<KeyValuePair<string, int>> GetRawTags(IEnumerable<Messages.Tag> tags)
	{
		return tags?.Select((Messages.Tag t) => new KeyValuePair<string, int>(t.Id, t.Level));
	}

	private static int ExpectTagComparision(ExpectTag t1, ExpectTag t2)
	{
		bool isUnreveal = t1.IsUnreveal;
		bool isUnreveal2 = t2.IsUnreveal;
		if (isUnreveal != isUnreveal2)
		{
			if (isUnreveal)
			{
				return -1;
			}
			return 1;
		}
		Yaml.Tag tag = t1.GetTag();
		Yaml.Tag tag2 = t2.GetTag();
		if (tag == null && tag2 == null)
		{
			return string.CompareOrdinal(t1.Id, t2.Id);
		}
		if (tag == null)
		{
			return -1;
		}
		if (tag2 == null)
		{
			return 1;
		}
		TagGrade grade = tag.Grade;
		int num = tag2.Grade - grade;
		if (num == 0)
		{
			num = string.CompareOrdinal(tag.Name, tag2.Name);
		}
		return num;
	}

	[NotNull]
	private ListObjectPool<CraftExpectTagItemWidget> GetTagWidgetList()
	{
		if (_tagWidgetList == null)
		{
			_tagWidgetList = new ListObjectPool<CraftExpectTagItemWidget>();
			_tagWidgetList.BaseObject = _tagWidgetBase;
			_tagWidgetList.UseBase = true;
		}
		return _tagWidgetList;
	}

	private float UpdateTagsLayout(ListObjectPool<CraftExpectTagItemWidget> list)
	{
		int num = (int)((float)_expectTagsWidget.width - 32f);
		Vector3 vector = _expectTagsWidget.localCorners[1] + new Vector3(16f, -16f);
		float num2 = 0f;
		int height = _tagWidgetBase.height;
		float num3 = 0f;
		float result = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			CraftExpectTagItemWidget craftExpectTagItemWidget = list[i];
			if (num2 > 0f && num2 + (float)craftExpectTagItemWidget.width > (float)num)
			{
				num2 = 0f;
				num3 += (float)height + 10f;
				i--;
			}
			else
			{
				craftExpectTagItemWidget.SetPosition(vector + new Vector3(num2, 0f - num3), 0f, 1f);
				num2 += (float)craftExpectTagItemWidget.width + 10f;
				result = num3 + (float)height;
			}
		}
		return result;
	}
}
