using L10N;
using StatisticsData;
using UnityEngine;

public class AutoGuideTemplatePage : MonoBehaviour
{
	[SerializeField]
	private UILabel _titleName;

	[SerializeField]
	private UILabel _titleExplain;

	[SerializeField]
	private UISprite _titleIcon;

	[SerializeField]
	private UILabel _progressLabel;

	[SerializeField]
	private GameObject _infoButton;

	[SerializeField]
	private AutoGuideTemplateSelectWidget _selectWidget;

	[SerializeField]
	private AutoGuideTemplateDetailWidget _detailWidget;

	[SerializeField]
	private AutoGuideInfoPopup _infoPopup;

	private bool _initialized;

	public void Show(bool visible)
	{
		((Component)this).gameObject.SetActive(visible);
		if (visible)
		{
			Initialize();
			Title targetTitle = GameSystem<AutoGuideSystem>.Instance().TargetTitle;
			_titleName.text = targetTitle.Name;
			_titleExplain.text = $"[i]{targetTitle.Description}[/i]";
			_titleIcon.spriteName = targetTitle.Icon;
			SetProgress();
			_selectWidget.Set(GameSystem<AutoGuideSystem>.Instance().GetTemplates());
			((Component)_infoPopup).gameObject.SetActive(false);
		}
	}

	public void SetProgress()
	{
		_progressLabel.text = T._("나의 목표 달성율 <em>{0}%</em>", GameSystem<AutoGuideSystem>.Instance().Progress);
		if (((Behaviour)_infoPopup).isActiveAndEnabled)
		{
			_infoPopup.Show();
		}
	}

	public bool TryCloseInfoPopup()
	{
		bool activeSelf = ((Component)_infoPopup).gameObject.activeSelf;
		((Component)_infoPopup).gameObject.SetActive(false);
		return activeSelf;
	}

	private void Initialize()
	{
		if (!_initialized)
		{
			_selectWidget.Selected += SelectWidget_Selected;
			UIEventListener.Get(((Component)_titleName).gameObject).onClick = InfoButton_OnClick;
			UIEventListener.Get(_infoButton).onClick = InfoButton_OnClick;
			_initialized = true;
		}
	}

	private void SelectWidget_Selected()
	{
		_detailWidget.Set(_selectWidget.SelectedTemplate);
	}

	private void InfoButton_OnClick(GameObject go)
	{
		_infoPopup.Show();
	}
}
