using System;
using Durango.UI.Control;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Animal;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class DomesticCagePetInfoWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _contentsWidget;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private UIModelViewer _viewer;

	[SerializeField]
	private NestedPrefabLinker _ratioWidget;

	[SerializeField]
	private SelectableWidget _releaseButton;

	[SerializeField]
	private UIWidget _manageButtonsArea;

	[SerializeField]
	private SelectableWidget _domesticateButton;

	[SerializeField]
	private SelectableWidget _cancelDomesticateButton;

	[SerializeField]
	private SelectableWidget _feedButton;

	[SerializeField]
	private SelectableWidget _takeoutButton;

	[SerializeField]
	private SelectableButton _actionButton;

	[SerializeField]
	private UIWidget _emptyWidget;

	[SerializeField]
	private UILabel _emptyLabel;

	private DomesticationInfo _target;

	private UIWidget[] _domesticButtons;

	private bool _escapedView;

	public event Action<DomesticationInfo> Released;

	public event Action<DomesticationInfo> DomesticateStarted;

	public event Action<DomesticationInfo> DomesticateStoped;

	public event Action<DomesticationInfo> DomesticateFinished;

	public event Action<DomesticationInfo> ReinTookOut;

	public event Action<DomesticationInfo> OnFeed;

	private void Start()
	{
		SelectableWidget releaseButton = _releaseButton;
		releaseButton.Clicked = (Action)Delegate.Combine(releaseButton.Clicked, (Action)delegate
		{
			if (this.Released != null)
			{
				this.Released(_target);
			}
		});
		SelectableButton actionButton = _actionButton;
		actionButton.Clicked = (Action)Delegate.Combine(actionButton.Clicked, new Action(OnActionButtonClick));
		SelectableWidget domesticateButton = _domesticateButton;
		domesticateButton.Clicked = (Action)Delegate.Combine(domesticateButton.Clicked, (Action)delegate
		{
			if (this.DomesticateStarted != null)
			{
				this.DomesticateStarted(_target);
			}
		});
		SelectableWidget cancelDomesticateButton = _cancelDomesticateButton;
		cancelDomesticateButton.Clicked = (Action)Delegate.Combine(cancelDomesticateButton.Clicked, (Action)delegate
		{
			if (this.DomesticateStoped != null)
			{
				this.DomesticateStoped(_target);
			}
		});
		SelectableWidget feedButton = _feedButton;
		feedButton.Clicked = (Action)Delegate.Combine(feedButton.Clicked, (Action)delegate
		{
			if (this.OnFeed != null)
			{
				this.OnFeed(_target);
			}
		});
		SelectableWidget takeoutButton = _takeoutButton;
		takeoutButton.Clicked = (Action)Delegate.Combine(takeoutButton.Clicked, (Action)delegate
		{
			if (this.ReinTookOut != null)
			{
				this.ReinTookOut(_target);
			}
		});
		_domesticButtons = new UIWidget[4] { _domesticateButton.Widget, _cancelDomesticateButton.Widget, _feedButton.Widget, _takeoutButton.Widget };
	}

	public void Set(DomesticationInfo target)
	{
		_target = target;
		_emptyWidget.gameObject.SetActive(value: false);
		Animal animal = SingletonDict<int, Animal>.Get(target.EntityType);
		if (animal == null)
		{
			_contentsWidget.gameObject.SetActive(value: false);
			return;
		}
		UILabel nameLabel = _nameLabel;
		PetRank? rank = target.Rank;
		nameLabel.text = ((!rank.HasValue) ? animal.Name.ToString() : PetUtil.GetRankedName(animal.Name, target.Rank.Value));
		_infoLabel.text = ((!target.Domesticated) ? T._("{0:lv:}  <bar/>  {1}", target.Level, T._("성공률 {0:P0}", target.DomesticateSuccessRate)) : T._("{0:lv:}", target.Level));
		_viewer.SetPlainModel(animal.PrefabPath, new UIModelViewer.Arguments
		{
			CameraAngle = 35f,
			Rotation = 140f,
			Loaded = _viewer.DefaultAnimalPlay("idle", "stand")
		});
		_ratioWidget.Object.GetComponent<DomesticRatioWidget>().Set(target);
		RefreshButtons();
		_contentsWidget.gameObject.SetActive(value: true);
		_escapedView = false;
	}

	public void SetEmpty()
	{
		if (_escapedView)
		{
			SetEmpty(T._("[size=28]동물이 도망쳤습니다[/size]"));
		}
		else
		{
			SetEmpty(T._("새로운 동물을 넣어주세요.\n[size=22][FFFFFF90]포획된 동물을 길들이거나, 길들인 동물을 보관할 수 있습니다.[/size]"));
		}
	}

	public void SetEscaped()
	{
		_escapedView = true;
		SetEmpty();
	}

	private void SetEmpty(string text)
	{
		_contentsWidget.gameObject.SetActive(value: false);
		_emptyWidget.gameObject.SetActive(value: true);
		_emptyLabel.text = text;
	}

	public void PlayYammyAnimation()
	{
		_ratioWidget.Object.GetComponent<DomesticRatioWidget>().PlayYammyAnimation();
	}

	private void RefreshButtons()
	{
		switch (PetUtil.ConverInfoToStatus(_target))
		{
		case CageStatus.Wild:
			_manageButtonsArea.gameObject.SetActive(value: true);
			_domesticateButton.gameObject.SetActive(value: true);
			_cancelDomesticateButton.gameObject.SetActive(value: false);
			_feedButton.gameObject.SetActive(value: false);
			_takeoutButton.gameObject.SetActive(value: true);
			_actionButton.gameObject.SetActive(value: false);
			break;
		case CageStatus.InProgress:
			_manageButtonsArea.gameObject.SetActive(value: true);
			_domesticateButton.gameObject.SetActive(value: false);
			_cancelDomesticateButton.gameObject.SetActive(value: true);
			_feedButton.gameObject.SetActive(value: true);
			_takeoutButton.gameObject.SetActive(value: false);
			_actionButton.gameObject.SetActive(value: false);
			break;
		case CageStatus.Complete:
			_manageButtonsArea.gameObject.SetActive(value: false);
			_actionButton.gameObject.SetActive(value: true);
			_actionButton.Text = T._("결과 확인");
			_actionButton.SetEffect(PresetButton.Effect.Emphasis);
			break;
		case CageStatus.Domesticated:
			_manageButtonsArea.gameObject.SetActive(value: false);
			_actionButton.gameObject.SetActive(value: true);
			_actionButton.Text = T._("가방에 넣기");
			_actionButton.ClearEffect();
			break;
		}
		UIUtility.WidgetsReposition(_domesticButtons, Vector3.right, Vector3.zero, 0f, 0.5f);
	}

	private void OnActionButtonClick()
	{
		switch (PetUtil.ConverInfoToStatus(_target))
		{
		case CageStatus.Complete:
			if (this.DomesticateFinished != null)
			{
				this.DomesticateFinished(_target);
			}
			break;
		case CageStatus.Domesticated:
			if (this.ReinTookOut != null)
			{
				this.ReinTookOut(_target);
			}
			break;
		}
	}
}
