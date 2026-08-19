using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Encyclopedia;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class EncyclopediaCropWidget : MonoBehaviour
{
	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UISprite _progressSprite;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private UILabel _masteryLabel;

	[SerializeField]
	private ListObjectPool _masteryList;

	public string Key { get; private set; }

	public event Action<string> Clicked;

	public void Set(string key, FarmingEncyclopediaData data)
	{
		Key = key;
		CropInfo cropInfo = SingletonDict<string, CropInfo>.Get(key);
		EncyclopediaItem encyclopediaItem = EncyclopediaItems.Get(EncyclopediaType.Farming, key);
		bool flag = data.CurrentLevel == 0;
		bool flag2 = encyclopediaItem != null && data.CurrentLevel >= encyclopediaItem.MaxLevel;
		bool flag3 = true;
		int num = 0;
		KeyValuePair<int, KeyValuePair<string, float>[][]>[] array = encyclopediaItem?.GetMasteryModifiersList();
		if (array != null)
		{
			KeyValuePair<int, KeyValuePair<string, float>[][]>[] array2 = array;
			foreach (KeyValuePair<int, KeyValuePair<string, float>[][]> keyValuePair in array2)
			{
				flag3 = false;
				if (keyValuePair.Key <= data.CurrentLevel)
				{
					num++;
				}
			}
		}
		if (data.MasteryLevelToIndex != null)
		{
			foreach (KeyValuePair<int, int> item in data.MasteryLevelToIndex)
			{
				if (item.Key <= data.CurrentLevel)
				{
					num--;
				}
			}
		}
		bool flag4 = num > 0;
		if (cropInfo == null)
		{
			_iconTexture.SetIcon(string.Empty);
		}
		else if (flag)
		{
			_iconTexture.HideShadow = true;
			_iconTexture.SetIcon(cropInfo.Icon, new ItemColor(new Color(0f, 0f, 0f, 0.75f)));
		}
		else
		{
			_iconTexture.HideShadow = false;
			_iconTexture.SetIcon(cropInfo.Icon, cropInfo.ColorR, cropInfo.ColorG, cropInfo.ColorB);
		}
		int num2 = data.NextLevelExpThreshold - data.CurrentLevelExpThreshold;
		_progressSprite.fillAmount = ((num2 <= 0) ? 0f : ((float)(data.CurrentExp - data.CurrentLevelExpThreshold) / (float)num2));
		_progressSprite.color = ((!flag2) ? ((Color)new Color32(59, 96, 123, byte.MaxValue)) : PresetColor.UIYellow);
		_titleLabel.text = ((cropInfo != null) ? cropInfo.Name.ToString() : key);
		_infoLabel.text = ((!flag) ? LocalizeUtil.FormatLevel(data.CurrentLevel) : T._("미습득"));
		if (flag3)
		{
			_masteryList.Clear();
			_masteryLabel.text = string.Format("<weak>{0}</weak>", T._("특성 없음"));
			return;
		}
		if (flag4)
		{
			_masteryList.Clear();
			_masteryLabel.text = string.Format("<em>{0}</em>", T._("특성 선택 가능!"));
			return;
		}
		_masteryList.BeginLoad();
		if (data.MasteryLevelToIndex != null && encyclopediaItem != null)
		{
			using Reusable<List<int>> reusable = ReusableList<int>.Pop();
			List<int> value = reusable.Value;
			value.AddRange(data.MasteryLevelToIndex.Keys);
			value.Sort((int k1, int k2) => k1 - k2);
			foreach (int item2 in value)
			{
				KeyValuePair<string, float>[][] masteryModifiers = encyclopediaItem.GetMasteryModifiers(item2);
				if (masteryModifiers == null)
				{
					continue;
				}
				int num3 = data.MasteryLevelToIndex[item2];
				if (num3 < KUtility.GetSize(masteryModifiers))
				{
					KeyValuePair<string, float> keyValuePair2 = masteryModifiers[num3].FirstOrDefault();
					EncyclopediaModifiers encyclopediaModifiers = ((!string.IsNullOrEmpty(keyValuePair2.Key)) ? SingletonDict<string, EncyclopediaModifiers>.Get(keyValuePair2.Key) : null);
					if (encyclopediaModifiers != null)
					{
						_masteryList.GetNext().transform.Find("Icon").GetComponent<UISprite>().spriteName = encyclopediaModifiers.Icon;
					}
				}
			}
		}
		_masteryList.EndLoad();
		if (_masteryList.Count > 0)
		{
			_masteryLabel.text = null;
			UIUtility.WidgetsReposition(_masteryList, Vector3.right, Vector3.zero, 5f, 0.5f);
		}
		else
		{
			_masteryLabel.text = string.Format("<weak>[icon=icon_item_lock] {0}</weak>", T._("특성 잠김"));
		}
	}

	private void OnClick()
	{
		if (this.Clicked != null)
		{
			this.Clicked(Key);
		}
	}
}
