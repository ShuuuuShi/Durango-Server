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

public class ArtifactInfoContextInteriorSet : ItemContextBase
{
	[SerializeField]
	private UIWidget _currentSetEffect;

	[SerializeField]
	private UISpriteLabel _textCurrentSetEffect;

	[SerializeField]
	private ArtifactInteriorSetItem _setItemBase;

	private List<ArtifactInteriorSetItem.Info> _itemInfoList = new List<ArtifactInteriorSetItem.Info>();

	private ListObjectPool<ArtifactInteriorSetItem> _setItems;

	private bool _unavailable;

	public override void Init()
	{
		base.Init();
		UIEventListener.Get(_textCurrentSetEffect.gameObject).onClick = delegate
		{
			if (_unavailable)
			{
				CardNewsPopup cardNewsPopup = UIManager.Popup.Tooltip<CardNewsPopup>();
				if (cardNewsPopup.Load("interior_set"))
				{
					cardNewsPopup.Show();
				}
			}
		};
		_setItems = new ListObjectPool<ArtifactInteriorSetItem>();
		_setItems.BaseObject = _setItemBase;
		_setItems.UseBase = false;
	}

	public bool Set(ArtifactSet? interiorSet, int statFactor, string blueprintId)
	{
		int interiorSetMinRequiredStatFactor = Singleton<ArtifactSetEffectsYaml>.Instance.InteriorSetMinRequiredStatFactor;
		ArtifactInteriorSetItem.Info info = null;
		int num = 0;
		_unavailable = statFactor < interiorSetMinRequiredStatFactor;
		_setItems.BeginLoad();
		if (!_unavailable)
		{
			foreach (ArtifactInteriorSetItem.Info item in EnumerateItemInfo(interiorSet, blueprintId))
			{
				if (_setItems.GetNext().Set(item))
				{
					info = item;
					num++;
				}
			}
		}
		_setItems.EndLoad();
		switch (num)
		{
		case 0:
			if (_unavailable)
			{
				_currentSetEffect.gameObject.SetActive(value: true);
				_textCurrentSetEffect.text = T._("항균력이 {0} 이상일때 세트효과 메뉴가 활성화 됩니다.\n[size=8] [/size]\n[FFD85BE6]세트효과 [icon=img_loading_unknown_question2][-]", interiorSetMinRequiredStatFactor);
			}
			else
			{
				_currentSetEffect.gameObject.SetActive(value: false);
			}
			break;
		case 1:
			if (info != null)
			{
				_currentSetEffect.gameObject.SetActive(value: true);
				_textCurrentSetEffect.text = info.SummaryDescription;
			}
			break;
		default:
			_currentSetEffect.gameObject.SetActive(value: true);
			_textCurrentSetEffect.text = T._("[FFD85B][icon=icon_caution:1.2][-]\n조건을 만족하는 세트 효과가 너무 많습니다.");
			foreach (ArtifactInteriorSetItem item2 in _setItems.Where((ArtifactInteriorSetItem item) => item.IsFullChecked))
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
		if (_setItems.Count > 0 && !_currentSetEffect.gameObject.activeSelf)
		{
			_setItems[0].ShowDotLine(show: false);
		}
	}

	private void UpdateLayout()
	{
		int num = 0;
		if (_currentSetEffect.gameObject.activeSelf)
		{
			num = _textCurrentSetEffect.height + 32;
			_currentSetEffect.transform.localPosition = Vector3.down * (num / 2);
			_currentSetEffect.height = num;
		}
		float num2 = (int)UIUtility.WidgetsReposition(_setItems, Vector3.down, new Vector3(0f, -num));
		_body.height = num + (int)num2;
	}

	private IEnumerable<ArtifactInteriorSetItem.Info> EnumerateItemInfo(ArtifactSet? setEffect, string blueprintId)
	{
		if (setEffect.HasValue)
		{
			Dictionary<string, ArtifactInteriorSet> interiorSet = Singleton<ArtifactSetEffectsYaml>.Instance.InteriorSet;
			_itemInfoList.Clear();
			int num = 0;
			foreach (KeyValuePair<string, ArtifactInteriorSet> item in interiorSet)
			{
				ArtifactInteriorSet value = item.Value;
				if ((value.TargetPrototypes == null || value.TargetPrototypes.Contains(blueprintId)) && (string.IsNullOrEmpty(value.Season) || GameSystem<SeasonSystem>.Instance().GetSeasonStatus(value.Season) == SeasonSystem.Period.During))
				{
					_itemInfoList.Add(new ArtifactInteriorSetItem.Info(num++, setEffect.Value.TagSlots.Get(item.Key), item.Value));
				}
			}
			_itemInfoList.Sort();
			return _itemInfoList;
		}
		return Enumerable.Empty<ArtifactInteriorSetItem.Info>();
	}
}
