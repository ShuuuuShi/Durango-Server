using System;
using System.Collections.Generic;
using System.Text;
using L10N;
using Shared.Player;
using Shared.Skill;
using SkillData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class PrologueCharacterSelectGroup : UIBase
{
	public Action OnSubmit;

	public Action OnChangeCostume;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private UISpriteLabel _categoryTitleLabel;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UIWidget[] _lines;

	[SerializeField]
	private DefaultSelectableButton _selectBtn;

	[SerializeField]
	private SimpleContainer _changeCostumeBtn;

	private SimpleContainer _cancelBtn;

	private List<int> _margins;

	private UIWidget[] _widgets;

	private void Awake()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		_cancelBtn = ((Component)((Component)_changeCostumeBtn).transform.parent).gameObject.AddChild(((Component)_changeCostumeBtn).gameObject).GetComponent<SimpleContainer>();
		UIWidget component = ((Component)_changeCostumeBtn).GetComponent<UIWidget>();
		Vector3 localPosition = ((Component)_changeCostumeBtn).transform.localPosition;
		((Component)_changeCostumeBtn).transform.localPosition = localPosition + Vector3.up * ((float)component.height * 0.5f);
		((Component)_cancelBtn).transform.localPosition = localPosition + Vector3.down * ((float)component.height * 0.5f);
		_changeCostumeBtn.Set("Color", UIManager.UIWhite);
		_changeCostumeBtn.Set("SelectColor", UIManager.UIYellow);
		_changeCostumeBtn.Get<UILabel>("Label").text = T._("외형 변경");
		_changeCostumeBtn.Get<UISprite>("Icon").spriteName = "icon_changelook";
		_changeCostumeBtn.Get<UISprite>("Icon").MakePixelPerfect();
		UIEventListener.Get(((Component)_changeCostumeBtn).gameObject).onPress = OnTouchButtons;
		UIEventListener.Get(((Component)_changeCostumeBtn).gameObject).onClick = OnChangeCharacterCostume;
		OnTouchButtons(((Component)_changeCostumeBtn).gameObject, press: false);
		_cancelBtn.Set("Color", UIManager.UIRed);
		_cancelBtn.Set("SelectColor", UIManager.UIYellow);
		_cancelBtn.Get<UILabel>("Label").text = T._("다른 캐릭터");
		_cancelBtn.Get<UISprite>("Icon").spriteName = "icon_back";
		_cancelBtn.Get<UISprite>("Icon").MakePixelPerfect();
		UIEventListener.Get(((Component)_cancelBtn).gameObject).onPress = OnTouchButtons;
		UIEventListener.Get(((Component)_cancelBtn).gameObject).onClick = OnCancelSelectCharacter;
		OnTouchButtons(((Component)_cancelBtn).gameObject, press: false);
		_selectBtn.Clicked = OnSelectCharacter;
		OnClose();
	}

	private void OnTouchButtons(GameObject go, bool press)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		SimpleContainer component = go.GetComponent<SimpleContainer>();
		if (!((Object)(object)component == (Object)null))
		{
			if (press)
			{
				Color value = component.GetValue<Color>("SelectColor");
				component.Get<UISprite>("BG").color = value;
				component.Get<UISprite>("Icon").color = value;
				component.Get<UISprite>("LabelBG").color = value;
			}
			else
			{
				Color value2 = component.GetValue<Color>("Color");
				component.Get<UISprite>("BG").color = value2;
				component.Get<UISprite>("Icon").color = value2;
				component.Get<UISprite>("LabelBG").color = value2;
			}
		}
	}

	private void OnSelectCharacter()
	{
		if (OnSubmit != null)
		{
			OnSubmit();
		}
	}

	private void OnCancelSelectCharacter(GameObject go)
	{
		Close();
	}

	private void OnChangeCharacterCostume(GameObject go)
	{
		if (OnChangeCostume != null)
		{
			OnChangeCostume();
		}
	}

	private void InitLabelsMargin()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (_margins == null)
		{
			_widgets = new UIWidget[6]
			{
				_nameLabel,
				_commentLabel,
				_lines[0],
				_categoryTitleLabel.Label,
				_descriptionLabel,
				_lines[1]
			};
			_margins = new List<int>();
			UIWidget uIWidget = null;
			for (int i = 0; i < _widgets.Length; i++)
			{
				UIWidget uIWidget2 = _widgets[i];
				int item = (int)Mathf.Abs((!((Object)(object)uIWidget == (Object)null)) ? (uIWidget.GetPosition(0f, 0f).y - uIWidget2.GetPosition(0f, 1f).y) : uIWidget2.GetPosition(0f, 1f).y);
				_margins.Add(item);
				uIWidget = uIWidget2;
			}
		}
	}

	public void SetSelectCharactInfo(Shared.Player.Job job, bool male)
	{
		InitLabelsMargin();
		string key = LocalizeUtil.GetKey(job);
		_nameLabel.text = LocalizeSystem.Get(key);
		_commentLabel.text = LocalizeSystem.Get(string.Format("{0}_{1}_description", key, (!male) ? "f" : "m"));
		Yaml.Job job2 = SingletonDict<Shared.Player.Job, Yaml.Job>.Get(job);
		if (job2 == null)
		{
			_categoryTitleLabel.text = string.Empty;
			_descriptionLabel.text = string.Empty;
		}
		else
		{
			if (KUtility.GetSize(job2.category_levels) == 0)
			{
				_categoryTitleLabel.text = T._("스킬 없음");
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<Category, int> category_level in job2.category_levels)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(", ");
					}
					Category key2 = category_level.Key;
					stringBuilder.AppendFormat("[{0}:1.5]", SkillUtil.CategoryIcon(key2));
					stringBuilder.Append(T._("{1:lv:} {0}", SkillUtil.CategoryLocalizeName(key2), category_level.Value));
				}
				_categoryTitleLabel.text = stringBuilder.ToString().Trim();
			}
			_descriptionLabel.text = job2.description;
		}
		LabelsReposition();
	}

	private void LabelsReposition()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		UIWidget uIWidget = null;
		for (int i = 0; i < _widgets.Length; i++)
		{
			UIWidget uIWidget2 = _widgets[i];
			float num = ((!((Object)(object)uIWidget == (Object)null)) ? uIWidget.GetPosition(0f, 0f).y : 0f);
			Vector3 localPosition = ((Component)uIWidget2).transform.localPosition;
			localPosition.y = num - (float)_margins[i];
			((Component)uIWidget2).transform.localPosition = localPosition;
			uIWidget = uIWidget2;
		}
	}
}
