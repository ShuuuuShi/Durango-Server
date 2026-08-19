using System;
using System.Collections.Generic;
using System.Text;
using Durango.Logic.Skill;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Shared.Player;
using Shared.Skill;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class EditPlayerPresetPage : MonoBehaviour, IEditPlayerDisplayPage
{
	[SerializeField]
	private Transform _modelViewer;

	[SerializeField]
	private RectLayoutComponent _infoWidget;

	[SerializeField]
	private UILabel _infoTitleLabel;

	[SerializeField]
	private UILabel _infoDescriptionLabel;

	[SerializeField]
	private UILabel _selectedJobLabel;

	[SerializeField]
	private KScrollView _presetScrollView;

	[SerializeField]
	private SelectionMarker _crosshairMarker;

	[SerializeField]
	private SelectableButton _transitionButton;

	[SerializeField]
	private BinaryToggleSlider _genderSelector;

	private EditPlayerDisplayProxy _display;

	private AnimationWidget _animWidget;

	public event Action Confirmed;

	public void Initialize(EditPlayerDisplayProxy display)
	{
		_display = display;
		_animWidget = AnimationWidget.Get(base.gameObject, 0.3f, 0f, deactiveWhenFadeout: true);
		Observable<bool> gender = _display.Gender;
		gender.Changed = (Action<bool>)Delegate.Combine(gender.Changed, (Action<bool>)delegate(bool isMale)
		{
			_genderSelector.Set((!isMale) ? 1f : 0f, sendEvent: false, playAnimation: true);
		});
		Observable<Shared.Player.Job?> job = _display.Job;
		job.Changed = (Action<Shared.Player.Job?>)Delegate.Combine(job.Changed, new Action<Shared.Player.Job?>(SelectJobNode));
		BinaryToggleSlider genderSelector = _genderSelector;
		genderSelector.ValueChanged = (Action<bool>)Delegate.Combine(genderSelector.ValueChanged, new Action<bool>(OnGenderChange));
		_transitionButton.SetClickSound(UISound.ClickType.ButtonHighlight);
		SelectableButton transitionButton = _transitionButton;
		transitionButton.Clicked = (Action)Delegate.Combine(transitionButton.Clicked, new Action(OnConfirm));
		_transitionButton.Text = T._("다음");
		_presetScrollView.Nodes.Init(delegate(GameObject obj)
		{
			UIEventListener uIEventListener = UIEventListener.Get(obj);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnPresetNodeClick));
		});
		Shared.Player.Job[] array = Enums<Shared.Player.Job>.Greater(Shared.Player.Job.Invalid);
		_presetScrollView.Nodes.BeginLoad();
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].GetName();
			GameObject next = _presetScrollView.Nodes.GetNext();
			UILabel component = next.transform.Find("Text").GetComponent<UILabel>();
			component.text = text;
		}
		_presetScrollView.Nodes.EndLoad();
		_presetScrollView.ResetPosition();
	}

	public void Show(bool instant)
	{
		base.gameObject.SetActive(value: true);
		_animWidget.SetAlpha(1f, !instant);
		SelectJobNode(_display.Job);
		_genderSelector.Set((!_display.Gender) ? 1f : 0f);
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

	private void OnGenderChange(bool isFemale)
	{
		_display.Gender.Value = !isFemale;
	}

	private void OnConfirm()
	{
		if (this.Confirmed != null)
		{
			this.Confirmed();
		}
	}

	private void OnPresetNodeClick(GameObject obj)
	{
		int num = _presetScrollView.Nodes.IndexOf(obj);
		if (num != -1)
		{
			Shared.Player.Job[] array = Enums<Shared.Player.Job>.Greater(Shared.Player.Job.Invalid);
			Shared.Player.Job value = array[num];
			_display.Job.Value = value;
		}
	}

	private void SelectJobNode(Shared.Player.Job? job)
	{
		if (!job.HasValue)
		{
			return;
		}
		ListObjectPool nodes = _presetScrollView.Nodes;
		bool flag = false;
		for (int i = 0; i < nodes.Count; i++)
		{
			SelectableWidget component = nodes[i].GetComponent<SelectableWidget>();
			if (i == (int)job.Value)
			{
				flag = true;
				component.Selected = true;
				_crosshairMarker.Set(component.Widget);
			}
			else
			{
				component.Selected = false;
			}
		}
		if (!flag)
		{
			_crosshairMarker.gameObject.SetActive(value: false);
		}
		_selectedJobLabel.text = ((Enum)(object)job).GetName();
		Yaml.Job job2 = SingletonDict<Shared.Player.Job, Yaml.Job>.Get(job.Value);
		if (job2 == null)
		{
			_infoTitleLabel.text = string.Empty;
			_infoDescriptionLabel.text = string.Empty;
			return;
		}
		if (KUtility.GetSize(job2.category_levels) == 0)
		{
			_infoTitleLabel.text = T._("스킬 없음");
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<Shared.Skill.Category, int> category_level in job2.category_levels)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				Shared.Skill.Category key = category_level.Key;
				stringBuilder.Append(T._("{1:lv:} {0}", Util.CategoryLocalizeName(key), category_level.Value));
			}
			_infoTitleLabel.text = stringBuilder.ToString();
		}
		_infoDescriptionLabel.text = job2.description;
		_infoWidget.UpdateLayout();
	}
}
