using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using L10N;
using MsgPack;
using Player;
using Shared.Player;
using Shared.Skill;
using SkillData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class CreateCharacterPanel : MonoBehaviour
{
	private enum State
	{
		FirstState = 0,
		SelectGender = 0,
		SelectPrevJob = 1,
		SelectCostume = 2,
		SelectName = 3
	}

	private enum BodyModelType
	{
		Normal,
		Torn,
		Nothing
	}

	public const string TornBodyFormat = "{0}_torn";

	public const string NothingBodyModel = "inner_basic";

	public static readonly string[] NormalBodyModels = new string[8] { "body_engineer", "body_officelook", "body_school", "body_farmer", "body_waiter", "body_soldier", "body_house", "body_hoody" };

	private State _curState;

	[SerializeField]
	private DefaultSelectableButton[] _genderButtons;

	[SerializeField]
	private DefaultSelectableButton _prevJobButtonBase;

	private DefaultSelectableButton[] _prevJobButtons;

	[SerializeField]
	private UISpriteLabel _skillCategoryTitleLabel;

	[SerializeField]
	private UILabel _skillCategoryDescriptionLabel;

	[SerializeField]
	private int _prevJobButtonWidth;

	[SerializeField]
	private GameObject _previewTouchBox;

	[SerializeField]
	private PrologueCustomButton _randomBtn;

	[SerializeField]
	private PrologueCustomButton _hairShapeBtn;

	[SerializeField]
	private PrologueCustomButton _breadShapeBtn;

	[SerializeField]
	private PrologueCustomButton _voiceBtn;

	[SerializeField]
	private PrologueCustomButton _hairColorBtn;

	[SerializeField]
	private PrologueCustomButton _skinColorBtn;

	[SerializeField]
	private PrologueCustomButton[] _costumeColorBtns;

	[SerializeField]
	private PrologueCustomButton _eyeColorBtn;

	[SerializeField]
	private PrologueCustomButton _lipColorBtn;

	[SerializeField]
	private DefaultSelectableButton _tornBodyButton;

	[SerializeField]
	private DefaultSelectableButton _nothingBodyButton;

	[SerializeField]
	private UITexture _portraitWidget;

	[SerializeField]
	private GameObject _portraitNext;

	[SerializeField]
	private GameObject _portraitPrev;

	[SerializeField]
	private GameObject _portraitBgBtn;

	[SerializeField]
	private GameObject _portraitBgColorBtn;

	[SerializeField]
	private UISprite _portraitBgColorSprite;

	[SerializeField]
	private UIWidget _colorsWidget;

	[SerializeField]
	private BodySizeSelector _bodySizeSelector;

	[SerializeField]
	private GameObject _nextButton;

	[SerializeField]
	private PrologueCustomButton _prevButton;

	private PlayerPreviewScene _playerPreviewScene;

	[SerializeField]
	private UIInput _textInput;

	[SerializeField]
	private GameObject _explainDetailButton;

	[SerializeField]
	private GameObject[] _stateGroup;

	private PortraitBuilder.Argument _portraitArgument;

	private int _gender = -1;

	private int _job = -1;

	private BodyModelType _bodyModelType = BodyModelType.Torn;

	private MessagePackObjectDictionary _lastCostumeInfo;

	private string _hairName;

	private string _beardName;

	private ItemColor[] _colors;

	private float _bodySize;

	private int _voiceType = -1;

	private IList<string> _voiceSamples;

	private State CurState
	{
		get
		{
			return _curState;
		}
		set
		{
			State curState = _curState;
			_curState = value;
			if (curState == State.SelectCostume)
			{
				_playerPreviewScene.RemovePlayer();
			}
			GameObject val = _stateGroup[(int)_curState];
			GameObject go = _stateGroup[(int)curState];
			val.SetActive(true);
			val.GetComponent<UIRect>().alpha = 0f;
			TweenAlpha tweenAlpha = UITweener.Begin<TweenAlpha>(val, 0.2f);
			tweenAlpha.from = 0f;
			tweenAlpha.to = 1f;
			tweenAlpha = UITweener.Begin<TweenAlpha>(go, 0.2f);
			tweenAlpha.from = 1f;
			tweenAlpha.to = 0f;
			KUtility.DelayedCall((MonoBehaviour)(object)this, StateGroupActiveRefresh, 0.8f);
			_stateGroup[(int)_curState].SetActive(true);
			if (_curState != 0)
			{
				((Component)_prevButton).gameObject.SetActive(true);
				_prevButton.SetText(T._("이전"));
			}
			else
			{
				((Component)_prevButton).gameObject.SetActive(false);
			}
			if (_curState >= State.SelectPrevJob)
			{
				_nextButton.gameObject.SetActive(true);
				_nextButton.GetComponentInChildren<UILabel>().text = T._("다음");
			}
			else
			{
				_nextButton.gameObject.SetActive(false);
			}
			((Component)_playerPreviewScene).gameObject.SetActive(false);
			switch (_curState)
			{
			case State.SelectName:
				_nextButton.GetComponentInChildren<UILabel>().text = T._("완료");
				break;
			case State.FirstState:
				break;
			case State.SelectPrevJob:
				RefreshPrevJobButtons();
				break;
			case State.SelectCostume:
				PrologueCustomButton.SetButtonsText(new string[5]
				{
					T._("머리모양"),
					T._("외형 변경"),
					T._("새로 선택"),
					T._("수염"),
					T._("목소리")
				}, new PrologueCustomButton[5] { _hairShapeBtn, _randomBtn, _prevButton, _breadShapeBtn, _voiceBtn });
				PrologueCustomButton.SetButtonsText(new string[7]
				{
					T._("머리색"),
					T._("피부색"),
					T._("의상색1"),
					T._("의상색2"),
					T._("의상색3"),
					T._("눈색"),
					T._("입술색")
				}, new PrologueCustomButton[7]
				{
					_hairColorBtn,
					_skinColorBtn,
					_costumeColorBtns[0],
					_costumeColorBtns[1],
					_costumeColorBtns[2],
					_eyeColorBtn,
					_lipColorBtn
				});
				((Component)_breadShapeBtn).gameObject.SetActive(IsMale);
				((Component)_playerPreviewScene).gameObject.SetActive(true);
				_playerPreviewScene.CreatePlayer(IsMale);
				if (_lastCostumeInfo == null)
				{
					if (_colors == null)
					{
						_playerPreviewScene.SelectRandomCostume();
					}
					else
					{
						_playerPreviewScene.SetCostumeColors(_colors);
						_playerPreviewScene.ChangeHair(_hairName);
						if (IsMale)
						{
							_playerPreviewScene.ChangeBeard(_beardName);
						}
						_playerPreviewScene.ChangeBodySize((!(_bodySize > 0f)) ? 1f : _bodySize);
					}
				}
				else
				{
					_playerPreviewScene.SetCostume(_lastCostumeInfo, randomFillEmptyProperties: true);
					_playerPreviewScene.ChangeBodySize(1f);
				}
				ChangeBodyModelType(BodyModelType.Normal);
				_bodySizeSelector.Set(_playerPreviewScene.PreviewPlayer.BodySize);
				UpdatePortraitTexture();
				LoadVoiceSample(IsMale);
				SetVoiceType((_voiceType != -1) ? _voiceType : Random.Range(0, _voiceSamples.Count - 1), playSample: false);
				break;
			}
		}
	}

	private bool IsMale => _gender == 0;

	private void OnEnable()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		TweenAlpha component = ((Component)this).GetComponent<TweenAlpha>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.ResetToBeginning();
			component.PlayForward();
		}
		_gender = Random.Range(0, 2);
		_job = Random.Range(0, NormalBodyModels.Length);
		_lastCostumeInfo = KSingleton<PrologueManager>.Instance().LastCostumeInfo;
		if (_lastCostumeInfo != null)
		{
			_gender = ((!KSingleton<PrologueManager>.Instance().LastGender) ? 1 : 0);
			MessagePackObject val = default(MessagePackObject);
			if (_lastCostumeInfo.TryGetValue(MessagePackObject.op_Implicit("body"), ref val))
			{
				string text = ((MessagePackObject)(ref val)).AsString();
				int i = 0;
				for (int num = NormalBodyModels.Length; i < num; i++)
				{
					if (text.Contains(NormalBodyModels[i]))
					{
						_job = i;
						break;
					}
				}
			}
		}
		CurState = State.SelectCostume;
	}

	private void Awake()
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int num = _genderButtons.Length; i < num; i++)
		{
			UIEventListener uIEventListener = UIEventListener.Get(((Component)_genderButtons[i]).gameObject);
			uIEventListener.onClick = GenderButtons_OnClick;
		}
		List<string> list = new List<string>(Enum.GetNames(typeof(Shared.Player.Job)));
		list.Remove(Shared.Player.Job.Invalid.ToString());
		_prevJobButtons = new DefaultSelectableButton[list.Count];
		int j = 0;
		for (int count = list.Count; j < count; j++)
		{
			DefaultSelectableButton component = ((Component)((Component)_prevJobButtonBase).transform.parent).gameObject.AddChild(((Component)_prevJobButtonBase).gameObject).GetComponent<DefaultSelectableButton>();
			((Component)component).gameObject.SetActive(true);
			((Component)component).transform.localPosition = ((float)(j * _prevJobButtonWidth) - (float)(_prevJobButtonWidth * (count - 1)) / 2f) * Vector3.right;
			_prevJobButtons[j] = component;
		}
		((Component)_prevJobButtonBase).gameObject.SetActive(false);
		int k = 0;
		for (int num2 = _prevJobButtons.Length; k < num2; k++)
		{
			DefaultSelectableButton obj = _prevJobButtons[k];
			obj.Clicked = (Action)Delegate.Combine(obj.Clicked, new Action(PrevJobButtons_OnClick));
		}
		int l = 0;
		for (int num3 = _stateGroup.Length; l < num3; l++)
		{
			_stateGroup[l].SetActive(false);
		}
		UIEventListener.Get(((Component)_prevButton).gameObject).onClick = delegate
		{
			OnPrev();
		};
		UIEventListener.Get(_nextButton).onClick = delegate
		{
			OnNext();
		};
		UIEventListener.Get(((Component)_randomBtn).gameObject).onClick = OnSelectRandomCostume;
		UIEventListener.Get(((Component)_hairShapeBtn).gameObject).onClick = OnClickHairShapeChange;
		UIEventListener.Get(((Component)_hairColorBtn).gameObject).onClick = OnClickHairColorChange;
		UIEventListener.Get(((Component)_breadShapeBtn).gameObject).onClick = OnClickBreadShapeChange;
		UIEventListener.Get(((Component)_voiceBtn).gameObject).onClick = OnClickVoiceChange;
		UIEventListener.Get(((Component)_skinColorBtn).gameObject).onClick = OnClickSkinColorChange;
		int m = 0;
		for (int num4 = _costumeColorBtns.Length; m < num4; m++)
		{
			UIEventListener.Get(((Component)_costumeColorBtns[m]).gameObject).onClick = OnClickCostumeColorChange;
		}
		UIEventListener.Get(((Component)_eyeColorBtn).gameObject).onClick = OnClickEyeColorChange;
		UIEventListener.Get(((Component)_lipColorBtn).gameObject).onClick = OnClickLipColorChange;
		UIEventListener.Get(_portraitNext).onClick = OnClickPortraitChange;
		UIEventListener.Get(_portraitPrev).onClick = OnClickPortraitChange;
		UIEventListener.Get(_portraitBgBtn).onClick = OnClickPortraitBgChange;
		UIEventListener.Get(_portraitBgColorBtn).onClick = OnClickPortraitBgColorChange;
		UIEventListener.Get(_explainDetailButton).onClick = OnClickExplainDetail;
		_tornBodyButton.Clicked = OnClickTornBodyButton;
		_nothingBodyButton.Clicked = OnClickNothingBodyButton;
		PrologueCustomButton[] array = new PrologueCustomButton[5] { _prevButton, _randomBtn, _hairShapeBtn, _breadShapeBtn, _voiceBtn };
		for (int n = 0; n < array.Length; n++)
		{
			UIEventListener.Get(((Component)array[n]).gameObject).onPress = OnTouchButton;
			OnTouchButton(((Component)array[n]).gameObject, press: false);
		}
		UIEventListener.Get(_previewTouchBox).onDrag = OnDragPreviewPlayer;
		_playerPreviewScene = Object.FindObjectOfType<PlayerPreviewScene>();
		_playerPreviewScene.ColorChanged += PlayerPreviewScene_ColorChanged;
		_portraitArgument = PortraitBuilder.MakeArgument(0, IsMale, PortraitEmotion.Normal);
		PortraitBuilder.Set(_portraitArgument, _portraitWidget);
		_bodySizeSelector.Init(0.85f, 1.1f, 1f);
		_bodySizeSelector.ValueChanged += OnChangeBodySize;
	}

	private string GetBodyModelPath()
	{
		return _bodyModelType switch
		{
			BodyModelType.Normal => NormalBodyModels[_job], 
			BodyModelType.Torn => $"{NormalBodyModels[_job]}_torn", 
			BodyModelType.Nothing => "inner_basic", 
			_ => null, 
		};
	}

	private CharacterCostume.SkinDirty GetSkinDirtyLevel()
	{
		return (_bodyModelType == BodyModelType.Torn) ? CharacterCostume.SkinDirty.Dirty : CharacterCostume.SkinDirty.Clean;
	}

	private void PlayerPreviewScene_ColorChanged(CharacterCostume.CostumeType type, ItemColor color)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		switch (type)
		{
		case CharacterCostume.CostumeType.Body:
		{
			for (int i = 0; i < 3; i++)
			{
				_costumeColorBtns[i].SetColor(color[i]);
			}
			break;
		}
		case CharacterCostume.CostumeType.Hair:
			_hairColorBtn.SetColor(color[0]);
			_portraitArgument.Hair = color[0];
			UpdatePortraitTexture();
			break;
		case CharacterCostume.CostumeType.Skin:
			_skinColorBtn.SetColor(color[0]);
			_portraitArgument.Skin = color[0];
			UpdatePortraitTexture();
			break;
		case CharacterCostume.CostumeType.Eye:
			_eyeColorBtn.SetColor(color[0]);
			_portraitArgument.Eye = color[0];
			UpdatePortraitTexture();
			break;
		case CharacterCostume.CostumeType.Lip:
			_lipColorBtn.SetColor(color[0]);
			_portraitArgument.Lip = color[0];
			UpdatePortraitTexture();
			break;
		case CharacterCostume.CostumeType.Head:
		case CharacterCostume.CostumeType.Beard:
			break;
		}
	}

	private void UpdatePortraitTexture()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		_portraitArgument.Male = IsMale;
		PortraitBuilder.Set(_portraitArgument, _portraitWidget);
		_portraitBgColorSprite.color = _portraitArgument.BgColor;
	}

	private void StateGroupActiveRefresh()
	{
		foreach (object value in Enum.GetValues(typeof(State)))
		{
			_stateGroup[(int)value].SetActive((int)value == (int)_curState);
		}
	}

	private void GenderButtons_OnClick(GameObject obj)
	{
		for (int i = 0; i < _genderButtons.Length; i++)
		{
			if ((Object)(object)obj == (Object)(object)((Component)_genderButtons[i]).gameObject)
			{
				_gender = i;
				break;
			}
		}
		OnNext();
	}

	private void PrevJobButtons_OnClick()
	{
		for (int i = 0; i < _prevJobButtons.Length; i++)
		{
			if ((Object)(object)Selectable.Current == (Object)(object)_prevJobButtons[i])
			{
				_prevJobButtons[i].Select = true;
				_job = i;
			}
			else
			{
				_prevJobButtons[i].Select = false;
			}
		}
		Shared.Player.Job job = (Shared.Player.Job)_job;
		Yaml.Job job2 = SingletonDict<Shared.Player.Job, Yaml.Job>.Get(job);
		if (job2 == null)
		{
			_skillCategoryTitleLabel.text = string.Empty;
			_skillCategoryDescriptionLabel.text = string.Empty;
			return;
		}
		if (KUtility.GetSize(job2.category_levels) == 0)
		{
			_skillCategoryTitleLabel.text = T._("스킬 없음");
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
				Category key = category_level.Key;
				stringBuilder.AppendFormat("[{0}:1.5]", SkillUtil.CategoryIcon(key));
				stringBuilder.Append(T._("{1:lv:} {0}", SkillUtil.CategoryLocalizeName(key), category_level.Value));
			}
			_skillCategoryTitleLabel.text = stringBuilder.ToString().Trim();
		}
		_skillCategoryDescriptionLabel.text = job2.description;
	}

	private void RefreshPrevJobButtons()
	{
		string arg = ((_gender != 0) ? "f" : "m");
		for (int i = 0; i < _prevJobButtons.Length; i++)
		{
			Shared.Player.Job job = (Shared.Player.Job)i;
			UISprite component = ((Component)((Component)_prevJobButtons[i]).transform.FindChild("Preview")).GetComponent<UISprite>();
			component.spriteName = $"img_{job.ToString().ToLower()}_{arg}";
			_prevJobButtons[i].Text = LocalizeUtil.Get(job);
			_prevJobButtons[i].Select = false;
		}
		_skillCategoryTitleLabel.text = string.Empty;
		_skillCategoryDescriptionLabel.text = string.Empty;
		_job = -1;
	}

	public void OnChangeName()
	{
		string text = Regex.Replace(Regex.Replace(_textInput.value, "[\\[|\\]]", string.Empty), "\\s\\s+", " ");
		if (HasKorean(text) && text.Length > 10)
		{
			_textInput.value = text.Substring(0, 10);
		}
		else if (text.Length != _textInput.value.Length)
		{
			_textInput.value = text;
		}
	}

	private void OnPrev()
	{
		switch (CurState)
		{
		case State.SelectName:
			CurState = State.SelectCostume;
			break;
		case State.FirstState:
			break;
		case State.SelectPrevJob:
			CurState = State.FirstState;
			break;
		case State.SelectCostume:
			CurState = State.FirstState;
			_lastCostumeInfo = null;
			_hairName = string.Empty;
			_beardName = string.Empty;
			_colors = null;
			_bodySize = 1f;
			break;
		}
	}

	private static bool HasKorean(string value)
	{
		int length = value.Length;
		for (int i = 0; i < length; i++)
		{
			if (char.GetUnicodeCategory(value[i]) == UnicodeCategory.OtherLetter)
			{
				return true;
			}
		}
		return false;
	}

	private void OnNext()
	{
		switch (CurState)
		{
		case State.SelectName:
			Finish();
			break;
		case State.FirstState:
			if (_gender != -1)
			{
				CurState = State.SelectPrevJob;
			}
			break;
		case State.SelectPrevJob:
			if (_job != -1)
			{
				CurState = State.SelectCostume;
			}
			break;
		case State.SelectCostume:
			_playerPreviewScene.GetCostumeInfo(out _hairName, out _beardName, out _colors, out _bodySize);
			_lastCostumeInfo = null;
			CurState = State.SelectName;
			break;
		}
	}

	private void Finish()
	{
		string userName = _textInput.value.Trim();
		if (Debug.isDebugBuild && string.IsNullOrEmpty(userName))
		{
			int num = Random.Range(10, 15);
			char[] array = new char[num];
			for (int i = 0; i < num; i++)
			{
				if (Random.Range(0, 2) == 0)
				{
					array[i] = (char)Random.Range(97, 122);
				}
				else
				{
					array[i] = (char)Random.Range(65, 90);
				}
			}
			userName = new string(array);
		}
		if (string.IsNullOrEmpty(userName))
		{
			return;
		}
		UIManager.MessageBox.Show(T._("[b][fad257]{0}[-][/b]{0:-으로} 캐릭터를 만드시겠습니까?", userName), delegate(bool ok)
		{
			if (ok)
			{
				KSingleton<PrologueManager>.Instance().FinishCreateCharacter(IsMale, _job, userName, _hairName, _beardName, _colors, _portraitArgument, _voiceType + 1, _bodySize);
			}
		});
	}

	private void LoadVoiceSample(bool isMale)
	{
		_voiceSamples = PlayerVoice.GetSampleVoices(isMale);
	}

	private void SetVoiceType(int type, bool playSample = true)
	{
		_voiceType = type;
		_voiceBtn.Text = string.Format("{0} {1}", T._("목소리"), type + 1);
		if (playSample)
		{
			SoundManager.Play(_voiceSamples[_voiceType]);
		}
	}

	private void OnClickHairShapeChange(GameObject go)
	{
		string nextHair = _playerPreviewScene.GetNextHair(IsMale);
		_playerPreviewScene.ChangeHair(nextHair);
	}

	private void OnClickBreadShapeChange(GameObject go)
	{
		string nextBeard = _playerPreviewScene.GetNextBeard(IsMale);
		_playerPreviewScene.ChangeBeard(nextBeard);
	}

	private void OnClickVoiceChange(GameObject go)
	{
		int voiceType = _voiceType;
		voiceType = (voiceType + 1) % _voiceSamples.Count;
		SetVoiceType(voiceType);
	}

	private void OnClickHairColorChange(GameObject go)
	{
		PopupColorSelector(0);
	}

	private void OnClickSkinColorChange(GameObject go)
	{
		PopupColorSelector(1);
	}

	private void OnClickCostumeColorChange(GameObject go)
	{
		int num = -1;
		int i = 0;
		for (int num2 = ((_costumeColorBtns != null) ? _costumeColorBtns.Length : 0); i < num2; i++)
		{
			if (_costumeColorBtns != null && (Object)(object)((Component)_costumeColorBtns[i]).gameObject == (Object)(object)go)
			{
				num = i;
				break;
			}
		}
		PopupColorSelector(num + 2);
	}

	private void OnClickEyeColorChange(GameObject go)
	{
		PopupColorSelector(5);
	}

	private void OnClickLipColorChange(GameObject go)
	{
		PopupColorSelector(6);
	}

	private void OnClickPortraitChange(GameObject go)
	{
		if ((Object)(object)go == (Object)(object)_portraitNext)
		{
			ChangeNextPortrait();
		}
		else if ((Object)(object)go == (Object)(object)_portraitPrev)
		{
			ChangeNextPortrait(isPrev: true);
		}
	}

	private void ChangeNextPortrait(bool isPrev = false)
	{
		int portraitCount = KSingleton<PortraitBuilder>.Instance().GetPortraitCount(IsMale);
		if (portraitCount > 1)
		{
			if (isPrev)
			{
				_portraitArgument.Type += portraitCount - 1;
			}
			else
			{
				_portraitArgument.Type++;
			}
			_portraitArgument.Type %= portraitCount;
			UpdatePortraitTexture();
		}
	}

	private void OnClickPortraitBgChange(GameObject go)
	{
		int portraitBgCount = KSingleton<PortraitBuilder>.Instance().GetPortraitBgCount();
		_portraitArgument.Background++;
		_portraitArgument.Background %= portraitBgCount;
		UpdatePortraitTexture();
	}

	private void OnClickPortraitBgColorChange(GameObject go)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		ColorSelectorPopup colorSelectorPopup = UIManager.Popup.Tooltip<ColorSelectorPopup>();
		Color[] colors = ColorTable.ReadColorTable("color_portrait_bg.raw");
		Color bgColor = _portraitArgument.BgColor;
		colorSelectorPopup.Set(colors, bgColor, OnSelectPortraitBgColor);
		colorSelectorPopup.Show(3600f);
		((Component)_colorsWidget).gameObject.SetActive(false);
		((Component)_bodySizeSelector).gameObject.SetActive(false);
		colorSelectorPopup.AddOnFinished(delegate
		{
			((Component)_colorsWidget).gameObject.SetActive(true);
			((Component)_bodySizeSelector).gameObject.SetActive(true);
		});
	}

	private void OnClickExplainDetail(GameObject go)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, T._("<em>띄어쓰기와 특수문자 제한</em>\n처음과 마지막 글자가 될 수 없습니다.\n띄어쓰기는 한 번만 사용할 수 있습니다.\n특수문자는 연이어 쓸 수 없습니다.\n"));
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Show(go, new Vector2(36f, -180f), 10f);
	}

	private void OnClickNothingBodyButton()
	{
		ChangeBodyModelType((_bodyModelType != BodyModelType.Nothing) ? BodyModelType.Nothing : BodyModelType.Normal);
	}

	private void OnClickTornBodyButton()
	{
		ChangeBodyModelType((_bodyModelType != BodyModelType.Torn) ? BodyModelType.Torn : BodyModelType.Normal);
	}

	private void ChangeBodyModelType(BodyModelType type)
	{
		_bodyModelType = type;
		_tornBodyButton.Select = type == BodyModelType.Torn;
		_nothingBodyButton.Select = type == BodyModelType.Nothing;
		_playerPreviewScene.ChangeBody(GetBodyModelPath());
		_playerPreviewScene.PreviewPlayer.SkinDirtyLevel = GetSkinDirtyLevel();
		if (Random.value < 0.2f)
		{
			_playerPreviewScene.PreviewPlayer.PlayAnimation("Avatar_Dress");
		}
	}

	private void OnChangeBodySize(float ratio)
	{
		_playerPreviewScene.ChangeBodySize(ratio);
	}

	private void OnSelectRandomCostume(GameObject go)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		_playerPreviewScene.SelectRandomCostume();
		_bodySizeSelector.Set(_playerPreviewScene.PreviewPlayer.BodySize);
		int portraitCount = KSingleton<PortraitBuilder>.Instance().GetPortraitCount(IsMale);
		_portraitArgument.Type = Random.Range(0, portraitCount);
		int portraitBgCount = KSingleton<PortraitBuilder>.Instance().GetPortraitBgCount();
		_portraitArgument.Background = Random.Range(0, portraitBgCount);
		Color[] array = ColorTable.ReadColorTable("color_portrait_bg.raw");
		_portraitArgument.BgColor = array[Random.Range(0, array.Length)];
		UpdatePortraitTexture();
	}

	private void OnTouchButton(GameObject go, bool press)
	{
		PrologueCustomButton component = go.GetComponent<PrologueCustomButton>();
		if (!((Object)(object)component == (Object)null))
		{
			component.PressAnimation(press);
		}
	}

	private void PopupColorSelector(int tabIndex)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		Color[][] array = new Color[7][];
		Color[] array2 = (Color[])(object)new Color[7];
		string[] array3 = new string[7];
		int num = 0;
		array[num] = ColorTable.ReadColorTable("color_hair.raw");
		ref Color reference = ref array2[num];
		reference = _hairColorBtn.GetColor();
		array3[num] = _hairColorBtn.GetText();
		num++;
		array[num] = ColorTable.ReadColorTable("color_skin.raw");
		ref Color reference2 = ref array2[num];
		reference2 = _skinColorBtn.GetColor();
		array3[num] = _skinColorBtn.GetText();
		num++;
		for (int i = 0; i < _costumeColorBtns.Length; i++)
		{
			array[num] = ColorTable.ReadColorTable("color_create.raw");
			ref Color reference3 = ref array2[num];
			reference3 = _costumeColorBtns[i].GetColor();
			array3[num] = _costumeColorBtns[i].GetText();
			num++;
		}
		array[num] = ColorTable.ReadColorTable("color_eyes.raw");
		ref Color reference4 = ref array2[num];
		reference4 = _eyeColorBtn.GetColor();
		array3[num] = _eyeColorBtn.GetText();
		num++;
		array[num] = ColorTable.ReadColorTable((!IsMale) ? "color_lips_female.raw" : "color_lips_male.raw");
		ref Color reference5 = ref array2[num];
		reference5 = _lipColorBtn.GetColor();
		array3[num] = _lipColorBtn.GetText();
		ColorSelectorPopup colorSelectorPopup = UIManager.Popup.Tooltip<ColorSelectorPopup>();
		colorSelectorPopup.Set(array, array2, array3, tabIndex, OnSelectColorPalette);
		colorSelectorPopup.Show(3600f);
		((Component)_colorsWidget).gameObject.SetActive(false);
		((Component)_bodySizeSelector).gameObject.SetActive(false);
		colorSelectorPopup.AddOnFinished(delegate
		{
			((Component)_colorsWidget).gameObject.SetActive(true);
			((Component)_bodySizeSelector).gameObject.SetActive(true);
		});
	}

	private void OnSelectColorPalette(int index, Color color)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		switch (index)
		{
		case 0:
			_playerPreviewScene.ChangeCostumeColor(CharacterCostume.CostumeType.Hair, new ItemColor(color));
			break;
		case 1:
			_playerPreviewScene.ChangeCostumeColor(CharacterCostume.CostumeType.Skin, new ItemColor(color));
			break;
		case 2:
		case 3:
		case 4:
		{
			ItemColor color2 = new ItemColor(_costumeColorBtns[0].GetColor(), _costumeColorBtns[1].GetColor(), _costumeColorBtns[2].GetColor());
			color2[index - 2] = color;
			_playerPreviewScene.ChangeCostumeColor(CharacterCostume.CostumeType.Body, color2);
			break;
		}
		case 5:
			_playerPreviewScene.ChangeCostumeColor(CharacterCostume.CostumeType.Eye, new ItemColor(color));
			break;
		case 6:
			_playerPreviewScene.ChangeCostumeColor(CharacterCostume.CostumeType.Lip, new ItemColor(color));
			break;
		}
	}

	private void OnSelectPortraitBgColor(int index, Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_portraitArgument.BgColor = color;
		UpdatePortraitTexture();
	}

	private void OnDragPreviewPlayer(GameObject go, Vector2 delta)
	{
		if (Mathf.Abs(delta.x) > 0f)
		{
			_playerPreviewScene.PlayerRotate(delta.x);
		}
		if (Mathf.Abs(delta.y) > 0f)
		{
			_playerPreviewScene.PlayerMoveY(delta.y);
		}
	}
}
