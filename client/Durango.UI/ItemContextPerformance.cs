using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using APNGLib;
using Building;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.Social;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Item;
using Shared.MessageBoard;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ItemContextPerformance : ItemContextBase
{
	private struct SimpleValueAttribute
	{
		public readonly string Text;

		public readonly string Value;

		private readonly int _order;

		public SimpleValueAttribute(string text, string value, int order)
		{
			Text = text;
			Value = value;
			_order = ((order <= 0) ? int.MaxValue : order);
		}

		public static int Compare(SimpleValueAttribute x, SimpleValueAttribute y)
		{
			return x._order - y._order;
		}
	}

	private static readonly StringBuilder DigitFormat = new StringBuilder();

	[SerializeField]
	private UIWidget _simpleValueContainer;

	[SerializeField]
	private KeyValueLabel _simpleValueBase;

	[SerializeField]
	private UIWidget _descriptionContainer;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UIWidget _colorsContainer;

	[SerializeField]
	private ListObjectPool _colors;

	[SerializeField]
	private UIWidget _canvasContainer;

	[SerializeField]
	private KeyValueLabel _canvasText;

	[SerializeField]
	private ApngTexture _canvasViewer;

	[SerializeField]
	private UIWidget _statusEffectsContainer;

	[SerializeField]
	private ListObjectPool _statusEffects;

	[SerializeField]
	private UIWidget _skillsContainer;

	[SerializeField]
	private UILabel _skillsLabel;

	[SerializeField]
	private ListObjectPool _skillsPool;

	[SerializeField]
	private UISprite _separatorBase;

	private ListObjectPool<KeyValueLabel> _simpleValues;

	private ListObjectPool<UISprite> _separators;

	private readonly List<SimpleValueAttribute> _attributeList = new List<SimpleValueAttribute>();

	private readonly List<Pair<string, int>> _effectOnList = new List<Pair<string, int>>();

	private UIWidget[] _widgets;

	private bool _hasCanvas;

	private bool _hasSkills;

	private bool _hasDescription;

	private ItemData _item;

	private Messages.Pet? _pet;

	public override void Init()
	{
		base.Init();
		_simpleValues = new ListObjectPool<KeyValueLabel>();
		_simpleValues.BaseObject = _simpleValueBase;
		_simpleValues.UseBase = true;
		_separators = new ListObjectPool<UISprite>();
		_separators.BaseObject = _separatorBase;
		_separators.UseBase = true;
		_colors.Init(delegate(GameObject obj)
		{
			UIEventListener uIEventListener3 = UIEventListener.Get(obj);
			uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnClickColorObject));
			UIEventListener uIEventListener4 = UIEventListener.Get(obj);
			uIEventListener4.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener4.onHover, (UIEventListener.BoolDelegate)delegate(GameObject go, bool isHover)
			{
				if (isHover)
				{
					OnClickColorObject(go);
				}
				else
				{
					UIManager.Popup.Tooltip<WidgetTooltipControl>().Hide();
				}
			});
		});
		_statusEffects.Init(delegate(GameObject obj)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(obj);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickStatusEffectObject));
		});
		_skillsPool.Init(delegate(GameObject obj)
		{
			UIEventListener uIEventListener = UIEventListener.Get(obj);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickSkillObject));
		});
		base.HeaderText = T._("정보");
		_widgets = new UIWidget[6] { _statusEffectsContainer, _canvasContainer, _skillsContainer, _simpleValueContainer, _colorsContainer, _descriptionContainer };
	}

	public void Set([NotNull] ItemData item)
	{
		_item = item;
		_pet = null;
		BeginLoad();
		Prototype prototype = _item.Prototype;
		if (item.HasAttribute("slot"))
		{
			bool flag = CheckEquipablity(_item, prototype);
			AddKeyValueInfo(string.Format((!flag) ? "[DD5C56]{0}[-]" : "[FFFFFF]{0}[-]", T._("착용 가능 레벨")), string.Format((!flag) ? "[DD5C56]{0}[-]" : "[FFFFFF]{0}[-]", prototype?.MinLevel ?? _item.Level));
		}
		FillEffectOn(item);
		FillData(item.EmotionalMotions);
		int i = 0;
		for (int size = KUtility.GetSize(item.Performances); i < size; i++)
		{
			Performance data = item.Performances[i];
			FillData(data);
		}
		FillData(item.Capsule);
		if (prototype == null || !prototype.HidingColor)
		{
			FillData(item.Colors);
		}
		FillDescription(item);
		EndLoad();
	}

	public void Set(Messages.Pet pet)
	{
		_item = null;
		_pet = pet;
		BeginLoad();
		FillData(pet);
		FillSkill(pet);
		EndLoad();
	}

	private void BeginLoad()
	{
		_simpleValues.BeginLoad();
		_colors.BeginLoad();
		_statusEffects.BeginLoad();
		_hasCanvas = false;
		_hasSkills = false;
		_hasDescription = false;
	}

	private void EndLoad()
	{
		_simpleValues.EndLoad();
		_colors.EndLoad();
		_statusEffects.EndLoad();
		_simpleValueContainer.gameObject.SetActive(_simpleValues.Count > 0);
		_colorsContainer.gameObject.SetActive(_colors.Count > 0);
		_statusEffectsContainer.gameObject.SetActive(_statusEffects.Count > 0);
		_canvasContainer.gameObject.SetActive(_hasCanvas);
		_skillsContainer.gameObject.SetActive(_hasSkills);
		_descriptionContainer.gameObject.SetActive(_hasDescription);
		int num = 0;
		for (int i = 0; i < _widgets.Length; i++)
		{
			if (_widgets[i].gameObject.activeSelf)
			{
				num++;
			}
		}
		if (num == 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		if (_statusEffectsContainer.gameObject.activeSelf)
		{
			UIWidget component = _statusEffects.BaseObject.GetComponent<UIWidget>();
			Vector2 vector = UIUtility.WidgetsGridReposition(_statusEffects, null, Vector2.down, _statusEffectsContainer.localCorners[1], _statusEffectsContainer.width, component.GetSize(), 0f, 0f);
			_statusEffectsContainer.height = (int)vector.y;
		}
		if (_canvasContainer.gameObject.activeSelf)
		{
			_canvasText.UpdateLayout(_body.width);
			float num = _canvasText.Widget.height;
			if (_canvasViewer.gameObject.activeSelf)
			{
				num = Mathf.Max(num, _canvasViewer.GetComponent<UIWidget>().height);
			}
			_canvasContainer.height = (int)num;
		}
		if (_skillsContainer.gameObject.activeSelf)
		{
			float num2 = UIUtility.WidgetsReposition(_skillsPool, Vector3.down, new Vector3(120f, 0f), 16f);
			_skillsContainer.height = (int)num2;
		}
		int i = 0;
		for (int count = _simpleValues.Count; i < count; i++)
		{
			KeyValueLabel keyValueLabel = _simpleValues[i];
			keyValueLabel.UpdateLayout(_body.width);
		}
		if (_simpleValueContainer.gameObject.activeSelf)
		{
			Vector3[] array = _simpleValueContainer.localCorners;
			float num3 = UIUtility.WidgetsReposition(_simpleValues, Vector3.down, Vector3.Lerp(array[1], array[2], 0.5f), 16f);
			_simpleValueContainer.height = (int)num3;
		}
		if (_colorsContainer.gameObject.activeSelf)
		{
			Vector3[] array2 = _colorsContainer.localCorners;
			for (int j = 0; j < _colors.Count; j++)
			{
				UIWidget component2 = _colors[j].GetComponent<UIWidget>();
				switch (j)
				{
				case 0:
					component2.SetPosition(array2[1] + Vector3.right * 20f, 0f, 1f);
					break;
				case 1:
					component2.SetPosition(Vector3.Lerp(array2[1], array2[2], 0.5f), 0.5f, 1f);
					break;
				case 2:
					component2.SetPosition(array2[2] + Vector3.left * 20f, 1f, 1f);
					break;
				}
			}
		}
		Vector3[] array3 = _body.localCorners;
		float num4 = UIUtility.WidgetsReposition(_widgets, Vector3.down, Vector3.Lerp(array3[1], array3[2], 0.5f) + Vector3.down * 12f, 26f);
		_body.height = (int)(num4 + 24f);
		_separators.BeginLoad();
		int num5 = 0;
		for (int k = 0; k < _widgets.Length; k++)
		{
			if (UIUtility.IsVisibleWidget(_widgets[k]))
			{
				if (num5 > 0)
				{
					UISprite next = _separators.GetNext();
					next.transform.localPosition = _widgets[k].GetPosition(0.5f, 1f) + Vector3.up * 26f * 0.5f;
				}
				num5++;
			}
		}
		_separators.EndLoad();
	}

	private void AddKeyValueInfo(string key, string value)
	{
		KeyValueLabel next = _simpleValues.GetNext();
		next.Set(key, value);
	}

	private void FillData(Performance data)
	{
		if (data.IsEmpty())
		{
			return;
		}
		Dictionary<string, PerformanceVisibleInfo> dictionary = SingletonDict<string, Dictionary<string, PerformanceVisibleInfo>>.Get(data.Id);
		if (dictionary == null)
		{
			return;
		}
		_attributeList.Clear();
		foreach (KeyValuePair<string, float> num in data.Nums)
		{
			SimpleValueAttribute attr;
			if (data.Id == "modifiers")
			{
				if (CreateModifierAttribute(num.Key, num.Value, out attr))
				{
					_attributeList.Add(attr);
				}
			}
			else
			{
				if (!dictionary.TryGetValue(num.Key, out var value))
				{
					continue;
				}
				PerformanceVisibleType type = value.Type;
				if (type == PerformanceVisibleType.Ratio)
				{
					if (CreateRatioAttribute(num.Key, num.Value, value, out attr))
					{
						_attributeList.Add(attr);
					}
				}
				else if (CreateNumberAttribute(num.Key, num.Value, value, out attr))
				{
					_attributeList.Add(attr);
				}
			}
		}
		foreach (KeyValuePair<string, string> str in data.Strs)
		{
			if (dictionary.TryGetValue(str.Key, out var value2) && CreateStringAttribute(str.Key, str.Value, value2, out var attr2))
			{
				_attributeList.Add(attr2);
			}
		}
		_attributeList.Sort(SimpleValueAttribute.Compare);
		for (int i = 0; i < _attributeList.Count; i++)
		{
			AddKeyValueInfo(_attributeList[i].Text, _attributeList[i].Value);
		}
		_attributeList.Clear();
	}

	private bool CheckEquipablity(ItemData item, Prototype prototype)
	{
		int num = prototype?.MinLevel ?? item.Level;
		int level = GameSystem<StatisticsSystem>.Instance().Level;
		return level >= num;
	}

	private static bool CreateNumberAttribute(string name, float value, PerformanceVisibleInfo visibleInfo, out SimpleValueAttribute attr)
	{
		string localizedAttributeName = GetLocalizedAttributeName(name);
		if (localizedAttributeName == null)
		{
			attr = default(SimpleValueAttribute);
			return false;
		}
		if (visibleInfo.MinValue != 0f && (visibleInfo.MinValue > value || value == 0f))
		{
			attr = default(SimpleValueAttribute);
			return false;
		}
		float num = Mathf.Pow(10f, visibleInfo.Digits);
		float num2 = Mathf.Floor(value * num);
		num2 /= num;
		DigitFormat.Length = 0;
		DigitFormat.Append('{');
		DigitFormat.AppendFormat("0:N{0}", visibleInfo.Digits);
		DigitFormat.Append('}');
		attr = new SimpleValueAttribute(localizedAttributeName, T._(DigitFormat.ToString(), num2), visibleInfo.Order);
		return true;
	}

	private static bool CreateRatioAttribute(string name, float value, PerformanceVisibleInfo visibleInfo, out SimpleValueAttribute attr)
	{
		string localizedAttributeName = GetLocalizedAttributeName(name);
		if (localizedAttributeName == null)
		{
			attr = default(SimpleValueAttribute);
			return false;
		}
		if (visibleInfo.MinValue != 0f && (visibleInfo.MinValue > value || value == 0f))
		{
			attr = default(SimpleValueAttribute);
			return false;
		}
		float num = Mathf.Pow(10f, visibleInfo.Digits + 2);
		float num2 = Mathf.Floor(value * num);
		num2 /= num;
		DigitFormat.Length = 0;
		DigitFormat.Append('{');
		DigitFormat.AppendFormat("0:P{0}", visibleInfo.Digits);
		DigitFormat.Append('}');
		attr = new SimpleValueAttribute(localizedAttributeName, T._(DigitFormat.ToString(), num2), visibleInfo.Order);
		return true;
	}

	private static bool CreateModifierAttribute(string key, float value, out SimpleValueAttribute attr)
	{
		if (value == 0f)
		{
			attr = default(SimpleValueAttribute);
			return false;
		}
		SkillModifier skillModifier = SingletonDict<string, SkillModifier>.Get(key);
		if (skillModifier == null)
		{
			attr = new SimpleValueAttribute(key, value.ToString("0.#"), 0);
		}
		else
		{
			attr = new SimpleValueAttribute(skillModifier.Name, skillModifier.GetValueString(value), 0);
		}
		return true;
	}

	private static bool CreateStringAttribute(string name, string value, PerformanceVisibleInfo visibleInfo, out SimpleValueAttribute attr)
	{
		string localizedAttributeName = GetLocalizedAttributeName(name);
		if (localizedAttributeName == null)
		{
			attr = default(SimpleValueAttribute);
			return false;
		}
		attr = new SimpleValueAttribute(localizedAttributeName, GetLocalizedAttributeValue(value), visibleInfo.Order);
		return true;
	}

	private static string GetLocalizedAttributeName(string attrName)
	{
		string key = $"#attribute_{attrName}";
		return (!LocalizeSystem.Has(key)) ? null : LocalizeSystem.Get(key);
	}

	private static string GetLocalizedAttributeValue(string attrValue)
	{
		string key = $"#attribute_value_{attrValue}";
		return LocalizeSystem.Get(key);
	}

	private void FillData(string[] emotions)
	{
		if (KUtility.GetSize(emotions) == 0)
		{
			return;
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		value.Append(T._("전용 감정 표현"));
		Emotional emotional = GameSystem<SocialSystem>.Instance().Emotional;
		foreach (string text in emotions)
		{
			Durango.Logic.Social.Motion motion = emotional.GetMotion(text);
			value.Append("<br>20</br>");
			if (UIUtility.FindComponentInParent<TooltipBase>(base.gameObject) != null)
			{
				value.AppendFormat("[preset=rect_box?<em>[icon=icon_mainhud_equip] {0}</em>]", (motion != null) ? motion.Name : text);
			}
			else
			{
				value.AppendFormat("<ref_button>ui://Emotion/MotionPreview/{1},<em>[icon=icon_mainhud_equip] {0}</em></ref_button>", (motion != null) ? motion.Name : text, text);
			}
		}
		AddKeyValueInfo(value.ToString(), null);
	}

	private void FillData(Messages.Pet pet)
	{
		PetStats stat = pet.Stat;
		AddKeyValueInfo(T._("크기"), stat.Size.ToString());
		AddKeyValueInfo(T._("가방"), pet.Statistics.DerivedAbilities.Get(Derived.InventoryCapacity, 0f).ToString("0"));
		AddKeyValueInfo(T._("이동속도"), pet.Statistics.DerivedAbilities.Get(Derived.Speed, 0f).ToString("0"));
		AddKeyValueInfo(T._("생명"), ((int)((stat.Life != null) ? stat.Life.Max() : 0f)).ToString());
		AddKeyValueInfo(T._("활력"), ((int)((stat.Hungry != null) ? stat.Hungry.Max() : 0f)).ToString());
		AddKeyValueInfo(T._("공격"), pet.Statistics.DerivedAbilities.Get(Derived.Attack, 0f).ToString("0"));
		AddKeyValueInfo(T._("방어"), pet.Statistics.DerivedAbilities.Get(Derived.Defense, 0f).ToString("0"));
		AddKeyValueInfo(T._("명중"), pet.Statistics.DerivedAbilities.Get(Derived.Accuracy, 0f).ToString("0"));
	}

	private void FillData([CanBeNull] ArtifactCapsule? info)
	{
		if (!info.HasValue)
		{
			return;
		}
		ArtifactCapsule value = info.Value;
		AddKeyValueInfo(T._("레벨"), LocalizeUtil.FormatLevel(value.ArtifactLevel));
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(value.BlueprintId);
		if (blueprint == null)
		{
			return;
		}
		Point2 point = blueprint.Size;
		if (value.OccupySize.HasValue)
		{
			point = value.OccupySize.Value;
		}
		AddKeyValueInfo(T._("면적"), $"{point.x} × {point.y}");
		if (value.Display.Parts != null && blueprint.Slots != null)
		{
			Building.BlueprintSlot[] slots = blueprint.Slots;
			foreach (Building.BlueprintSlot blueprintSlot in slots)
			{
				if (!blueprintSlot.HasLook || string.IsNullOrEmpty(blueprintSlot.Name) || !value.Display.Parts.TryGetValue(blueprintSlot.Id, out var value2))
				{
					continue;
				}
				ArtifactLook artifactLook = null;
				foreach (KeyValuePair<string, ArtifactLook> look in blueprintSlot.Looks)
				{
					if (look.Value.model_key != value2)
					{
						continue;
					}
					artifactLook = look.Value;
					break;
				}
				if (artifactLook != null && !string.IsNullOrEmpty(artifactLook.name))
				{
					AddKeyValueInfo(blueprintSlot.Name, artifactLook.name);
				}
			}
		}
		ArtifactState state = value.State;
		if (!state.Scribble.HasValue)
		{
			return;
		}
		ScribbleContent value3 = state.Scribble.Value;
		switch (value3.Type)
		{
		case Drawing.Text:
			_canvasText.Set(T._("글"), Encoding.UTF8.GetString(value3.Data).Trim());
			_canvasViewer.gameObject.SetActive(value: false);
			_hasCanvas = true;
			break;
		case Drawing.Canvas:
		{
			_canvasText.Set(T._("그림"), null);
			_canvasViewer.gameObject.SetActive(value: true);
			APNG aPNG = new APNG();
			using (MemoryStream stream = new MemoryStream(value3.Data))
			{
				aPNG.Load(stream);
			}
			_canvasViewer.Set(aPNG);
			_hasCanvas = true;
			break;
		}
		}
	}

	private void FillEffectOn([NotNull] ItemData item)
	{
		_effectOnList.Clear();
		foreach (Pair<string, int> statusEffect in item.GetStatusEffects())
		{
			StatusEffectTemplate statusEffectTemplate = StatusEffectTemplateYaml.GetStatusEffectTemplate(statusEffect.Item1, statusEffect.Item2);
			if (statusEffectTemplate != null)
			{
				_effectOnList.Add(statusEffect);
				GameObject next = _statusEffects.GetNext();
				UISprite component = next.transform.Find("icon").GetComponent<UISprite>();
				UISprite component2 = next.transform.Find("arrow").GetComponent<UISprite>();
				component.spriteName = statusEffectTemplate.Icon;
				Color color;
				string spriteName;
				switch (statusEffectTemplate.IconColor)
				{
				case "negative":
					color = PresetColor.UIDebuff;
					spriteName = "icon_se_decr";
					break;
				case "positive":
					color = PresetColor.UIBuff;
					spriteName = "icon_se_incr";
					break;
				default:
					color = Color.clear;
					spriteName = string.Empty;
					break;
				}
				if (color.a > 0f)
				{
					component2.color = color;
					component2.spriteName = spriteName;
				}
				else
				{
					component2.alpha = 0f;
				}
			}
		}
	}

	private void FillData(ItemColor colors)
	{
		if (!colors.HasValue)
		{
			return;
		}
		for (int i = 0; i < colors.Count; i++)
		{
			Color color = colors[i];
			if (!(color == Color.clear))
			{
				GameObject next = _colors.GetNext();
				UISprite component = next.transform.Find("upper").GetComponent<UISprite>();
				component.color = color;
			}
		}
	}

	private void FillDescription(ItemData item)
	{
		if (!string.IsNullOrEmpty(item.Description))
		{
			_hasDescription = true;
			_descriptionLabel.text = item.Description;
			_descriptionContainer.height = _descriptionLabel.height;
		}
	}

	private void FillSkill(Messages.Pet pet)
	{
		Messages.PetActiveSkill[] availableActiveSkill = pet.Statistics.AvailableActiveSkill;
		if (KUtility.GetSize(availableActiveSkill) == 0)
		{
			return;
		}
		_hasSkills = true;
		_skillsLabel.text = T._("특수 행동");
		_skillsPool.BeginLoad();
		Messages.PetActiveSkill[] array = availableActiveSkill;
		for (int i = 0; i < array.Length; i++)
		{
			Messages.PetActiveSkill petActiveSkill = array[i];
			Yaml.PetActiveSkill petActiveSkill2 = PetActiveSkills.Get(petActiveSkill.SkillId, petActiveSkill.Rank);
			if (petActiveSkill2 != null)
			{
				GameObject next = _skillsPool.GetNext();
				UISprite component = next.transform.Find("Icon").GetComponent<UISprite>();
				component.spriteName = petActiveSkill2.Icon;
				UILabel component2 = next.transform.Find("Name").GetComponent<UILabel>();
				component2.text = petActiveSkill2.Name;
			}
		}
		_skillsPool.EndLoad();
	}

	private static void OnClickColorObject(GameObject obj)
	{
		UISprite component = obj.transform.Find("upper").GetComponent<UISprite>();
		Color32 color = component.color;
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, $"R: {color.r}\nG: {color.g}\nB: {color.b}");
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Sign = 1;
		widgetTooltipControl.Show(component, Vector2.zero, 3600f);
	}

	private void OnClickStatusEffectObject(GameObject obj)
	{
		int num = _statusEffects.IndexOf(obj);
		if (num != -1 && num >= 0 && num < _effectOnList.Count)
		{
			Pair<string, int> pair = _effectOnList[num];
			StatusEffectTemplate statusEffectTemplate = StatusEffectTemplateYaml.GetStatusEffectTemplate(pair.Item1, pair.Item2);
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			string title = $"<em>{statusEffectTemplate.Name}</em>";
			StringBuilder stringBuilder = new StringBuilder(statusEffectTemplate.Description);
			string value = ((statusEffectTemplate.Effects != null) ? Durango.Logic.StatusEffect.EffectsText(statusEffectTemplate.GetEffects(pair.Item2)) : null);
			if (!string.IsNullOrEmpty(value))
			{
				stringBuilder.Append("\n\n<em>");
				stringBuilder.Append(value);
				stringBuilder.Append("</em>");
			}
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Sign = 1;
			widgetTooltipControl.Set(title, stringBuilder.ToString().Trim(), 500);
			widgetTooltipControl.Show(10f);
		}
	}

	private void OnClickSkillObject(GameObject obj)
	{
		int num = _skillsPool.IndexOf(obj);
		Messages.Pet? pet = _pet;
		if (!pet.HasValue || num < 0)
		{
			return;
		}
		Messages.PetActiveSkill[] availableActiveSkill = _pet.Value.Statistics.AvailableActiveSkill;
		if (KUtility.GetSize(availableActiveSkill) > num)
		{
			Yaml.PetActiveSkill petActiveSkill = PetActiveSkills.Get(availableActiveSkill[num].SkillId, availableActiveSkill[num].Rank);
			if (petActiveSkill != null)
			{
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				widgetTooltipControl.Set($"<em>{petActiveSkill.Name}</em>", petActiveSkill.Description, 400);
				widgetTooltipControl.Show(5f);
			}
		}
	}
}
