using System;
using Durango.Logic.Item;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ResearchPageWidget : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private ResearchTiersWidget _tiersPage;

	[SerializeField]
	private UIWidget _infoWidget;

	[SerializeField]
	private UILabel _informationLabel;

	[SerializeField]
	private SelectableButton _researchStartButton;

	[SerializeField]
	private UIWidget _emptyWidget;

	[SerializeField]
	private UILabel _emptyLabel;

	public event Action<string> ResearchStarted;

	void IUIInitializable.Init()
	{
		_informationLabel.text = T._("<alert_icon/> 동시에 1개의 효과만 얻을 수 있습니다.");
		_emptyLabel.text = T._("연구 가능한 목록이 없습니다");
		_tiersPage.ResearchSelected += OnResearchSelected;
		SelectableButton researchStartButton = _researchStartButton;
		researchStartButton.Clicked = (Action)Delegate.Combine(researchStartButton.Clicked, (Action)delegate
		{
			if (_researchStartButton.Disabled)
			{
				if (_tiersPage.RequiredPioneerGrade.HasValue)
				{
					UIManager.SystemMsg(T._("개인섬 개척도 {0} 부터 연구할 수 있습니다.", _tiersPage.RequiredPioneerGrade.Value));
				}
			}
			else if (this.ResearchStarted != null)
			{
				this.ResearchStarted(_tiersPage.SelectedResearch);
			}
		});
		_researchStartButton.CanClickWhenDisabled = true;
	}

	public void Set(AvailablePersonalResearch? msg, bool reset)
	{
		if (!msg.HasValue)
		{
			SetEmpty();
		}
		else
		{
			SetResearchList(msg.Value, reset);
		}
	}

	private void SetResearchList(AvailablePersonalResearch msg, bool reset)
	{
		if (!_tiersPage.Set(msg, reset))
		{
			SetEmpty();
			return;
		}
		_tiersPage.gameObject.SetActive(value: true);
		_infoWidget.gameObject.SetActive(value: true);
		_emptyWidget.gameObject.SetActive(value: false);
		OnResearchSelected();
	}

	private void SetEmpty()
	{
		_tiersPage.gameObject.SetActive(value: false);
		_infoWidget.gameObject.SetActive(value: false);
		_emptyWidget.gameObject.SetActive(value: true);
	}

	private void OnResearchSelected()
	{
		string selectedResearch = _tiersPage.SelectedResearch;
		int? requiredPioneerGrade = _tiersPage.RequiredPioneerGrade;
		PersonalResearch personalResearch = ((!string.IsNullOrEmpty(selectedResearch)) ? SingletonDict<string, PersonalResearch>.Get(selectedResearch) : null);
		if (personalResearch == null || requiredPioneerGrade.HasValue)
		{
			_researchStartButton.Text = T._("연구");
			_researchStartButton.Disabled = true;
		}
		else
		{
			_researchStartButton.Text = Durango.Logic.Item.Inventory.ToCurrencyButtonText(T._("연구"), personalResearch.Amount, personalResearch.Currency);
			_researchStartButton.Disabled = false;
		}
	}

	public void UpdateResearchState()
	{
		_tiersPage.Refresh();
	}
}
