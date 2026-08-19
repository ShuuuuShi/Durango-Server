using System;
using System.Collections.Generic;
using L10N;
using Shared.Ability;
using StatisticsData;
using UnityEngine;

public class CharacterTitlePage : MonoBehaviour
{
	public Action<Title> TitleSelected;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private UIWidget _infoWidget;

	[SerializeField]
	private ComboBox _titleComboBox;

	[SerializeField]
	private UILabel _titleDescriptionLabel;

	[SerializeField]
	private UIWidget _effectInfoWidget;

	[SerializeField]
	private UILabel _titleEffectLabel;

	[SerializeField]
	private UIWidget _abilityEffectWidget;

	[SerializeField]
	private AbilityWidget[] _abilityWidgets;

	[SerializeField]
	private int _statEffectPerLine = 4;

	[SerializeField]
	private TweenerPlayer _showAnimation;

	private List<Title> _titles = new List<Title>();

	private bool _isPlayShowAnimation;

	private UIWidget _invisibleBox;

	private void Awake()
	{
		_titleComboBox.ItemSelected = delegate(int index)
		{
			if (TitleSelected != null)
			{
				TitleSelected(_titles[index]);
			}
		};
	}

	private void OnEnable()
	{
		_invisibleBox = UIUtility.SetScrollViewInvisibleBox(_scrollView, _invisibleBox);
		_showAnimation.ResetToBeginning();
	}

	private void OnDisable()
	{
		_isPlayShowAnimation = false;
	}

	public void SetTitleComboBox(int currentIndex, IList<Title> titles)
	{
		int num = titles?.Count ?? 0;
		Title title = ((currentIndex >= num || currentIndex < 0) ? null : titles[currentIndex]);
		_titles.Clear();
		_titles.Add(null);
		for (int i = 0; i < num; i++)
		{
			if (titles[i].Enabled)
			{
				_titles.Add(titles[i]);
			}
		}
		string[] array = new string[_titles.Count];
		for (int j = 0; j < _titles.Count; j++)
		{
			Title title2 = _titles[j];
			if (title2 == null)
			{
				array[j] = T._("없음");
			}
			else
			{
				array[j] = title2.Name;
			}
		}
		_titleComboBox.Set(array);
		SetTitle(title);
	}

	public void SetTitle(Title title)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Color color = UIManager.UIWhite;
		string text;
		string text2;
		if (title == null)
		{
			text = T._("없음");
			text2 = string.Empty;
		}
		else
		{
			text = title.Name;
			text2 = title.Description;
			color = ((!title.Enabled) ? UIManager.UIRed : UIManager.UIYellow);
		}
		_titleComboBox.SetLabel(text, color);
		bool flag = !string.IsNullOrEmpty(text2);
		float num = 0f - ((Component)_titleDescriptionLabel).transform.localPosition.y;
		if (flag)
		{
			_titleDescriptionLabel.text = text2;
			((Component)_titleDescriptionLabel).gameObject.SetActive(true);
			num += (float)(_titleDescriptionLabel.height + 20);
		}
		else
		{
			((Component)_titleDescriptionLabel).gameObject.SetActive(false);
		}
		_infoWidget.height = (int)num;
		SetTitleEffect(title);
		RefreshLayout();
	}

	private void SetTitleEffect(Title title)
	{
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		KeyValuePair<Basic, string>[] array = new KeyValuePair<Basic, string>[Statistics.PhysicalAbility.Length];
		int i = 0;
		for (int num2 = array.Length; i < num2; i++)
		{
			Basic key = Statistics.PhysicalAbility[i];
			int num3 = title?.GetAbility(key) ?? 0;
			ref KeyValuePair<Basic, string> reference = ref array[i];
			reference = new KeyValuePair<Basic, string>(key, (num3 != 0) ? $"+{num3}" : string.Empty);
			if (num3 > 0)
			{
				num++;
			}
		}
		KeyValuePair<Basic, string>[] array2 = new KeyValuePair<Basic, string>[Statistics.MentalAbility.Length];
		int j = 0;
		for (int num4 = array2.Length; j < num4; j++)
		{
			Basic key2 = Statistics.MentalAbility[j];
			int num5 = title?.GetAbility(key2) ?? 0;
			ref KeyValuePair<Basic, string> reference2 = ref array2[j];
			reference2 = new KeyValuePair<Basic, string>(key2, (num5 != 0) ? $"+{num5}" : string.Empty);
		}
		if (num > 0)
		{
			_abilityWidgets[0].Set(array);
			_abilityWidgets[1].Set(array2);
			int num6 = Mathf.Max(_abilityWidgets[0].Widget.height, _abilityWidgets[1].Widget.height);
			_abilityEffectWidget.height = num6 + (int)Mathf.Abs(_abilityWidgets[0].Widget.GetPosition(0f, 0f).y) + 5;
			UIUtility.UpdateAnchors(((Component)_abilityEffectWidget).transform);
			((Component)_abilityEffectWidget).gameObject.SetActive(true);
			((Component)_titleEffectLabel).gameObject.SetActive(false);
			_effectInfoWidget.height = _abilityEffectWidget.height;
		}
		else
		{
			((Component)_abilityEffectWidget).gameObject.SetActive(false);
			((Component)_titleEffectLabel).gameObject.SetActive(true);
			_titleEffectLabel.text = T._("아무런 효과가 없습니다");
			_effectInfoWidget.height = (int)Mathf.Abs(((Component)_titleEffectLabel).transform.localPosition.y * 2f - (float)_titleEffectLabel.height);
		}
	}

	private void RefreshLayout()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)_effectInfoWidget).transform.localPosition;
		localPosition.y = -_infoWidget.height;
		((Component)_effectInfoWidget).transform.localPosition = localPosition;
	}

	public void ShowAnimation()
	{
		if (!_isPlayShowAnimation)
		{
			_isPlayShowAnimation = true;
			_showAnimation.Play();
		}
	}
}
