using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ArtifactInfoContextInteriorMood : ItemContextBase
{
	[SerializeField]
	private UIWidget _currentMood;

	[SerializeField]
	private UISpriteLabel _textCurrentMood;

	[SerializeField]
	private ArtifactInteriorMoodItem _moodItemBase;

	private List<ArtifactInteriorMoodItem.Info> _itemInfoList = new List<ArtifactInteriorMoodItem.Info>();

	private ListObjectPool<ArtifactInteriorMoodItem> _moodItems;

	private bool _unavailable;

	public override void Init()
	{
		base.Init();
		UIEventListener.Get(_textCurrentMood.gameObject).onClick = delegate
		{
			if (_unavailable)
			{
				CardNewsPopup cardNewsPopup = UIManager.Popup.Tooltip<CardNewsPopup>();
				if (cardNewsPopup.Load("interior_mood"))
				{
					cardNewsPopup.Show();
				}
			}
		};
		_moodItems = new ListObjectPool<ArtifactInteriorMoodItem>();
		_moodItems.BaseObject = _moodItemBase;
		_moodItems.UseBase = true;
	}

	public bool Set(ArtifactMood? mood, int statFactor, string blueprintId)
	{
		int interiorMoodMinRequiredStatFactor = Singleton<ArtifactSetEffectsYaml>.Instance.InteriorMoodMinRequiredStatFactor;
		ArtifactInteriorMoodItem.Info info = null;
		int num = 0;
		_unavailable = statFactor < interiorMoodMinRequiredStatFactor;
		_moodItems.BeginLoad();
		if (!_unavailable)
		{
			foreach (ArtifactInteriorMoodItem.Info item in EnumerateItemInfo(mood, blueprintId))
			{
				ArtifactInteriorMoodItem next = _moodItems.GetNext();
				if (next.Set(item))
				{
					info = item;
					num++;
				}
			}
		}
		_moodItems.EndLoad();
		switch (num)
		{
		case 0:
			if (_unavailable)
			{
				_currentMood.gameObject.SetActive(value: true);
				_textCurrentMood.text = T._("안락도가 {0} 이상일때 분위기 메뉴가 활성화 됩니다.\n[size=8] [/size]\n[FFD85BE6]분위기효과 [icon=img_loading_unknown_question2][-]", interiorMoodMinRequiredStatFactor);
			}
			else
			{
				_currentMood.gameObject.SetActive(value: false);
			}
			break;
		case 1:
			if (info != null)
			{
				_currentMood.gameObject.SetActive(value: true);
				_textCurrentMood.text = info.SummaryDescription;
			}
			break;
		default:
			_currentMood.gameObject.SetActive(value: true);
			_textCurrentMood.text = T._("[FFD85B][icon=icon_caution:1.2][-]\n공간의 분위기가 혼란하여 효과가 발동되지 않습니다.");
			foreach (ArtifactInteriorMoodItem item2 in _moodItems.Where((ArtifactInteriorMoodItem item) => item.IsFullGauge))
			{
				item2.SetComplexity();
			}
			break;
		}
		RefreshFirstDotLine();
		UpdateLayout();
		return num > 1;
	}

	private void RefreshFirstDotLine()
	{
		if (_moodItems.Count > 0 && !_currentMood.gameObject.activeSelf)
		{
			_moodItems[0].ShowDotLine(show: false);
		}
	}

	private void UpdateLayout()
	{
		int num = 0;
		if (_currentMood.gameObject.activeSelf)
		{
			num = _textCurrentMood.height + 32;
			_currentMood.transform.localPosition = Vector3.down * (num / 2);
			_currentMood.height = num;
		}
		float num2 = (int)UIUtility.WidgetsReposition(_moodItems, Vector3.down, new Vector3(0f, -num));
		_body.height = num + (int)num2;
	}

	private IEnumerable<ArtifactInteriorMoodItem.Info> EnumerateItemInfo(ArtifactMood? moodEffect, string blueprintId)
	{
		if (moodEffect.HasValue)
		{
			Dictionary<string, ArtifactInteriorMood> interiorMood = Singleton<ArtifactSetEffectsYaml>.Instance.InteriorMood;
			_itemInfoList.Clear();
			int num = 0;
			foreach (KeyValuePair<string, ArtifactInteriorMood> item in interiorMood)
			{
				ArtifactInteriorMood value = item.Value;
				if ((value.TargetPrototypes == null || value.TargetPrototypes.Contains(blueprintId)) && (string.IsNullOrEmpty(value.Season) || GameSystem<SeasonSystem>.Instance().GetSeasonStatus(value.Season) == SeasonSystem.Period.During))
				{
					_itemInfoList.Add(new ArtifactInteriorMoodItem.Info(num++, GetCurrentLevel(item.Key, moodEffect.Value), item.Value));
				}
			}
			_itemInfoList.Sort();
			return _itemInfoList;
		}
		return Enumerable.Empty<ArtifactInteriorMoodItem.Info>();
	}

	private static int GetCurrentLevel(string tagId, ArtifactMood mood)
	{
		return mood.TagLevels.FirstOrDefault((KeyValuePair<string, int> tagLevel) => tagId == tagLevel.Key).Value;
	}
}
