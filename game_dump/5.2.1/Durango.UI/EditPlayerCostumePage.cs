using System;
using System.Collections.Generic;
using Durango.Player;
using Durango.Prologue;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class EditPlayerCostumePage : MonoBehaviour, IEditPlayerDisplayPage
{
	private enum MainTab
	{
		Portrait,
		Display,
		Color
	}

	private enum SubTab
	{
		Portarit,
		Pattern,
		BgColor,
		Hair,
		Beard,
		Voice,
		HairColor,
		SkinColor,
		Body1Color,
		Body2Color,
		Body3Color,
		EyeColor,
		LipColor
	}

	[SerializeField]
	private Transform _modelViewer;

	[SerializeField]
	private Selectable _portraitButton;

	[SerializeField]
	private Selectable _displayButton;

	[SerializeField]
	private Selectable _colorButton;

	[SerializeField]
	private BinaryToggleSlider _genderSelector;

	[SerializeField]
	private SelectionMarker _mainTabSelectionMaker;

	[SerializeField]
	private UITexture _portraitWidget;

	[SerializeField]
	private GameObject _randomButton;

	[SerializeField]
	private KScrollView _subtabView;

	[SerializeField]
	private SelectionMarker _subTabSelectionMaker;

	[SerializeField]
	private ScalableGaugeAddon _bodySizeSelector;

	[SerializeField]
	private SelectableWidget _normalBodyButton;

	[SerializeField]
	private SelectableWidget _tornBodyButton;

	[SerializeField]
	private SelectableWidget _nudeBodyButton;

	[SerializeField]
	private SelectableButton _transitionButton;

	[SerializeField]
	private KGridScrollView _textureListView;

	[SerializeField]
	private SelectionMarker _textureSelectionMaker;

	[SerializeField]
	private KScrollView _textListView;

	[SerializeField]
	private SelectionMarker _textSelectionMaker;

	[SerializeField]
	private HexagonScrollView _colorListView;

	[SerializeField]
	private SelectionMarker _colorSelectionMaker;

	private NodesScrollView _currentListScroll;

	private SelectionMarker _currentSelectionMaker;

	private int _currentSelectedIndex;

	private Color[] _portraitBgColors;

	private List<PlayerCostumeTable.PreviewableDatum> _maleHairPreviewTextures;

	private List<PlayerCostumeTable.PreviewableDatum> _femaleHairPreviewTextures;

	private List<PlayerCostumeTable.PreviewableDatum> _beardPreviewTextures;

	private Color[] _colorArray;

	private Selectable[] _mainTabs;

	private SelectableWidget[] _bodyModelTypes;

	private MainTab? _selectedMainTab;

	private SubTab _selectedSubTab;

	private SubTab[] _subTabs;

	private EditPlayerDisplayProxy _display;

	private AnimationWidget _animWidget;

	public bool CanEditCostumeColor { get; set; }

	public bool CanEditGender { get; set; }

	public event Action Confirmed;

	public void Initialize(EditPlayerDisplayProxy display)
	{
		_display = display;
		_animWidget = AnimationWidget.Get(base.gameObject, 0.3f, 0f, deactiveWhenFadeout: true);
		Observable<int> portrait = _display.Portrait;
		portrait.Changed = (Action<int>)Delegate.Combine(portrait.Changed, (Action<int>)delegate(int value)
		{
			UpdatePortrait();
			SelectPortraitTexture(value);
		});
		Observable<int> portraitBg = _display.PortraitBg;
		portraitBg.Changed = (Action<int>)Delegate.Combine(portraitBg.Changed, (Action<int>)delegate(int value)
		{
			UpdatePortrait();
			SelectPortraitPattern(value);
		});
		Observable<Color> portraitBgColor = _display.PortraitBgColor;
		portraitBgColor.Changed = (Action<Color>)Delegate.Combine(portraitBgColor.Changed, (Action<Color>)delegate(Color value)
		{
			UpdatePortrait();
			SelectPortraitColor(value);
			RefreshSubTabs();
		});
		Observable<string> hair = _display.Hair;
		hair.Changed = (Action<string>)Delegate.Combine(hair.Changed, new Action<string>(SelectHair));
		Observable<string> beard = _display.Beard;
		beard.Changed = (Action<string>)Delegate.Combine(beard.Changed, new Action<string>(SelectBeard));
		Observable<int> voiceType = _display.VoiceType;
		voiceType.Changed = (Action<int>)Delegate.Combine(voiceType.Changed, new Action<int>(SelectVoice));
		Observable<Color> hairColor = _display.HairColor;
		hairColor.Changed = (Action<Color>)Delegate.Combine(hairColor.Changed, (Action<Color>)delegate(Color value)
		{
			UpdatePortrait();
			SelectColor(SubTab.HairColor, value);
			RefreshSubTabs();
		});
		Observable<Color> bodyColor = _display.BodyColor1;
		bodyColor.Changed = (Action<Color>)Delegate.Combine(bodyColor.Changed, (Action<Color>)delegate(Color value)
		{
			UpdatePortrait();
			SelectColor(SubTab.Body1Color, value);
			RefreshSubTabs();
		});
		Observable<Color> bodyColor2 = _display.BodyColor2;
		bodyColor2.Changed = (Action<Color>)Delegate.Combine(bodyColor2.Changed, (Action<Color>)delegate(Color value)
		{
			UpdatePortrait();
			SelectColor(SubTab.Body2Color, value);
			RefreshSubTabs();
		});
		Observable<Color> bodyColor3 = _display.BodyColor3;
		bodyColor3.Changed = (Action<Color>)Delegate.Combine(bodyColor3.Changed, (Action<Color>)delegate(Color value)
		{
			UpdatePortrait();
			SelectColor(SubTab.Body3Color, value);
			RefreshSubTabs();
		});
		Observable<Color> skinColor = _display.SkinColor;
		skinColor.Changed = (Action<Color>)Delegate.Combine(skinColor.Changed, (Action<Color>)delegate(Color value)
		{
			UpdatePortrait();
			SelectColor(SubTab.SkinColor, value);
			RefreshSubTabs();
		});
		Observable<Color> eyeColor = _display.EyeColor;
		eyeColor.Changed = (Action<Color>)Delegate.Combine(eyeColor.Changed, (Action<Color>)delegate(Color value)
		{
			UpdatePortrait();
			SelectColor(SubTab.EyeColor, value);
			RefreshSubTabs();
		});
		Observable<Color> lipColor = _display.LipColor;
		lipColor.Changed = (Action<Color>)Delegate.Combine(lipColor.Changed, (Action<Color>)delegate(Color value)
		{
			UpdatePortrait();
			SelectColor(SubTab.LipColor, value);
			RefreshSubTabs();
		});
		Observable<float> bodySize = _display.BodySize;
		bodySize.Changed = (Action<float>)Delegate.Combine(bodySize.Changed, (Action<float>)delegate
		{
			UpdateBodySize();
		});
		Observable<bool> gender = _display.Gender;
		gender.Changed = (Action<bool>)Delegate.Combine(gender.Changed, (Action<bool>)delegate
		{
			UpdatePortrait();
			_selectedMainTab = null;
			SelectMainTab(MainTab.Portrait);
			SetBodyClothState(PlayerCostumeTable.ClothState.Normal);
		});
		UIEventListener uIEventListener = UIEventListener.Get(_randomButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			_display.RandomCostume();
		});
		Observable<bool> gender2 = _display.Gender;
		gender2.Changed = (Action<bool>)Delegate.Combine(gender2.Changed, (Action<bool>)delegate(bool isMale)
		{
			_genderSelector.Set((!isMale) ? 1f : 0f, sendEvent: false, playAnimation: true);
		});
		BinaryToggleSlider genderSelector = _genderSelector;
		genderSelector.ValueChanged = (Action<bool>)Delegate.Combine(genderSelector.ValueChanged, (Action<bool>)delegate(bool isFemale)
		{
			_display.Gender.Value = !isFemale;
		});
		_subtabView.Nodes.Init(delegate(GameObject obj)
		{
			Selectable component = obj.GetComponent<Selectable>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickSubTab));
		});
		_transitionButton.SetClickSound(UISound.ClickType.ButtonHighlight);
		SelectableButton transitionButton = _transitionButton;
		transitionButton.Clicked = (Action)Delegate.Combine(transitionButton.Clicked, new Action(OnConfirm));
		_transitionButton.Text = T._("다음");
		_bodySizeSelector.ValueChanged = BodySizeChanged;
		_bodyModelTypes = new SelectableWidget[3] { _normalBodyButton, _tornBodyButton, _nudeBodyButton };
		SelectableWidget normalBodyButton = _normalBodyButton;
		normalBodyButton.Clicked = (Action)Delegate.Combine(normalBodyButton.Clicked, (Action)delegate
		{
			SetBodyClothState(PlayerCostumeTable.ClothState.Normal);
		});
		SelectableWidget tornBodyButton = _tornBodyButton;
		tornBodyButton.Clicked = (Action)Delegate.Combine(tornBodyButton.Clicked, (Action)delegate
		{
			SetBodyClothState(PlayerCostumeTable.ClothState.Torn);
		});
		SelectableWidget nudeBodyButton = _nudeBodyButton;
		nudeBodyButton.Clicked = (Action)Delegate.Combine(nudeBodyButton.Clicked, (Action)delegate
		{
			SetBodyClothState(PlayerCostumeTable.ClothState.Nothing);
		});
		_mainTabs = new Selectable[3] { _portraitButton, _displayButton, _colorButton };
		for (int i = 0; i < _mainTabs.Length; i++)
		{
			Selectable obj2 = _mainTabs[i];
			obj2.Clicked = (Action)Delegate.Combine(obj2.Clicked, new Action(OnClickMainTab));
		}
		_bodySizeSelector.Init(0.85f, 1.1f, 0f);
		_textureListView.Nodes.Init(ListItemInitialize);
		_textListView.Nodes.Init(ListItemInitialize);
		_colorListView.Nodes.Init(ListItemInitialize);
		_portraitBgColors = ColorTableLoader.GetAll("color_portrait_bg.raw");
		PlayerCostumeTable playerCostumeTable = ResourceSingleton<PlayerCostumeTable>.Instance();
		_maleHairPreviewTextures = playerCostumeTable.GetDataArray(PlayerCostumeTable.Category.Hair, dataIsMale: true);
		_femaleHairPreviewTextures = playerCostumeTable.GetDataArray(PlayerCostumeTable.Category.Hair, dataIsMale: false);
		_beardPreviewTextures = playerCostumeTable.GetDataArray(PlayerCostumeTable.Category.Beard, dataIsMale: true);
	}

	public void Show(bool instant)
	{
		UpdateBodyStateButtons();
		if (CanEditGender)
		{
			_genderSelector.gameObject.SetActive(value: true);
			_genderSelector.Set((!_display.Gender) ? 1f : 0f);
		}
		else
		{
			_genderSelector.gameObject.SetActive(value: false);
		}
		base.gameObject.SetActive(value: true);
		_animWidget.SetAlpha(1f, !instant);
		_selectedMainTab = null;
		SelectMainTab(MainTab.Portrait);
		SetBodyClothState(PlayerCostumeTable.ClothState.Normal);
		UpdatePortrait();
		UpdateBodySize();
	}

	public void Hide(bool instant)
	{
		_animWidget.SetAlpha(0f, !instant);
	}

	public Transform GetModelPosition()
	{
		return _modelViewer;
	}

	public void SetConfirmText(string text)
	{
		_transitionButton.Text = text;
	}

	public void WaitForLoading(bool loading)
	{
		UIManager.ShowLoadingIcon(loading);
		_transitionButton.Disabled = loading;
	}

	private void OnConfirm()
	{
		if (this.Confirmed != null)
		{
			this.Confirmed();
		}
	}

	private void UpdatePortrait()
	{
		PortraitBuilder.Set(_display.GetPortraitArgument(), _portraitWidget);
	}

	private void UpdateBodySize()
	{
		_bodySizeSelector.Set(_display.BodySize, raiseEvent: false, playAnimation: true);
	}

	private void OnClickMainTab()
	{
		int num = Array.IndexOf(_mainTabs, Selectable.Current);
		if (num != -1)
		{
			SelectMainTab((MainTab)num);
		}
	}

	private void OnClickSubTab()
	{
		int num = _subtabView.Nodes.IndexOf(Selectable.Current.gameObject);
		if (num != -1)
		{
			SelectSubTab(_subTabs[num]);
		}
	}

	private void SelectMainTab(MainTab tab)
	{
		if (_selectedMainTab.HasValue && _selectedMainTab.Value == tab)
		{
			return;
		}
		_selectedMainTab = tab;
		for (int i = 0; i < _mainTabs.Length; i++)
		{
			bool flag = i == (int)tab;
			_mainTabs[i].Selected = flag;
			if (flag)
			{
				_mainTabSelectionMaker.Set(_mainTabs[i].Widget);
			}
		}
		switch (tab)
		{
		case MainTab.Portrait:
			MakeSubTabs(SubTab.Portarit, SubTab.Pattern, SubTab.BgColor);
			break;
		case MainTab.Display:
			if ((bool)_display.Gender)
			{
				MakeSubTabs(SubTab.Hair, SubTab.Beard, SubTab.Voice);
			}
			else
			{
				MakeSubTabs(SubTab.Hair, SubTab.Voice);
			}
			break;
		case MainTab.Color:
			if (CanEditCostumeColor)
			{
				MakeSubTabs(SubTab.HairColor, SubTab.SkinColor, SubTab.Body1Color, SubTab.Body2Color, SubTab.Body3Color, SubTab.EyeColor, SubTab.LipColor);
			}
			else
			{
				MakeSubTabs(SubTab.HairColor, SubTab.SkinColor, SubTab.EyeColor, SubTab.LipColor);
			}
			break;
		}
	}

	private void SelectSubTab(SubTab tab)
	{
		if (KUtility.GetSize(_subTabs) == 0)
		{
			return;
		}
		int num = Array.IndexOf(_subTabs, tab);
		if (num == -1)
		{
			tab = _subTabs[0];
			num = 0;
		}
		_selectedSubTab = tab;
		for (int i = 0; i < _subtabView.Nodes.Count; i++)
		{
			bool flag = i == num;
			SelectableWidget component = _subtabView.Nodes[i].GetComponent<SelectableWidget>();
			component.Selected = flag;
			if (flag)
			{
				_subTabSelectionMaker.Set(component.Widget);
			}
		}
		switch (tab)
		{
		case SubTab.Portarit:
			ShowPortraitTextureList();
			break;
		case SubTab.Pattern:
			ShowPortraitPatternList();
			break;
		case SubTab.BgColor:
			ShowPortraitColorList();
			break;
		case SubTab.Hair:
			ShowHairList();
			break;
		case SubTab.Beard:
			ShowBeardList();
			break;
		case SubTab.Voice:
			ShowVoiceList();
			break;
		case SubTab.HairColor:
		case SubTab.SkinColor:
		case SubTab.Body1Color:
		case SubTab.Body2Color:
		case SubTab.Body3Color:
		case SubTab.EyeColor:
		case SubTab.LipColor:
			ShowColorList(tab);
			break;
		}
	}

	private void MakeSubTabs(params SubTab[] tabs)
	{
		_subTabs = tabs;
		_subtabView.Nodes.Set(KUtility.GetSize(tabs));
		_subtabView.ResetPosition();
		RefreshSubTabs();
		SelectSubTab(tabs[0]);
	}

	private void RefreshSubTabs()
	{
		for (int i = 0; i < _subtabView.Nodes.Count; i++)
		{
			GameObject obj = _subtabView.Nodes[i];
			SetSubTab(obj, _subTabs[i]);
		}
	}

	private void SetSubTab(GameObject obj, SubTab tab)
	{
		string text = null;
		string text2 = null;
		Color? color = null;
		switch (tab)
		{
		case SubTab.Portarit:
			text = T._("초상화");
			text2 = "icon_face";
			break;
		case SubTab.Pattern:
			text = T._("패턴");
			text2 = "icon_pattern";
			break;
		case SubTab.BgColor:
			text = T._("배경색");
			color = _display.PortraitBgColor;
			break;
		case SubTab.Hair:
			text = T._("머리");
			text2 = "icon_hair";
			break;
		case SubTab.Beard:
			text = T._("수염");
			text2 = "icon_beard";
			break;
		case SubTab.Voice:
			text = T._("목소리");
			text2 = "icon_voice";
			break;
		case SubTab.HairColor:
			text = T._("머리색");
			color = _display.HairColor;
			break;
		case SubTab.SkinColor:
			text = T._("피부색");
			color = _display.SkinColor;
			break;
		case SubTab.Body1Color:
			text = T._("의상색1");
			color = _display.BodyColor1;
			break;
		case SubTab.Body2Color:
			text = T._("의상색2");
			color = _display.BodyColor2;
			break;
		case SubTab.Body3Color:
			text = T._("의상색3");
			color = _display.BodyColor3;
			break;
		case SubTab.EyeColor:
			text = T._("눈색");
			color = _display.EyeColor;
			break;
		case SubTab.LipColor:
			text = T._("입술색");
			color = _display.LipColor;
			break;
		}
		UILabel component = obj.transform.Find("Text").GetComponent<UILabel>();
		UISprite component2 = obj.transform.Find("Image").GetComponent<UISprite>();
		UISprite component3 = obj.transform.Find("Bg").GetComponent<UISprite>();
		component.text = text;
		if (string.IsNullOrEmpty(text2))
		{
			component2.gameObject.SetActive(value: false);
		}
		else
		{
			component2.gameObject.SetActive(value: true);
			component2.spriteName = text2;
		}
		component3.color = ((!color.HasValue) ? new Color(0f, 0f, 0f, 0.7f) : color.Value);
	}

	private void UpdateCurrentListSelection()
	{
		if (_currentListScroll == null)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < _currentListScroll.Nodes.Count; i++)
		{
			GameObject gameObject = _currentListScroll.Nodes[i];
			Selectable component = gameObject.GetComponent<Selectable>();
			if ((bool)component)
			{
				component.Selected = i == _currentSelectedIndex;
			}
			if (i == _currentSelectedIndex)
			{
				_currentSelectionMaker.Set(gameObject.GetComponent<UIWidget>());
				flag = false;
			}
		}
		if (flag)
		{
			_currentSelectionMaker.gameObject.SetActive(value: false);
		}
	}

	private void SetTextureList(IList<Texture> textures)
	{
		_textureListView.gameObject.SetActive(value: true);
		_textListView.gameObject.SetActive(value: false);
		_colorListView.gameObject.SetActive(value: false);
		ListObjectPool nodes = _textureListView.Nodes;
		nodes.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(textures); i < size; i++)
		{
			UITexture component = nodes.GetNext().transform.Find("Texture").GetComponent<UITexture>();
			component.material = null;
			component.mainTexture = textures[i];
		}
		nodes.EndLoad();
		_textureListView.ResetPosition();
		_currentListScroll = _textureListView;
		_currentSelectionMaker = _textureSelectionMaker;
	}

	private void SetTextureList(IList<PlayerCostumeTable.PreviewableDatum> list)
	{
		_textureListView.gameObject.SetActive(value: true);
		_textListView.gameObject.SetActive(value: false);
		_colorListView.gameObject.SetActive(value: false);
		ListObjectPool nodes = _textureListView.Nodes;
		nodes.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(list); i < size; i++)
		{
			UITexture component = nodes.GetNext().transform.Find("Texture").GetComponent<UITexture>();
			component.material = null;
			component.mainTexture = list[i].PreviewTexture;
		}
		nodes.EndLoad();
		_textureListView.ResetPosition();
		_currentListScroll = _textureListView;
		_currentSelectionMaker = _textureSelectionMaker;
	}

	private void SetTextureList(IList<Material> materials)
	{
		_textureListView.gameObject.SetActive(value: true);
		_textListView.gameObject.SetActive(value: false);
		_colorListView.gameObject.SetActive(value: false);
		ListObjectPool nodes = _textureListView.Nodes;
		nodes.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(materials); i < size; i++)
		{
			UITexture component = nodes.GetNext().transform.Find("Texture").GetComponent<UITexture>();
			component.mainTexture = null;
			component.material = materials[i];
		}
		nodes.EndLoad();
		_textureListView.ResetPosition();
		_currentListScroll = _textureListView;
		_currentSelectionMaker = _textureSelectionMaker;
	}

	private void SetTextList(IList<string> texts)
	{
		_textureListView.gameObject.SetActive(value: false);
		_textListView.gameObject.SetActive(value: true);
		_colorListView.gameObject.SetActive(value: false);
		ListObjectPool nodes = _textListView.Nodes;
		nodes.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(texts); i < size; i++)
		{
			nodes.GetNext().transform.Find("Text").GetComponent<UILabel>().text = texts[i];
		}
		nodes.EndLoad();
		_textListView.ResetPosition();
		_currentListScroll = _textListView;
		_currentSelectionMaker = _textSelectionMaker;
	}

	private void SetColorList(IList<Color> colors)
	{
		_textureListView.gameObject.SetActive(value: false);
		_textListView.gameObject.SetActive(value: false);
		_colorListView.gameObject.SetActive(value: true);
		ListObjectPool nodes = _colorListView.Nodes;
		nodes.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(colors); i < size; i++)
		{
			nodes.GetNext().transform.Find("Bg").GetComponent<UISprite>().color = colors[i];
		}
		nodes.EndLoad();
		_colorListView.ResetPosition();
		_currentListScroll = _colorListView;
		_currentSelectionMaker = _colorSelectionMaker;
	}

	private void UpdateBodyStateButtons()
	{
		_normalBodyButton.gameObject.SetActive(!string.IsNullOrEmpty(_display.DefaultBody));
		_tornBodyButton.gameObject.SetActive(!string.IsNullOrEmpty(_display.TornBody));
		_nudeBodyButton.gameObject.SetActive(!string.IsNullOrEmpty(_display.NudeBody));
	}

	private void SetBodyClothState(PlayerCostumeTable.ClothState targetBodyModel)
	{
		_display.SetClothState(targetBodyModel);
		for (int i = 0; i < _bodyModelTypes.Length; i++)
		{
			_bodyModelTypes[i].Selected = i == (int)targetBodyModel;
		}
	}

	private void BodySizeChanged(float ratio)
	{
		_display.BodySize.Value = ratio;
	}

	private void ListItemInitialize(GameObject obj)
	{
		UIEventListener uIEventListener = UIEventListener.Get(obj);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickListItem));
	}

	private void OnClickListItem(GameObject obj)
	{
		int num = 0;
		for (int i = 0; i < _currentListScroll.GetNodeCount(); i++)
		{
			if (_currentListScroll.GetNode(i).gameObject == obj)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			ListItemSelected(num);
		}
	}

	private void ListItemSelected(int index)
	{
		switch (_selectedSubTab)
		{
		case SubTab.Portarit:
			_display.Portrait.Value = index;
			break;
		case SubTab.Pattern:
			_display.PortraitBg.Value = index;
			break;
		case SubTab.BgColor:
			_display.PortraitBgColor.Value = _portraitBgColors[index];
			break;
		case SubTab.Hair:
			_display.Hair.Value = ((!_display.Gender) ? _femaleHairPreviewTextures[index].AssetBundlePathBase : _maleHairPreviewTextures[index].AssetBundlePathBase);
			break;
		case SubTab.Beard:
			_display.Beard.Value = ((!_display.Gender) ? string.Empty : _beardPreviewTextures[index].AssetBundlePathBase);
			break;
		case SubTab.Voice:
			_display.VoiceType.Value = index + 1;
			SoundManager.PlayEvent("character_select", SoundPosition.Empty, PlayerManager.GetVoiceSoundSwitch(_display.Gender, _display.VoiceType));
			break;
		case SubTab.HairColor:
			_display.HairColor.Value = _colorArray[index];
			break;
		case SubTab.SkinColor:
			_display.SkinColor.Value = _colorArray[index];
			break;
		case SubTab.Body1Color:
			_display.BodyColor1.Value = _colorArray[index];
			break;
		case SubTab.Body2Color:
			_display.BodyColor2.Value = _colorArray[index];
			break;
		case SubTab.Body3Color:
			_display.BodyColor3.Value = _colorArray[index];
			break;
		case SubTab.EyeColor:
			_display.EyeColor.Value = _colorArray[index];
			break;
		case SubTab.LipColor:
			_display.LipColor.Value = _colorArray[index];
			break;
		}
	}

	private void ShowPortraitTextureList()
	{
		PortraitBuilder.PortraitTexturesGroup[] collection = ((!_display.Gender) ? ResourceSingleton<PortraitBuilder>.Instance()._femaleTextureGroup : ResourceSingleton<PortraitBuilder>.Instance()._maleTextureGroup);
		List<Material> list = new List<Material>();
		PortraitBuilder.Argument portraitArgument = _display.GetPortraitArgument();
		int i = 0;
		for (int size = KUtility.GetSize(collection); i < size; i++)
		{
			portraitArgument.Type = i;
			Material item = PortraitBuilder.CreateMaterial(portraitArgument);
			list.Add(item);
		}
		SetTextureList(list);
		SelectPortraitTexture(_display.Portrait);
	}

	private void ShowPortraitPatternList()
	{
		Texture[] bgTextures = ResourceSingleton<PortraitBuilder>.Instance()._bgTextures;
		SetTextureList(bgTextures);
		SelectPortraitPattern(_display.PortraitBg);
	}

	private void ShowPortraitColorList()
	{
		Color[] portraitBgColors = _portraitBgColors;
		SetColorList(portraitBgColors);
		SelectPortraitColor(_display.PortraitBgColor);
	}

	private void ShowHairList()
	{
		List<PlayerCostumeTable.PreviewableDatum> textureList = ((!_display.Gender) ? _femaleHairPreviewTextures : _maleHairPreviewTextures);
		SetTextureList(textureList);
		SelectHair(_display.Hair);
	}

	private void ShowBeardList()
	{
		List<PlayerCostumeTable.PreviewableDatum> beardPreviewTextures = _beardPreviewTextures;
		SetTextureList(beardPreviewTextures);
		SelectBeard(_display.Beard);
	}

	private void ShowVoiceList()
	{
		int num = ((!_display.Gender) ? 7 : 7);
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = T._("목소리 {0}", i + 1);
		}
		SetTextList(array);
		SelectVoice(_display.VoiceType);
	}

	private void ShowColorList(SubTab tab)
	{
		Color color;
		switch (tab)
		{
		default:
			return;
		case SubTab.HairColor:
			color = _display.HairColor;
			break;
		case SubTab.SkinColor:
			color = _display.SkinColor;
			break;
		case SubTab.Body1Color:
			color = _display.BodyColor1;
			break;
		case SubTab.Body2Color:
			color = _display.BodyColor2;
			break;
		case SubTab.Body3Color:
			color = _display.BodyColor3;
			break;
		case SubTab.EyeColor:
			color = _display.EyeColor;
			break;
		case SubTab.LipColor:
			color = _display.LipColor;
			break;
		}
		_colorArray = GetModelColorPallete(tab);
		SetColorList(_colorArray);
		SelectColor(tab, color);
	}

	private void SelectPortraitTexture(int type)
	{
		if (_selectedSubTab == SubTab.Portarit)
		{
			_currentSelectedIndex = type;
			UpdateCurrentListSelection();
		}
	}

	private void SelectPortraitPattern(int type)
	{
		if (_selectedSubTab == SubTab.Pattern)
		{
			_currentSelectedIndex = type;
			UpdateCurrentListSelection();
		}
	}

	private void SelectPortraitColor(Color color)
	{
		if (_selectedSubTab == SubTab.BgColor)
		{
			_currentSelectedIndex = Array.IndexOf(_portraitBgColors, color);
			UpdateCurrentListSelection();
		}
	}

	private void SelectHair(string hair)
	{
		if (_selectedSubTab != SubTab.Hair)
		{
			return;
		}
		List<PlayerCostumeTable.PreviewableDatum> list = ((!_display.Gender) ? _femaleHairPreviewTextures : _maleHairPreviewTextures);
		int currentSelectedIndex = -1;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].AssetBundlePathBase == hair)
			{
				currentSelectedIndex = i;
				break;
			}
		}
		_currentSelectedIndex = currentSelectedIndex;
		UpdateCurrentListSelection();
	}

	private void SelectBeard(string beard)
	{
		if (_selectedSubTab != SubTab.Beard)
		{
			return;
		}
		List<PlayerCostumeTable.PreviewableDatum> beardPreviewTextures = _beardPreviewTextures;
		int currentSelectedIndex = -1;
		for (int i = 0; i < beardPreviewTextures.Count; i++)
		{
			if (beardPreviewTextures[i].AssetBundlePathBase == beard)
			{
				currentSelectedIndex = i;
				break;
			}
		}
		_currentSelectedIndex = currentSelectedIndex;
		UpdateCurrentListSelection();
	}

	private void SelectVoice(int type)
	{
		if (_selectedSubTab == SubTab.Voice)
		{
			_currentSelectedIndex = type - 1;
			UpdateCurrentListSelection();
		}
	}

	private void SelectColor(SubTab tabType, Color color)
	{
		if (_selectedSubTab == tabType)
		{
			int currentSelectedIndex = Array.IndexOf(_colorArray, color);
			_currentSelectedIndex = currentSelectedIndex;
			UpdateCurrentListSelection();
		}
	}

	private Color[] GetModelColorPallete(SubTab type)
	{
		string tableName;
		switch (type)
		{
		case SubTab.HairColor:
			tableName = "color_hair.raw";
			break;
		case SubTab.SkinColor:
			tableName = "color_skin.raw";
			break;
		case SubTab.Body1Color:
		case SubTab.Body2Color:
		case SubTab.Body3Color:
			tableName = "color_create.raw";
			break;
		case SubTab.EyeColor:
			tableName = "color_eyes.raw";
			break;
		case SubTab.LipColor:
			tableName = ((!_display.Gender) ? "color_lips_female.raw" : "color_lips_male.raw");
			break;
		default:
			return new Color[1] { Color.black };
		}
		return ColorTableLoader.GetAll(tableName);
	}
}
