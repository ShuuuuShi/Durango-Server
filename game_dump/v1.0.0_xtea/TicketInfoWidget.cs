using System;
using L10N;
using Messages;
using Ticket;
using UnityEngine;

public class TicketInfoWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _tierSprite;

	[SerializeField]
	private GameObject _tierHelpButton;

	[SerializeField]
	private TicketTierHelpWidget _tierHelpWidget;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private Selectable _reticketButton;

	[SerializeField]
	private GameObject _pointHelpButton;

	[SerializeField]
	private UILabel _myPointLabel;

	[SerializeField]
	private UILabel _friendPointLabel;

	[SerializeField]
	private UILabel _totalPointLabel;

	public event Action Reticketed;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_tierHelpButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickTierHelpButton));
		UIEventListener uIEventListener2 = UIEventListener.Get(_pointHelpButton);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickPointHelpButton));
		_tierHelpWidget.Set(TicketGroup.TierMetas);
		((Component)_tierHelpWidget).gameObject.SetActive(false);
		Selectable reticketButton = _reticketButton;
		reticketButton.Clicked = (Action)Delegate.Combine(reticketButton.Clicked, (Action)delegate
		{
			if (this.Reticketed != null)
			{
				this.Reticketed();
			}
		});
	}

	public void Set(TicketSales sales)
	{
		TicketGroup.SetTierIcon(_tierSprite, sales.Tier);
		TierMeta tierMeta = TicketGroup.GetTierMeta(sales.Tier);
		_nameLabel.text = T._(tierMeta.Name);
		_descriptionLabel.text = T._("다음 티어까지 <em>{0}</em> 포인트 필요", sales.RemainedScore);
		_reticketButton.Disable = !sales.Reissuable;
		_myPointLabel.text = sales.Score.ToString();
		_friendPointLabel.text = sales.Subscore.ToString();
		_totalPointLabel.text = (sales.Score + sales.Subscore).ToString();
	}

	private void OnClickTierHelpButton(GameObject obj)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Horizontal;
		widgetTooltipControl.Set(_tierHelpWidget.Widget, null, null, null);
		widgetTooltipControl.Show(_tierHelpButton, Vector2.right * 10f, 3600f);
	}

	private void OnClickPointHelpButton(GameObject obj)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Horizontal;
		widgetTooltipControl.Set(null, T._("<em>총 포인트</em> = [현재 라운드 포인트] + [활성화된 친구 티어 포인트]"));
		widgetTooltipControl.Show(((Component)_pointHelpButton.transform.parent).gameObject, Vector2.zero, 3600f);
	}
}
