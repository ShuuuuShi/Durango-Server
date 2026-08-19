using System;
using UnityEngine;

public class AutoGuideGroup : UIBase, INewCheckerable
{
	[SerializeField]
	private GameObject _closeButton;

	[SerializeField]
	private LoadingIndicatorWidget _loadingWidget;

	[SerializeField]
	private AutoGuideTitleSelectPage _titleSelectPage;

	[SerializeField]
	private AutoGuideTemplatePage _currentTemplatePage;

	private readonly NewCheckerCountableNode _newChecker = new NewCheckerCountableNode();

	public NewChecker NewChecker => _newChecker;

	protected override bool OnClose()
	{
		if (((Component)_currentTemplatePage).gameObject.activeSelf && _currentTemplatePage.TryCloseInfoPopup())
		{
			return false;
		}
		return base.OnClose();
	}

	private void OnEnable()
	{
		GameSystem<AutoGuideSystem>.Instance().TemplateUpdated += AutoGuideSystem_TemplateUpdated;
		GameSystem<AutoGuideSystem>.Instance().ProgressUpdated += AutoGuideSystem_ProgressUpdated;
	}

	private void OnDisable()
	{
		GameSystem<AutoGuideSystem>.Instance().TemplateUpdated -= AutoGuideSystem_TemplateUpdated;
		GameSystem<AutoGuideSystem>.Instance().ProgressUpdated -= AutoGuideSystem_ProgressUpdated;
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_closeButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			Close();
		});
		base.OnOpenSucceed += AutoGuideGroup_OnOpenSucceed;
		OnClose();
	}

	private void CurrentTemplatePage_TemplateSelected()
	{
		AutoGuideSystem autoGuideSystem = GameSystem<AutoGuideSystem>.Instance();
		_newChecker.Count = autoGuideSystem.GetNewTemplateCount();
	}

	private void AutoGuideGroup_OnOpenSucceed()
	{
		GameSystem<AutoGuideSystem>.Instance().UpdateAchievementRatio();
		AutoGuideSystem_TemplateUpdated();
	}

	private void AutoGuideSystem_TemplateUpdated()
	{
		AutoGuideSystem autoGuideSystem = GameSystem<AutoGuideSystem>.Instance();
		_newChecker.Count = autoGuideSystem.GetNewTemplateCount();
		if (base.IsOpen)
		{
			bool isWaitingResponse = autoGuideSystem.IsWaitingResponse;
			((Component)_loadingWidget).gameObject.SetActive(isWaitingResponse);
			if (isWaitingResponse)
			{
				_titleSelectPage.Show(visible: false);
				_currentTemplatePage.Show(visible: false);
			}
			else if (autoGuideSystem.TargetTitle == null)
			{
				_titleSelectPage.Show(visible: true);
				_currentTemplatePage.Show(visible: false);
			}
			else
			{
				_currentTemplatePage.Show(visible: true);
				_titleSelectPage.Show(visible: false);
			}
		}
	}

	private void AutoGuideSystem_ProgressUpdated()
	{
		if (base.IsOpen)
		{
			_currentTemplatePage.SetProgress();
		}
	}
}
