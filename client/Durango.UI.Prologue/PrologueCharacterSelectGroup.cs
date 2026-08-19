using System.Collections.Generic;
using System.Text;
using Durango.Logic.Skill;
using Durango.UI.Control;
using L10N;
using Shared.Skill;
using UnityEngine;
using Yaml;

namespace Durango.UI.Prologue;

public class PrologueCharacterSelectGroup : PrologueCharacterSelectGroupBase
{
	[SerializeField]
	private UILabel _categoryTitleLabel;

	[SerializeField]
	private SimpleContainer _changeCostumeBtn;

	private SimpleContainer _cancelBtn;

	protected override void Awake()
	{
		base.Awake();
		_cancelBtn = _changeCostumeBtn.transform.parent.gameObject.AddChild(_changeCostumeBtn.gameObject).GetComponent<SimpleContainer>();
		UIWidget component = _changeCostumeBtn.GetComponent<UIWidget>();
		Vector3 localPosition = _changeCostumeBtn.transform.localPosition;
		_changeCostumeBtn.transform.localPosition = localPosition + Vector3.up * ((float)component.height * 0.5f);
		_cancelBtn.transform.localPosition = localPosition + Vector3.down * ((float)component.height * 0.5f);
		_changeCostumeBtn.Set("Color", PresetColor.UIWhite);
		_changeCostumeBtn.Set("SelectColor", PresetColor.UIYellow);
		_changeCostumeBtn.Get<UILabel>("Label").text = T._("외형 변경");
		_changeCostumeBtn.Get<UISprite>("Icon").spriteName = "icon_changelook";
		_changeCostumeBtn.Get<UISprite>("Icon").MakePixelPerfect();
		UIEventListener.Get(_changeCostumeBtn.gameObject).onPress = OnTouchButtons;
		UIEventListener.Get(_changeCostumeBtn.gameObject).onClick = base.OnChangeCharacterCostume;
		OnTouchButtons(_changeCostumeBtn.gameObject, press: false);
		_cancelBtn.Set("Color", PresetColor.UIWhite);
		_cancelBtn.Set("SelectColor", PresetColor.UIYellow);
		_cancelBtn.Get<UILabel>("Label").text = T._("다른 캐릭터");
		_cancelBtn.Get<UISprite>("Icon").spriteName = "icon_back";
		_cancelBtn.Get<UISprite>("Icon").MakePixelPerfect();
		UIEventListener.Get(_cancelBtn.gameObject).onPress = OnTouchButtons;
		UIEventListener.Get(_cancelBtn.gameObject).onClick = OnCancelSelectCharacter;
		OnTouchButtons(_cancelBtn.gameObject, press: false);
	}

	private void OnTouchButtons(GameObject go, bool press)
	{
		SimpleContainer component = go.GetComponent<SimpleContainer>();
		if (!(component == null))
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

	protected override void OnSetSelectCharacterInfo(Job data)
	{
		if (data == null)
		{
			_categoryTitleLabel.text = string.Empty;
			return;
		}
		if (KUtility.GetSize(data.category_levels) == 0)
		{
			_categoryTitleLabel.text = T._("스킬 없음");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<Shared.Skill.Category, int> category_level in data.category_levels)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(", ");
			}
			Shared.Skill.Category key = category_level.Key;
			stringBuilder.AppendFormat("[icon={0}:1.5]", Util.CategoryIcon(key));
			stringBuilder.Append(T._("{1:lv:} {0}", Util.CategoryLocalizeName(key), category_level.Value));
		}
		_categoryTitleLabel.text = stringBuilder.ToString().Trim();
	}
}
